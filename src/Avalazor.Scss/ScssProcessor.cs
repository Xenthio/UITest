using SharpScss;
using System;
using System.Collections.Generic;
using System.IO;

namespace Avalazor.Scss;

/// <summary>
/// SCSS to CSS compilation processor
/// </summary>
public static class ScssProcessor
{
    /// <summary>
    /// Compile SCSS to CSS
    /// </summary>
    /// <param name="scssContent">The SCSS content to compile</param>
    /// <param name="filename">The source filename for error reporting</param>
    /// <param name="includePaths">Additional include paths for @import statements</param>
    /// <returns>Compiled CSS</returns>
    public static string CompileScss(string scssContent, string filename = "input.scss", List<string>? includePaths = null)
    {
        var options = new ScssOptions
        {
            InputFile = filename,
            OutputStyle = ScssOutputStyle.Compressed
        };

        // Add include paths if provided
        if (includePaths != null && includePaths.Count > 0)
        {
            foreach (var path in includePaths)
            {
                options.IncludePaths.Add(path);
            }
        }

        try
        {
            var result = SharpScss.Scss.ConvertToCss(scssContent, options);
            return result.Css ?? string.Empty;
        }
        catch (ScssException ex)
        {
            throw new ScssCompilationException($"SCSS compilation error in {filename}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Compile an SCSS file to CSS
    /// </summary>
    /// <param name="inputPath">Path to the SCSS file</param>
    /// <param name="outputPath">Path where the CSS should be written</param>
    /// <param name="includePaths">Additional include paths for @import statements</param>
    public static void CompileScssFile(string inputPath, string? outputPath = null, List<string>? includePaths = null)
    {
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"SCSS file not found: {inputPath}");
        }

        var scssContent = File.ReadAllText(inputPath);
        var css = CompileScss(scssContent, inputPath, includePaths);

        // If no output path specified, use the same name with .css extension
        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = Path.ChangeExtension(inputPath, ".css");
        }

        // Ensure output directory exists
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        File.WriteAllText(outputPath, css);
    }
}

/// <summary>
/// Exception thrown when SCSS compilation fails
/// </summary>
public class ScssCompilationException : Exception
{
    public ScssCompilationException(string message) : base(message) { }
    public ScssCompilationException(string message, Exception innerException) : base(message, innerException) { }
}
