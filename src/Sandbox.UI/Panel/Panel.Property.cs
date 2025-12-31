namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Property setting methods
/// Based on s&box's Panel.Property.cs
/// </summary>
public partial class Panel
{
    private string? _previousPropertyClass;
    private Dictionary<string, string>? _attributes;

    /// <summary>
    /// String value for the panel. Can be used to store simple string data.
    /// </summary>
    public string? StringValue { get; set; }

    /// <summary>
    /// Set a property on the panel, such as special properties (class, id, style and value, etc.)
    /// and properties of the panel's C# class.
    /// </summary>
    /// <param name="name">Name of the property to modify.</param>
    /// <param name="value">Value to assign to the property.</param>
    public virtual void SetProperty(string name, string value)
    {
        if (name == "id")
        {
            Id = value;
            return;
        }

        if (name == "value")
        {
            StringValue = value;
            return;
        }

        if (name == "class")
        {
            if (!string.IsNullOrEmpty(_previousPropertyClass))
            {
                RemoveClass(_previousPropertyClass);
            }

            _previousPropertyClass = value;
            AddClass(value);
            return;
        }

        if (name == "style")
        {
            Style.Set(value);
            return;
        }

        // Store as attribute for derived classes to access
        SetAttribute(name, value);
    }

    /// <summary>
    /// Set an attribute on the panel. Used for custom HTML-like attributes.
    /// </summary>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    public void SetAttribute(string name, string value)
    {
        _attributes ??= new Dictionary<string, string>();
        _attributes[name.ToLower()] = value;
    }

    /// <summary>
    /// Get an attribute value from the panel.
    /// </summary>
    /// <param name="name">Attribute name</param>
    /// <returns>Attribute value, or null if not found</returns>
    public string? GetAttribute(string name)
    {
        if (_attributes == null) return null;
        _attributes.TryGetValue(name.ToLower(), out var value);
        return value;
    }

    /// <summary>
    /// Check if the panel has an attribute.
    /// </summary>
    /// <param name="name">Attribute name</param>
    /// <returns>True if the attribute exists</returns>
    public bool HasAttribute(string name)
    {
        return _attributes?.ContainsKey(name.ToLower()) ?? false;
    }

    /// <summary>
    /// Remove an attribute from the panel.
    /// </summary>
    /// <param name="name">Attribute name</param>
    public void RemoveAttribute(string name)
    {
        _attributes?.Remove(name.ToLower());
    }

    /// <summary>
    /// Set the content of the panel. For Label, this sets the text.
    /// For other panels, this can be used to set inner content.
    /// </summary>
    /// <param name="value">The content value</param>
    public virtual void SetContent(string? value)
    {
        // Base implementation does nothing - derived classes override this
    }

    /// <summary>
    /// Called when parameters are set on the panel (e.g., from Razor).
    /// Override this to handle parameter updates.
    /// </summary>
    protected virtual void OnParametersSet()
    {
        // Base implementation does nothing
    }
}
