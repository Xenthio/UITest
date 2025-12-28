namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Property setting methods
/// Based on s&box's Panel.Property.cs
/// </summary>
public partial class Panel
{
    private string? _previousPropertyClass;

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
            // Style.Set(value); // TODO: Implement style parsing
            return;
        }

        // For other properties, derived classes can handle them
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
