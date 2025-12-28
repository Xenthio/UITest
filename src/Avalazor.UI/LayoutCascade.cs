namespace Avalazor.UI;

/// <summary>
/// Holds state that cascades down during layout computation.
/// Based on s&box's LayoutCascade pattern from Panel.Layout.cs
/// </summary>
public class LayoutCascade
{
    /// <summary>
    /// The root panel being laid out
    /// </summary>
    public Panel Root { get; set; }

    /// <summary>
    /// Available width for layout
    /// </summary>
    public float AvailableWidth { get; set; }

    /// <summary>
    /// Available height for layout
    /// </summary>
    public float AvailableHeight { get; set; }

    /// <summary>
    /// StyleEngine for computing styles
    /// </summary>
    public StyleEngine StyleEngine { get; set; }
}
