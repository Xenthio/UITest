namespace Sandbox.UI;

/// <summary>
/// Renderer-agnostic color. Matches s&box's Color.
/// </summary>
public struct Color
{
    public float r;
    public float g;
    public float b;
    public float a;

    public Color(float r, float g, float b, float a = 1.0f)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public Color WithAlpha(float alpha) => new(r, g, b, alpha);
    public Color WithAlphaMultiplied(float alpha) => new(r, g, b, a * alpha);

    // Named colors
    public static Color White => new(1, 1, 1, 1);
    public static Color Black => new(0, 0, 0, 1);
    public static Color Red => new(1, 0, 0, 1);
    public static Color Green => new(0, 1, 0, 1);
    public static Color Blue => new(0, 0, 1, 1);
    public static Color Yellow => new(1, 1, 0, 1);
    public static Color Cyan => new(0, 1, 1, 1);
    public static Color Magenta => new(1, 0, 1, 1);
    public static Color Transparent => new(0, 0, 0, 0);
    public static Color Gray => new(0.5f, 0.5f, 0.5f, 1);

    /// <summary>
    /// Create color from RGBA bytes (0-255)
    /// </summary>
    public static Color FromRgba(byte r, byte g, byte b, byte a = 255)
    {
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    /// <summary>
    /// Create color from hex string (e.g., "#FF0000" or "FF0000")
    /// </summary>
    public static Color? Parse(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return null;

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
            {
                return FromRgba(r, g, b);
            }
        }
        else if (hex.Length == 8)
        {
            if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b) &&
                byte.TryParse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out byte a))
            {
                return FromRgba(r, g, b, a);
            }
        }

        return null;
    }

    public static bool operator ==(Color left, Color right) =>
        left.r == right.r && left.g == right.g && left.b == right.b && left.a == right.a;

    public static bool operator !=(Color left, Color right) => !(left == right);

    public override bool Equals(object? obj) => obj is Color color && this == color;
    public override int GetHashCode() => HashCode.Combine(r, g, b, a);

    public override string ToString() => $"Color({r:F2}, {g:F2}, {b:F2}, {a:F2})";
}
