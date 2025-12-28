using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Contains the bounding boxes for a panel.
/// Based on s&box's Box class from Sandbox.Engine/Systems/UI/Panel/
/// </summary>
public class Box
{
    /// <summary>
    /// The outer rect including margin
    /// </summary>
    public SKRect Rect;

    /// <summary>
    /// The inner rect excluding padding
    /// </summary>
    public SKRect RectInner;

    /// <summary>
    /// The outer rect including margin (same as Rect for now)
    /// </summary>
    public SKRect RectOuter;
}
