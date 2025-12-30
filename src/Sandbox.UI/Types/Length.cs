namespace Sandbox.UI;

/// <summary>
/// CSS Length value with unit
/// Based on s&box's Length struct
/// </summary>
public struct Length
{
    public float Value;
    public LengthUnit Unit;

    public Length(float value, LengthUnit unit = LengthUnit.Pixels)
    {
        Value = value;
        Unit = unit;
    }

    /// <summary>
    /// Create a pixel length
    /// </summary>
    public static Length Pixels(float value) => new(value, LengthUnit.Pixels);

    /// <summary>
    /// Create a percentage length
    /// </summary>
    public static Length Percent(float value) => new(value, LengthUnit.Percentage);

    /// <summary>
    /// Auto length
    /// </summary>
    public static Length Auto => new(0, LengthUnit.Auto);
    
    /// <summary>
    /// Undefined length
    /// </summary>
    public static Length Undefined => new(0, LengthUnit.Undefined);

    /// <summary>
    /// Contain (for images)
    /// </summary>
    public static Length Contain => new(0, LengthUnit.Contain);

    /// <summary>
    /// Cover (for images)
    /// </summary>
    public static Length Cover => new(0, LengthUnit.Cover);

    /// <summary>
    /// Get pixels value, resolving relative units
    /// </summary>
    public float GetPixels(float parentSize)
    {
        return Unit switch
        {
            LengthUnit.Pixels => Value,
            LengthUnit.Percentage => Value * parentSize / 100f,
            LengthUnit.Em => Value * CurrentFontSize,
            LengthUnit.RootEm => Value * RootFontSize,
            LengthUnit.ViewWidth => Value * RootSize.x / 100f,
            LengthUnit.ViewHeight => Value * RootSize.y / 100f,
            LengthUnit.ViewMin => Value * Math.Min(RootSize.x, RootSize.y) / 100f,
            LengthUnit.ViewMax => Value * Math.Max(RootSize.x, RootSize.y) / 100f,
            _ => Value
        };
    }

    /// <summary>
    /// Check if this is a dynamic unit (needs recalculation)
    /// </summary>
    public bool IsDynamic => Unit is LengthUnit.ViewWidth or LengthUnit.ViewHeight or
                               LengthUnit.ViewMin or LengthUnit.ViewMax or
                               LengthUnit.Em or LengthUnit.RootEm or LengthUnit.Expression;

    // Thread-local context for relative calculations
    public static Vector2 RootSize = new(1920, 1080);
    public static float RootFontSize = 16f;
    public static float RootScale = 1f;
    [ThreadStatic] public static float CurrentFontSize = 16f;

    public static implicit operator Length(float value) => Pixels(value);

    public static bool operator ==(Length a, Length b) => a.Value == b.Value && a.Unit == b.Unit;
    public static bool operator !=(Length a, Length b) => !(a == b);

    public override bool Equals(object? obj) => obj is Length length && this == length;
    public override int GetHashCode() => HashCode.Combine(Value, Unit);

    public override string ToString() => Unit switch
    {
        LengthUnit.Pixels => $"{Value}px",
        LengthUnit.Percentage => $"{Value}%",
        LengthUnit.Auto => "auto",
        _ => $"{Value}{Unit}"
    };
    
    /// <summary>
    /// Scale a length value by a multiplier (ref parameter for in-place modification)
    /// </summary>
    public static void Scale(ref Length length, float scale)
    {
        length = new Length(length.Value * scale, length.Unit);
    }
    
    /// <summary>
    /// Linear interpolation between two length values
    /// </summary>
    public static Length? Lerp(Length a, Length b, float t)
    {
        // Only lerp if units match
        if (a.Unit != b.Unit)
            return b;
            
        return new Length(MathX.Lerp(a.Value, b.Value, t), a.Unit);
    }
    
    /// <summary>
    /// Linear interpolation between two length values with dimension context (for percentage calculations)
    /// </summary>
    public static Length? Lerp(Length a, Length b, float t, float dimension)
    {
        // Only lerp if units match
        if (a.Unit != b.Unit)
            return b;
            
        return new Length(MathX.Lerp(a.Value, b.Value, t), a.Unit);
    }

    /// <summary>
    /// Get fraction value for percentages (0-1 range)
    /// </summary>
    public float GetFraction() => Unit == LengthUnit.Percentage ? Value / 100f : Value;
    
    /// <summary>
    /// Create a length from a fraction (0-1 range) as percentage
    /// </summary>
    public static Length Fraction(float value) => new(value * 100f, LengthUnit.Percentage);

    /// <summary>
    /// Parse a CSS length string
    /// </summary>
    public static Length? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim().ToLowerInvariant();

        // Handle special keywords
        if (value == "auto") return Auto;
        if (value == "contain") return Contain;
        if (value == "cover") return Cover;

        // Try to extract number and unit
        int unitStart = 0;
        for (int i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsDigit(c) || c == '.' || c == '-' || c == '+')
            {
                unitStart = i + 1;
                continue;
            }
            break;
        }

        // If we didn't find any number characters after initial processing
        if (unitStart == 0)
        {
            // No number found - return null (keywords like auto/contain/cover handled above)
            return null;
        }

        var numberPart = value.Substring(0, unitStart);
        var unitPart = value.Substring(unitStart).Trim();

        if (!float.TryParse(numberPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number))
            return null;

        var unit = unitPart switch
        {
            "" or "px" => LengthUnit.Pixels,
            "%" => LengthUnit.Percentage,
            "em" => LengthUnit.Em,
            "rem" => LengthUnit.RootEm,
            "vw" => LengthUnit.ViewWidth,
            "vh" => LengthUnit.ViewHeight,
            "vmin" => LengthUnit.ViewMin,
            "vmax" => LengthUnit.ViewMax,
            _ => LengthUnit.Undefined
        };

        if (unit == LengthUnit.Undefined && !string.IsNullOrEmpty(unitPart))
            return null;

        return new Length(number, unit);
    }
    
    /// <summary>
    /// Try to parse a CSS length string
    /// </summary>
    public static bool TryParse(string value, out Length result)
    {
        var parsed = Parse(value);
        if (parsed.HasValue)
        {
            result = parsed.Value;
            return true;
        }
        result = default;
        return false;
    }
}

public enum LengthUnit
{
    Undefined,
    Pixels,
    Percentage,
    Auto,
    Em,
    RootEm,
    ViewWidth,
    ViewHeight,
    ViewMin,
    ViewMax,
    Expression,
    Contain,
    Cover
}
