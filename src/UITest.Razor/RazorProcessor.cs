using Microsoft.AspNetCore.Razor.Language;
using System.Collections.Generic;

namespace UITest.Razor;

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
        // If a root namespace is provided and the file doesn't have a namespace, inject one
        if (useFolderNamespacing)
        {
            text = AddNamespace(text, filename, rootNamespace);
        }

        var engine = GetEngine();

        RazorSourceDocument source = RazorSourceDocument.Create(text, filename);
        RazorCodeDocument code = engine.Process(source, FileKinds.Component, new List<RazorSourceDocument>(), new List<TagHelperDescriptor>());
        code.SetCodeGenerationOptions(RazorCodeGenerationOptions.Create(o => { }));

        RazorCSharpDocument document = code.GetCSharpDocument();

        return document.GeneratedCode;
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
