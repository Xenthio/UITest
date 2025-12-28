namespace Sandbox.UI;

/// <summary>
/// Represents position and size of a Panel on the screen.
/// Based on s&box's Box class from Panel.Layout.cs
/// </summary>
public class Box
{
    /// <summary>
    /// Position and size of the element on the screen, <b>including both - its padding AND margin</b>.
    /// </summary>
    public Rect RectOuter;

    /// <summary>
    /// Position and size of only the element's inner content on the screen, <i>without padding OR margin</i>.
    /// </summary>
    public Rect RectInner;

    /// <summary>
    /// The size of padding.
    /// </summary>
    public Margin Padding;

    /// <summary>
    /// The size of border.
    /// </summary>
    public Margin Border;

    /// <summary>
    /// The size of margin.
    /// </summary>
    public Margin Margin;

    /// <summary>
    /// Position and size of the element on the screen, <b>including its padding</b>, <i>but not margin</i>.
    /// </summary>
    public Rect Rect;

    /// <summary>
    /// Rect minus the border sizes.
    /// Used internally to "clip" (hide) everything outside of these bounds.
    /// </summary>
    public Rect ClipRect;

    /// <summary>
    /// Position of the left edge in screen coordinates.
    /// </summary>
    public float Left => Rect.Left;

    /// <summary>
    /// Position of the right edge in screen coordinates.
    /// </summary>
    public float Right => Rect.Right;

    /// <summary>
    /// Position of the top edge in screen coordinates.
    /// </summary>
    public float Top => Rect.Top;

    /// <summary>
    /// Position of the bottom edge in screen coordinates.
    /// </summary>
    public float Bottom => Rect.Bottom;
}
