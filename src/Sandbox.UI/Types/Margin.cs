namespace Sandbox.UI;

/// <summary>
/// Margin/padding values for each edge
/// </summary>
public struct Margin
{
    public float Left;
    public float Top;
    public float Right;
    public float Bottom;

    public Margin(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Margin(float all)
    {
        Left = Top = Right = Bottom = all;
    }

    public static Margin Zero => new(0, 0, 0, 0);

    public override string ToString() => $"Margin({Left}, {Top}, {Right}, {Bottom})";

    /// <summary>
    /// Create a Margin from Length values
    /// </summary>
    public static Margin GetEdges(Vector2 size, Length? left, Length? top, Length? right, Length? bottom)
    {
        return new Margin(
            left?.GetPixels(size.x) ?? 0f,
            top?.GetPixels(size.y) ?? 0f,
            right?.GetPixels(size.x) ?? 0f,
            bottom?.GetPixels(size.y) ?? 0f
        );
    }

    /// <summary>
    /// Add two margins together
    /// </summary>
    public static Margin operator +(Margin a, Margin b)
    {
        return new Margin(
            a.Left + b.Left,
            a.Top + b.Top,
            a.Right + b.Right,
            a.Bottom + b.Bottom
        );
    }
}
