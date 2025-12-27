using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Represents computed CSS styles for a panel
/// Based on s&box's style system
/// </summary>
public class ComputedStyle
{
    // Layout
    public float? Width { get; set; }
    public float? Height { get; set; }
    public float? MinWidth { get; set; }
    public float? MinHeight { get; set; }
    public float? MaxWidth { get; set; }
    public float? MaxHeight { get; set; }

    // Flexbox
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Column;
    public float FlexGrow { get; set; }
    public float FlexShrink { get; set; } = 1;
    public AlignItems AlignItems { get; set; }
    public JustifyContent JustifyContent { get; set; }

    // Spacing
    public float MarginTop { get; set; }
    public float MarginRight { get; set; }
    public float MarginBottom { get; set; }
    public float MarginLeft { get; set; }
    public float PaddingTop { get; set; }
    public float PaddingRight { get; set; }
    public float PaddingBottom { get; set; }
    public float PaddingLeft { get; set; }

    // Visual
    public SKColor? BackgroundColor { get; set; }
    public SKColor? Color { get; set; }
    public float BorderWidth { get; set; }
    public SKColor? BorderColor { get; set; }
    public float BorderRadius { get; set; }
    public float Opacity { get; set; } = 1.0f;

    // Positioning
    public Position Position { get; set; }
    public float? Top { get; set; }
    public float? Right { get; set; }
    public float? Bottom { get; set; }
    public float? Left { get; set; }
    public int ZIndex { get; set; }

    // Text
    public string? FontFamily { get; set; }
    public float FontSize { get; set; } = 14;
    public FontWeight FontWeight { get; set; } = FontWeight.Normal;
}

public enum FlexDirection
{
    Row,
    Column,
    RowReverse,
    ColumnReverse
}

public enum AlignItems
{
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    Baseline
}

public enum JustifyContent
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum Position
{
    Relative,
    Absolute,
    Fixed
}

public enum FontWeight
{
    Thin = 100,
    ExtraLight = 200,
    Light = 300,
    Normal = 400,
    Medium = 500,
    SemiBold = 600,
    Bold = 700,
    ExtraBold = 800,
    Black = 900
}
