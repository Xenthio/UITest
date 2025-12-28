namespace Sandbox.UI;

/// <summary>
/// Style property container - holds CSS property values.
/// Based on s&box's Styles class from engine/Sandbox.Engine/Systems/UI/Styles/
/// </summary>
public class Styles
{
    // Layout properties
    public Length? Width { get; set; }
    public Length? Height { get; set; }
    public Length? MinWidth { get; set; }
    public Length? MinHeight { get; set; }
    public Length? MaxWidth { get; set; }
    public Length? MaxHeight { get; set; }

    // Position
    public PositionMode? Position { get; set; }
    public Length? Left { get; set; }
    public Length? Top { get; set; }
    public Length? Right { get; set; }
    public Length? Bottom { get; set; }

    // Flex
    public FlexDirection? FlexDirection { get; set; }
    public Justify? JustifyContent { get; set; }
    public Align? AlignItems { get; set; }
    public Align? AlignSelf { get; set; }
    public Align? AlignContent { get; set; }
    public float? FlexGrow { get; set; }
    public float? FlexShrink { get; set; }
    public Length? FlexBasis { get; set; }
    public Wrap? FlexWrap { get; set; }

    // Spacing
    public Length? MarginLeft { get; set; }
    public Length? MarginTop { get; set; }
    public Length? MarginRight { get; set; }
    public Length? MarginBottom { get; set; }
    public Length? PaddingLeft { get; set; }
    public Length? PaddingTop { get; set; }
    public Length? PaddingRight { get; set; }
    public Length? PaddingBottom { get; set; }

    // Border
    public Length? BorderLeftWidth { get; set; }
    public Length? BorderTopWidth { get; set; }
    public Length? BorderRightWidth { get; set; }
    public Length? BorderBottomWidth { get; set; }
    public Color? BorderLeftColor { get; set; }
    public Color? BorderTopColor { get; set; }
    public Color? BorderRightColor { get; set; }
    public Color? BorderBottomColor { get; set; }
    public Length? BorderTopLeftRadius { get; set; }
    public Length? BorderTopRightRadius { get; set; }
    public Length? BorderBottomLeftRadius { get; set; }
    public Length? BorderBottomRightRadius { get; set; }

    // Visual
    public DisplayMode? Display { get; set; }
    public OverflowMode? Overflow { get; set; }
    public float? Opacity { get; set; }
    public Color? BackgroundColor { get; set; }
    public int? ZIndex { get; set; }
    public PointerEvents? PointerEvents { get; set; }

    // Text (cascading)
    public Color? Color { get; set; }
    public Length? FontSize { get; set; }
    public string? FontFamily { get; set; }
    public int? FontWeight { get; set; }
    public TextAlign? TextAlign { get; set; }
    public WordWrap? WordWrap { get; set; }

    // Gaps
    public Length? RowGap { get; set; }
    public Length? ColumnGap { get; set; }

    // Aspect ratio
    public float? AspectRatio { get; set; }

    /// <summary>
    /// Apply cascading properties from parent styles
    /// </summary>
    public void ApplyCascading(Styles parent)
    {
        // These properties cascade from parent if not set on child
        Color ??= parent.Color;
        FontSize ??= parent.FontSize;
        FontFamily ??= parent.FontFamily;
        FontWeight ??= parent.FontWeight;
    }

