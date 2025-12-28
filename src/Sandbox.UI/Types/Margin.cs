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
}
