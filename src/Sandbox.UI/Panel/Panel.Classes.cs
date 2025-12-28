namespace Sandbox.UI;

/// <summary>
/// Panel partial class: CSS class management
/// Based on s&box's Panel.Classes.cs
/// </summary>
public partial class Panel
{
    internal HashSet<string>? _classes;

    /// <summary>
    /// Holds all CSS classes that are assigned to this panel.
    /// </summary>
    public IEnumerable<string> Classes => _classes ?? Enumerable.Empty<string>();

    /// <summary>
    /// Returns true if this panel has the given class.
    /// </summary>
    public bool HasClass(string? classname)
    {
        if (string.IsNullOrWhiteSpace(classname)) return false;
        return _classes?.Contains(classname) ?? false;
    }

    /// <summary>
    /// Add a CSS class to this panel.
    /// </summary>
    public void AddClass(string? classnames)
    {
        if (string.IsNullOrWhiteSpace(classnames)) return;

        _classes ??= new();

        foreach (var classname in classnames.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (_classes.Add(classname))
            {
                StyleSelectorsChanged(false, false);
            }
        }
    }

    /// <summary>
    /// Remove a CSS class from this panel.
    /// </summary>
    public void RemoveClass(string? classname)
    {
        if (string.IsNullOrWhiteSpace(classname)) return;
        if (_classes == null) return;

        if (_classes.Remove(classname))
        {
            StyleSelectorsChanged(false, false);
        }
    }

    /// <summary>
    /// Toggle a CSS class on this panel.
    /// </summary>
    public bool ToggleClass(string classname)
    {
        if (HasClass(classname))
        {
            RemoveClass(classname);
            return false;
        }

        AddClass(classname);
        return true;
    }

    /// <summary>
    /// Set a class conditionally.
    /// </summary>
    public void SetClass(string classname, bool enabled)
    {
        if (enabled)
            AddClass(classname);
        else
            RemoveClass(classname);
    }

    /// <summary>
    /// Returns true if this panel has all of the given classes.
    /// </summary>
    public bool HasClasses(string[] classes)
    {
        if (classes == null || classes.Length == 0) return true;
        if (_classes == null) return false;

        foreach (var classname in classes)
        {
            if (!_classes.Contains(classname))
                return false;
        }
        return true;
    }
}
