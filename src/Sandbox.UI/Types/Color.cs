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
    /// Create color from hex string (e.g., "#FF0000" or "FF0000") or CSS color name or rgba()
    /// </summary>
    public static Color? Parse(string value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        value = value.Trim().ToLowerInvariant();

        // Handle named colors
        var namedColor = ParseNamedColor(value);
        if (namedColor.HasValue) return namedColor;

        // Handle rgba() / rgb()
        if (value.StartsWith("rgba(") || value.StartsWith("rgb("))
        {
            return ParseRgba(value);
        }

        // Handle hex
        var hex = value.TrimStart('#');

        // Handle 3-character hex (#RGB)
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        // Handle 4-character hex (#RGBA)
        if (hex.Length == 4)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
        }

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

    private static Color? ParseRgba(string value)
    {
        var start = value.IndexOf('(');
        var end = value.LastIndexOf(')');
        if (start < 0 || end < 0) return null;

        var inner = value.Substring(start + 1, end - start - 1);
        var parts = inner.Split(',', StringSplitOptions.TrimEntries);

        if (parts.Length < 3) return null;

        if (!float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var r)) return null;
        if (!float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var g)) return null;
        if (!float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var b)) return null;

        var a = 1f;
        if (parts.Length >= 4)
        {
            if (!float.TryParse(parts[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) return null;
        }

        // If values are > 1, assume they're 0-255 range
        if (r > 1 || g > 1 || b > 1)
        {
            r /= 255f;
            g /= 255f;
            b /= 255f;
        }

        // Alpha can also be in 0-255 range
        if (a > 1)
        {
            a /= 255f;
        }

        return new Color(r, g, b, a);
    }

    private static Color? ParseNamedColor(string name)
    {
        return name switch
        {
            "white" => White,
            "black" => Black,
            "red" => Red,
            "green" => Green,
            "blue" => Blue,
            "yellow" => Yellow,
            "cyan" or "aqua" => Cyan,
            "magenta" or "fuchsia" => Magenta,
            "transparent" => Transparent,
            "gray" or "grey" => Gray,
            "orange" => new Color(1, 0.647f, 0, 1),
            "purple" => new Color(0.5f, 0, 0.5f, 1),
            "pink" => new Color(1, 0.753f, 0.796f, 1),
            "brown" => new Color(0.647f, 0.165f, 0.165f, 1),
            "navy" => new Color(0, 0, 0.5f, 1),
            "lime" => new Color(0, 1, 0, 1),
            "olive" => new Color(0.5f, 0.5f, 0, 1),
            "maroon" => new Color(0.5f, 0, 0, 1),
            "silver" => new Color(0.753f, 0.753f, 0.753f, 1),
            "teal" => new Color(0, 0.5f, 0.5f, 1),
            _ => null
        };
    }

    public static bool operator ==(Color left, Color right) =>
        left.r == right.r && left.g == right.g && left.b == right.b && left.a == right.a;

    public static bool operator !=(Color left, Color right) => !(left == right);

    public override bool Equals(object? obj) => obj is Color color && this == color;
    public override int GetHashCode() => HashCode.Combine(r, g, b, a);

    public override string ToString() => $"Color({r:F2}, {g:F2}, {b:F2}, {a:F2})";
}
