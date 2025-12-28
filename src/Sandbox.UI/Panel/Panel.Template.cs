using System.Reflection;

namespace Sandbox.UI;

public partial class Panel
{
    private List<string>? _loadedTemplateStylesheets;

    /// <summary>
    /// Load the stylesheet for this panel based on [StyleSheet] attributes
    /// </summary>
    protected void LoadStyleSheet()
    {
        if (LoadStyleSheetFromAttribute())
            return;

        if (LoadStyleSheetAuto())
            return;
    }

    /// <summary>
    /// Loads stylesheets from [StyleSheet] attributes.
    /// </summary>
    /// <returns>True if any attribute exists and we loaded from it, otherwise false</returns>
    private bool LoadStyleSheetFromAttribute()
    {
        var type = GetType();
        var attrs = type.GetCustomAttributes<StyleSheetAttribute>(false).ToList();

        if (attrs.Count == 0)
        {
            // Clear any previously loaded stylesheets
            ClearLoadedTemplateStylesheets();
            return false;
        }

        // Get the source file location from SourceLocationAttribute
        var sourceFile = GetSourceFileForType(type);

        // Clear old sheets and load new ones
        ClearLoadedTemplateStylesheets();
        _loadedTemplateStylesheets = new List<string>();

        foreach (var attr in attrs)
        {
            var path = attr.Name;
            var fullPath = GetFullPath(path, sourceFile);
            if (LoadStyleSheetFromPath(fullPath, false))
            {
                _loadedTemplateStylesheets.Add(fullPath);
            }
        }

        return _loadedTemplateStylesheets.Count > 0;
    }

    /// <summary>
    /// Loads a stylesheet from one based on the class name.
    /// </summary>
    /// <returns>True if loaded, otherwise false</returns>
    private bool LoadStyleSheetAuto()
    {
        var type = GetType();
        var sourceFile = GetSourceFileForType(type);

        if (sourceFile == null)
            return false;

        var fullPath = GetFullPath(null, sourceFile);
        if (LoadStyleSheetFromPath(fullPath, true))
        {
            _loadedTemplateStylesheets ??= new List<string>();
            _loadedTemplateStylesheets.Add(fullPath);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Clear all previously loaded template stylesheets
    /// </summary>
    private void ClearLoadedTemplateStylesheets()
    {
        if (_loadedTemplateStylesheets == null)
            return;

        foreach (var path in _loadedTemplateStylesheets)
        {
            StyleSheet.Remove(path);
        }
        _loadedTemplateStylesheets.Clear();
    }

    /// <summary>
    /// Loads a stylesheet from the specified path.
    /// </summary>
    /// <returns>True if the stylesheet was loaded successfully, otherwise false</returns>
    private bool LoadStyleSheetFromPath(string? path, bool failSilently)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        path = path.NormalizeFilename();

        StyleSheet.Load(path, true, failSilently);
        return true;
    }

    /// <summary>
    /// Get the source file path for a type (if available)
    /// </summary>
    private string? GetSourceFileForType(Type type)
    {
        // Try to get from SourceLocationAttribute if it exists
        var sourceAttr = type.GetCustomAttribute<SourceLocationAttribute>();
        if (sourceAttr != null)
            return sourceAttr.FilePath;

        // Fallback: try to find it based on assembly location and type name
        var assembly = type.Assembly;
        var assemblyLocation = assembly.Location;
        if (!string.IsNullOrEmpty(assemblyLocation))
        {
            var dir = System.IO.Path.GetDirectoryName(assemblyLocation);
            var possiblePath = System.IO.Path.Combine(dir ?? "", type.Name + ".razor");
            if (System.IO.File.Exists(possiblePath))
                return possiblePath;
        }

        return null;
    }

    private string GetFullPath(string? path, string? sourceFile)
    {
        if (string.IsNullOrWhiteSpace(path) && sourceFile != null)
        {
            // Replace .razor extension with .scss
            var basePath = System.IO.Path.ChangeExtension(sourceFile, null);
            return basePath + ".scss";
        }
        else if (sourceFile != null && !string.IsNullOrWhiteSpace(path) && 
                 !path.StartsWith('/') && !path.StartsWith('\\'))
        {
            // Relative path - combine with source file directory
            var dir = System.IO.Path.GetDirectoryName(sourceFile);
            return System.IO.Path.Combine(dir ?? "", path);
        }

        return path ?? "";
    }
}
