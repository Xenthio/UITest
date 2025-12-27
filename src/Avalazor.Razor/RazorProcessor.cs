using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Components;
using System.Collections.Generic;

namespace Avalazor.Razor;

/// <summary>
/// Razor to C# transpilation processor based on s&box implementation (MIT licensed)
/// Source: https://github.com/Facepunch/sbox-public/blob/master/engine/Sandbox.Razor/Razor.cs
/// </summary>
public static class RazorProcessor
{
    /// <summary>
    /// Generate C# code from a Razor file
    /// </summary>
    /// <param name="text">The Razor file content</param>
    /// <param name="filename">The file path/name</param>
    /// <param name="rootNamespace">The root namespace for the project</param>
    /// <param name="useFolderNamespacing">Whether to use folder-based namespacing</param>
    /// <returns>Generated C# code</returns>
    public static string GenerateFromSource(string text, string filename, string? rootNamespace = null, bool useFolderNamespacing = true)
    {
        // Get the class name from the file name
        var className = System.IO.Path.GetFileNameWithoutExtension(filename);
        if (className.EndsWith(".razor", System.StringComparison.OrdinalIgnoreCase))
        {
            className = className.Substring(0, className.Length - 6);
        }

        // If a root namespace is provided and the file doesn't have a namespace, inject one
        if (useFolderNamespacing)
        {
            text = AddNamespace(text, filename, rootNamespace);
        }

        var engine = GetEngine();

        RazorSourceDocument source = RazorSourceDocument.Create(text, filename);
        RazorCodeDocument code = engine.Process(source, FileKinds.Component, new List<RazorSourceDocument>(), new List<TagHelperDescriptor>());
        code.SetCodeGenerationOptions(RazorCodeGenerationOptions.Create(o =>
        {
            // Suppress line directives to avoid file not found errors during compilation
            o.SuppressMetadataSourceChecksumAttributes = true;
            o.SuppressPrimaryMethodBody = false;
            o.SuppressNullabilityEnforcement = true;
        }));

        RazorCSharpDocument document = code.GetCSharpDocument();
        
        // Post-process to fix the namespace and class name
        var generatedCode = document.GeneratedCode;
        var targetNamespace = ExtractNamespace(text);
        generatedCode = FixNamespaceAndClassName(generatedCode, targetNamespace, className);

        return generatedCode;
    }

    private static string ExtractNamespace(string text)
    {
        // Extract namespace from @namespace directive
        var lines = text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("@namespace "))
            {
                return trimmedLine.Substring("@namespace ".Length).Trim();
            }
        }
        return "__GeneratedComponent";
    }

    private static string FixNamespaceAndClassName(string generatedCode, string targetNamespace, string expectedClassName)
    {
        // Replace the auto-generated namespace with the target namespace
        // Pattern: namespace __GeneratedComponent
        generatedCode = System.Text.RegularExpressions.Regex.Replace(
            generatedCode,
            @"namespace __GeneratedComponent\b",
            $"namespace {targetNamespace}"
        );
        
        // Replace the auto-generated class name pattern with the expected class name
        // The pattern is: public partial class AspNetCore_[hash] : 
        generatedCode = System.Text.RegularExpressions.Regex.Replace(
            generatedCode, 
            @"public partial class AspNetCore_[a-f0-9]+ :",
            $"public partial class {expectedClassName} :"
        );
        
        // Add using directive for Microsoft.AspNetCore.Components.Rendering namespace
        // This is needed for the extension methods (AddLocation, OpenElement overloads)
        if (!generatedCode.Contains("using Microsoft.AspNetCore.Components.Rendering;"))
        {
            // Find the last using statement and insert after it
            var lastUsingIndex = generatedCode.LastIndexOf("using ");
            if (lastUsingIndex >= 0)
            {
                var nextNewline = generatedCode.IndexOf('\n', lastUsingIndex);
                if (nextNewline >= 0)
                {
                    generatedCode = generatedCode.Insert(nextNewline + 1, "    using Microsoft.AspNetCore.Components.Rendering;\n");
                }
            }
        }
        
        // Remove #line directives to avoid file not found errors during compilation
        // These directives reference the original .razor files which may not be accessible during build
        generatedCode = System.Text.RegularExpressions.Regex.Replace(
            generatedCode,
            @"#line \d+ "".*?""[\r\n]+",
            ""
        );
        
        // Also remove #nullable directives that may be problematic
        generatedCode = System.Text.RegularExpressions.Regex.Replace(
            generatedCode,
            @"#nullable (restore|enable|disable)[\r\n]+",
            ""
        );
        
        return generatedCode;
    }

    private static string AddNamespace(string text, string filename, string? rootNamespace)
    {
        if (string.IsNullOrEmpty(rootNamespace)) return text;
        if (text.Contains("@namespace")) return text;

        // Compute namespace from folder structure
        var directory = System.IO.Path.GetDirectoryName(filename);
        var computedNamespace = rootNamespace;

        if (!string.IsNullOrEmpty(directory))
        {
            // Normalize path separators to forward slash
            directory = directory.Replace('\\', '/');

            // Split by path separator and filter out empty or invalid segments
            var folders = directory.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var folder in folders)
            {
                // Only include folders that would make valid C# namespace identifiers
                if (IsValidNamespaceSegment(folder))
                {
                    computedNamespace += "." + folder;
                }
            }
        }

        return $"@namespace {computedNamespace}\n{text}";
    }

    /// <summary>
    /// Check if a folder name is valid as a C# namespace segment
    /// </summary>
    private static bool IsValidNamespaceSegment(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment)) return false;
        if (segment.Contains(":")) return false;
        if (segment == "." || segment == "..") return false;
        if (segment.Contains(" ") || segment.Contains("-")) return false;
        if (char.IsDigit(segment[0])) return false;

        return true;
    }

    private static RazorProjectEngine GetEngine()
    {
        var configuration = RazorConfiguration.Default;
        var razorProjectEngine = RazorProjectEngine.Create(configuration, RazorProjectFileSystem.Create("."));

        return razorProjectEngine;
    }
}
