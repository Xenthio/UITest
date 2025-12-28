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

public static class LengthExtensions
{
    public static bool IsDynamic(this LengthUnit unit) => unit is
        LengthUnit.ViewWidth or LengthUnit.ViewHeight or
        LengthUnit.ViewMin or LengthUnit.ViewMax or
        LengthUnit.Em or LengthUnit.RootEm or LengthUnit.Expression;
}
