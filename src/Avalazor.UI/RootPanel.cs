using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// A root panel. Serves as a container for other panels, handles things such as rendering.
/// Based on s&box's RootPanel.cs (MIT licensed)
/// </summary>
public class RootPanel : Panel
{
    /// <summary>
    /// Bounds of the panel, i.e. its size and position on the screen.
    /// </summary>
    public SKRect PanelBounds { get; set; } = new SKRect(0, 0, 512, 512);

    /// <summary>
    /// The scale of this panel and its children.
    /// </summary>
    public float Scale { get; protected set; } = 1.0f;

    public RootPanel()
    {
        Tag = "root";
        
        // Root panel takes full width and height
        Style = "width: 100%; height: 100%;";
    }

    /// <summary>
    /// Performs the full layout cycle: PreLayout, Calculate, FinalLayout
    /// Based on s&box's RootPanel.Layout() method
    /// </summary>
    internal void PerformLayout(float width, float height)
    {
        PanelBounds = new SKRect(0, 0, width, height);
        
        var cascade = new LayoutCascade
        {
            AvailableWidth = width,
            AvailableHeight = height
        };

        // PreLayout: Compute styles and setup Yoga
        PreLayout(cascade);

        // Calculate Yoga layout
        YogaNode?.CalculateLayout(width, height);

        // FinalLayout: Calculate final positions from Yoga
        FinalLayout(cascade);
    }

    /// <summary>
    /// Render this panel and all children
    /// </summary>
    internal void Render(SKCanvas canvas)
    {
        canvas.Save();
        Paint(canvas);
        canvas.Restore();
    }
}
