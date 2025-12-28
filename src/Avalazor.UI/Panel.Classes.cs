namespace Avalazor.UI;

/// <summary>
/// Panel partial class: CSS class management
/// Based on s&box's Panel.Classes.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Add a CSS class
    /// </summary>
    public void AddClass(string className)
    {
        if (Classes.Add(className))
        {
            needsPreLayout = true;
        }
    }

    /// <summary>
    /// Remove a CSS class
    /// </summary>
    public void RemoveClass(string className)
    {
        if (Classes.Remove(className))
        {
            needsPreLayout = true;
        }
    }

    /// <summary>
    /// Toggle a CSS class
    /// </summary>
    public void ToggleClass(string className)
    {
        if (Classes.Contains(className))
            RemoveClass(className);
        else
            AddClass(className);
    }

    /// <summary>
    /// Check if panel has a CSS class
    /// </summary>
    public bool HasClass(string className) => Classes.Contains(className);

    /// <summary>
    /// Set a single class, removing all others
    /// </summary>
    public void SetClass(string className)
    {
        Classes.Clear();
        Classes.Add(className);
        needsPreLayout = true;
    }
}
