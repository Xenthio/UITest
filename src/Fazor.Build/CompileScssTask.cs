using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Fazor.Scss;

namespace Fazor.Build;

/// <summary>
/// MSBuild task to compile SCSS files to CSS
/// </summary>
public class CompileScssTask : Task
{
    /// <summary>
    /// SCSS files to compile
    /// </summary>
    [Required]
    public ITaskItem[] ScssFiles { get; set; } = Array.Empty<ITaskItem>();

    /// <summary>
    /// Output directory for CSS files
    /// </summary>
    [Required]
    public string? OutputPath { get; set; }

    /// <summary>
    /// Include paths for SCSS imports
    /// </summary>
    public string[]? IncludePaths { get; set; }

    /// <summary>
    /// Generated CSS files (output)
    /// </summary>
    [Output]
    public ITaskItem[] GeneratedFiles { get; set; } = Array.Empty<ITaskItem>();

    public override bool Execute()
    {
        try
        {
            Log.LogMessage(MessageImportance.High, $"Compiling {ScssFiles.Length} SCSS file(s)...");

            if (string.IsNullOrEmpty(OutputPath))
            {
                Log.LogError("OutputPath is required");
                return false;
            }

            // Ensure output directory exists
            Directory.CreateDirectory(OutputPath);

            var includePaths = IncludePaths != null ? new List<string>(IncludePaths) : null;
            var generatedFiles = new List<ITaskItem>();

            foreach (var scssFile in ScssFiles)
            {
                try
                {
                    var inputPath = scssFile.ItemSpec;
                    var filename = Path.GetFileName(inputPath);

                    // Skip partials (files starting with _)
                    if (filename.StartsWith("_"))
                    {
                        Log.LogMessage(MessageImportance.Low, $"Skipping partial {inputPath}");
                        continue;
                    }

                    Log.LogMessage(MessageImportance.Normal, $"Compiling {inputPath}");

                    // Read SCSS file
                    var scssContent = File.ReadAllText(inputPath);

                    // Compile to CSS
                    var css = ScssProcessor.CompileScss(scssContent, inputPath, includePaths);

                    // Create output file path
                    var outputFileName = Path.ChangeExtension(filename, ".css");
                    var outputFilePath = Path.Combine(OutputPath, outputFileName);

                    // Write CSS file
                    File.WriteAllText(outputFilePath, css);

                    var generatedItem = new TaskItem(outputFilePath);
                    generatedItem.SetMetadata("AutoGen", "true");
                    generatedFiles.Add(generatedItem);

                    Log.LogMessage(MessageImportance.Normal, $"Generated {outputFilePath}");
                }
                catch (ScssCompilationException ex)
                {
                    Log.LogError($"SCSS compilation error in {scssFile.ItemSpec}: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Log.LogError($"Error compiling {scssFile.ItemSpec}: {ex.Message}");
                    return false;
                }
            }

            GeneratedFiles = generatedFiles.ToArray();

            Log.LogMessage(MessageImportance.High, $"Successfully compiled {GeneratedFiles.Length} SCSS file(s)");
            return true;
        }
        catch (Exception ex)
        {
            Log.LogErrorFromException(ex);
            return false;
        }
    }
}