    /// <summary>
    /// Add/merge styles from another Styles object
    /// </summary>
    public void Add(Styles other)
    {
        if (other.Width.HasValue) Width = other.Width;
        if (other.Height.HasValue) Height = other.Height;
        if (other.MinWidth.HasValue) MinWidth = other.MinWidth;
        if (other.MinHeight.HasValue) MinHeight = other.MinHeight;
        if (other.MaxWidth.HasValue) MaxWidth = other.MaxWidth;
        if (other.MaxHeight.HasValue) MaxHeight = other.MaxHeight;

        if (other.Position.HasValue) Position = other.Position;
        if (other.Left.HasValue) Left = other.Left;
        if (other.Top.HasValue) Top = other.Top;
        if (other.Right.HasValue) Right = other.Right;
        if (other.Bottom.HasValue) Bottom = other.Bottom;

        if (other.FlexDirection.HasValue) FlexDirection = other.FlexDirection;
        if (other.JustifyContent.HasValue) JustifyContent = other.JustifyContent;
        if (other.AlignItems.HasValue) AlignItems = other.AlignItems;
        if (other.AlignSelf.HasValue) AlignSelf = other.AlignSelf;
        if (other.AlignContent.HasValue) AlignContent = other.AlignContent;
        if (other.FlexGrow.HasValue) FlexGrow = other.FlexGrow;
        if (other.FlexShrink.HasValue) FlexShrink = other.FlexShrink;
        if (other.FlexBasis.HasValue) FlexBasis = other.FlexBasis;
        if (other.FlexWrap.HasValue) FlexWrap = other.FlexWrap;

        if (other.MarginLeft.HasValue) MarginLeft = other.MarginLeft;
        if (other.MarginTop.HasValue) MarginTop = other.MarginTop;
        if (other.MarginRight.HasValue) MarginRight = other.MarginRight;
        if (other.MarginBottom.HasValue) MarginBottom = other.MarginBottom;
        if (other.PaddingLeft.HasValue) PaddingLeft = other.PaddingLeft;
        if (other.PaddingTop.HasValue) PaddingTop = other.PaddingTop;
        if (other.PaddingRight.HasValue) PaddingRight = other.PaddingRight;
        if (other.PaddingBottom.HasValue) PaddingBottom = other.PaddingBottom;

        if (other.BorderLeftWidth.HasValue) BorderLeftWidth = other.BorderLeftWidth;
        if (other.BorderTopWidth.HasValue) BorderTopWidth = other.BorderTopWidth;
        if (other.BorderRightWidth.HasValue) BorderRightWidth = other.BorderRightWidth;
        if (other.BorderBottomWidth.HasValue) BorderBottomWidth = other.BorderBottomWidth;
        if (other.BorderLeftColor.HasValue) BorderLeftColor = other.BorderLeftColor;
        if (other.BorderTopColor.HasValue) BorderTopColor = other.BorderTopColor;
        if (other.BorderRightColor.HasValue) BorderRightColor = other.BorderRightColor;
        if (other.BorderBottomColor.HasValue) BorderBottomColor = other.BorderBottomColor;
        if (other.BorderTopLeftRadius.HasValue) BorderTopLeftRadius = other.BorderTopLeftRadius;
        if (other.BorderTopRightRadius.HasValue) BorderTopRightRadius = other.BorderTopRightRadius;
        if (other.BorderBottomLeftRadius.HasValue) BorderBottomLeftRadius = other.BorderBottomLeftRadius;
        if (other.BorderBottomRightRadius.HasValue) BorderBottomRightRadius = other.BorderBottomRightRadius;

        if (other.Display.HasValue) Display = other.Display;
        if (other.Overflow.HasValue) Overflow = other.Overflow;
        if (other.Opacity.HasValue) Opacity = other.Opacity;
        if (other.BackgroundColor.HasValue) BackgroundColor = other.BackgroundColor;
        if (other.ZIndex.HasValue) ZIndex = other.ZIndex;
        if (other.PointerEvents.HasValue) PointerEvents = other.PointerEvents;

        if (other.Color.HasValue) Color = other.Color;
        if (other.FontSize.HasValue) FontSize = other.FontSize;
        if (other.FontFamily != null) FontFamily = other.FontFamily;
        if (other.FontWeight.HasValue) FontWeight = other.FontWeight;
        if (other.TextAlign.HasValue) TextAlign = other.TextAlign;
        if (other.WordWrap.HasValue) WordWrap = other.WordWrap;

        if (other.RowGap.HasValue) RowGap = other.RowGap;
        if (other.ColumnGap.HasValue) ColumnGap = other.ColumnGap;
        if (other.AspectRatio.HasValue) AspectRatio = other.AspectRatio;
    }
}
