using SkiaSharp;

namespace Sandbox.UI.Skia;

/// <summary>
/// Extension methods for converting between Sandbox.UI types and SkiaSharp types
/// </summary>
public static class SkiaExtensions
{
    /// <summary>
    /// Convert Sandbox.Color to SKColor
    /// </summary>
    public static SKColor ToSKColor(this Color color)
    {
        return new SKColor(
            (byte)(color.r * 255),
            (byte)(color.g * 255),
            (byte)(color.b * 255),
            (byte)(color.a * 255)
        );
    }

    /// <summary>
    /// Convert SKColor to Sandbox.Color
    /// </summary>
    public static Color ToSandboxColor(this SKColor color)
    {
        return new Color(
            color.Red / 255f,
            color.Green / 255f,
            color.Blue / 255f,
            color.Alpha / 255f
        );
    }

    /// <summary>
    /// Convert Sandbox.Rect to SKRect
    /// </summary>
    public static SKRect ToSKRect(this Rect rect)
    {
        return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

    /// <summary>
    /// Convert SKRect to Sandbox.Rect
    /// </summary>
    public static Rect ToSandboxRect(this SKRect rect)
    {
        return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
    }

    /// <summary>
    /// Convert Sandbox.Vector2 to SKPoint
    /// </summary>
    public static SKPoint ToSKPoint(this Vector2 v)
    {
        return new SKPoint(v.x, v.y);
    }

    /// <summary>
    /// Convert SKPoint to Sandbox.Vector2
    /// </summary>
    public static Vector2 ToSandboxVector2(this SKPoint p)
    {
        return new Vector2(p.X, p.Y);
    }

    /// <summary>
    /// Convert SKSize to Sandbox.Vector2
    /// </summary>
    public static Vector2 ToSandboxVector2(this SKSize s)
    {
        return new Vector2(s.Width, s.Height);
    }
}
