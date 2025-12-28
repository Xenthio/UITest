namespace Sandbox.UI;

/// <summary>
/// Style property container - holds CSS property values.
/// Based on s&box's Styles class from engine/Sandbox.Engine/Systems/UI/Styles/
/// </summary>
public class Styles
{
    internal Dictionary<string, IStyleBlock.StyleProperty> RawValues = new Dictionary<string, IStyleBlock.StyleProperty>( StringComparer.OrdinalIgnoreCase );

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

    /// <summary>
    /// Copy all styles from another Styles object
    /// </summary>
    public void From(Styles other)
    {
        Width = other.Width;
        Height = other.Height;
        MinWidth = other.MinWidth;
        MinHeight = other.MinHeight;
        MaxWidth = other.MaxWidth;
        MaxHeight = other.MaxHeight;

        Position = other.Position;
        Left = other.Left;
        Top = other.Top;
        Right = other.Right;
        Bottom = other.Bottom;

        FlexDirection = other.FlexDirection;
        JustifyContent = other.JustifyContent;
        AlignItems = other.AlignItems;
        AlignSelf = other.AlignSelf;
        AlignContent = other.AlignContent;
        FlexGrow = other.FlexGrow;
        FlexShrink = other.FlexShrink;
        FlexBasis = other.FlexBasis;
        FlexWrap = other.FlexWrap;

        MarginLeft = other.MarginLeft;
        MarginTop = other.MarginTop;
        MarginRight = other.MarginRight;
        MarginBottom = other.MarginBottom;
        PaddingLeft = other.PaddingLeft;
        PaddingTop = other.PaddingTop;
        PaddingRight = other.PaddingRight;
        PaddingBottom = other.PaddingBottom;

        BorderLeftWidth = other.BorderLeftWidth;
        BorderTopWidth = other.BorderTopWidth;
        BorderRightWidth = other.BorderRightWidth;
        BorderBottomWidth = other.BorderBottomWidth;
        BorderLeftColor = other.BorderLeftColor;
        BorderTopColor = other.BorderTopColor;
        BorderRightColor = other.BorderRightColor;
        BorderBottomColor = other.BorderBottomColor;
        BorderTopLeftRadius = other.BorderTopLeftRadius;
        BorderTopRightRadius = other.BorderTopRightRadius;
        BorderBottomLeftRadius = other.BorderBottomLeftRadius;
        BorderBottomRightRadius = other.BorderBottomRightRadius;

        Display = other.Display;
        Overflow = other.Overflow;
        Opacity = other.Opacity;
        BackgroundColor = other.BackgroundColor;
        ZIndex = other.ZIndex;
        PointerEvents = other.PointerEvents;

        Color = other.Color;
        FontSize = other.FontSize;
        FontFamily = other.FontFamily;
        FontWeight = other.FontWeight;
        TextAlign = other.TextAlign;
        WordWrap = other.WordWrap;

        RowGap = other.RowGap;
        ColumnGap = other.ColumnGap;
        AspectRatio = other.AspectRatio;
    }

    /// <summary>
    /// Interpolate between two Styles objects
    /// </summary>
    public void FromLerp(Styles from, Styles to, float delta)
    {
        // For now, just use the "to" values - animation lerping can be implemented later
        From(to);
    }

    /// <summary>
    /// Set a CSS property by name
    /// </summary>
    public virtual bool Set(string property, string value)
    {
        property = property.Trim().ToLowerInvariant();
        value = value.Trim();

        switch (property)
        {
            // Layout
            case "width": Width = Length.Parse(value); return Width.HasValue;
            case "height": Height = Length.Parse(value); return Height.HasValue;
            case "min-width": MinWidth = Length.Parse(value); return MinWidth.HasValue;
            case "min-height": MinHeight = Length.Parse(value); return MinHeight.HasValue;
            case "max-width": MaxWidth = Length.Parse(value); return MaxWidth.HasValue;
            case "max-height": MaxHeight = Length.Parse(value); return MaxHeight.HasValue;

            // Position
            case "position": return SetPosition(value);
            case "left": Left = Length.Parse(value); return Left.HasValue;
            case "top": Top = Length.Parse(value); return Top.HasValue;
            case "right": Right = Length.Parse(value); return Right.HasValue;
            case "bottom": Bottom = Length.Parse(value); return Bottom.HasValue;

            // Flex
            case "flex-direction": return SetFlexDirection(value);
            case "justify-content": return SetJustifyContent(value);
            case "align-items": return SetAlign(value, v => AlignItems = v);
            case "align-self": return SetAlign(value, v => AlignSelf = v);
            case "align-content": return SetAlign(value, v => AlignContent = v);
            case "flex-grow": return SetFloat(value, v => FlexGrow = v);
            case "flex-shrink": return SetFloat(value, v => FlexShrink = v);
            case "flex-basis": FlexBasis = Length.Parse(value); return FlexBasis.HasValue;
            case "flex-wrap": return SetFlexWrap(value);

            // Margin
            case "margin": return SetMargin(value);
            case "margin-left": MarginLeft = Length.Parse(value); return MarginLeft.HasValue;
            case "margin-top": MarginTop = Length.Parse(value); return MarginTop.HasValue;
            case "margin-right": MarginRight = Length.Parse(value); return MarginRight.HasValue;
            case "margin-bottom": MarginBottom = Length.Parse(value); return MarginBottom.HasValue;

            // Padding
            case "padding": return SetPadding(value);
            case "padding-left": PaddingLeft = Length.Parse(value); return PaddingLeft.HasValue;
            case "padding-top": PaddingTop = Length.Parse(value); return PaddingTop.HasValue;
            case "padding-right": PaddingRight = Length.Parse(value); return PaddingRight.HasValue;
            case "padding-bottom": PaddingBottom = Length.Parse(value); return PaddingBottom.HasValue;

            // Border width
            case "border-width": return SetBorderWidth(value);
            case "border-left-width": BorderLeftWidth = Length.Parse(value); return BorderLeftWidth.HasValue;
            case "border-top-width": BorderTopWidth = Length.Parse(value); return BorderTopWidth.HasValue;
            case "border-right-width": BorderRightWidth = Length.Parse(value); return BorderRightWidth.HasValue;
            case "border-bottom-width": BorderBottomWidth = Length.Parse(value); return BorderBottomWidth.HasValue;

            // Border color
            case "border-color": return SetBorderColor(value);
            case "border-left-color": BorderLeftColor = UI.Color.Parse(value); return BorderLeftColor.HasValue;
            case "border-top-color": BorderTopColor = UI.Color.Parse(value); return BorderTopColor.HasValue;
            case "border-right-color": BorderRightColor = UI.Color.Parse(value); return BorderRightColor.HasValue;
            case "border-bottom-color": BorderBottomColor = UI.Color.Parse(value); return BorderBottomColor.HasValue;

            // Border radius
            case "border-radius": return SetBorderRadius(value);
            case "border-top-left-radius": BorderTopLeftRadius = Length.Parse(value); return BorderTopLeftRadius.HasValue;
            case "border-top-right-radius": BorderTopRightRadius = Length.Parse(value); return BorderTopRightRadius.HasValue;
            case "border-bottom-left-radius": BorderBottomLeftRadius = Length.Parse(value); return BorderBottomLeftRadius.HasValue;
            case "border-bottom-right-radius": BorderBottomRightRadius = Length.Parse(value); return BorderBottomRightRadius.HasValue;

            // Visual
            case "display": return SetDisplay(value);
            case "overflow": return SetOverflow(value);
            case "opacity": return SetFloat(value, v => Opacity = v);
            case "background-color": BackgroundColor = UI.Color.Parse(value); return BackgroundColor.HasValue;
            case "background": BackgroundColor = UI.Color.Parse(value); return BackgroundColor.HasValue;
            case "z-index": return SetInt(value, v => ZIndex = v);
            case "pointer-events": return SetPointerEvents(value);

            // Text
            case "color": Color = UI.Color.Parse(value); return Color.HasValue;
            case "font-size": FontSize = Length.Parse(value); return FontSize.HasValue;
            case "font-family": FontFamily = value.Trim('"', '\''); return true;
            case "font-weight": return SetFontWeight(value);
            case "text-align": return SetTextAlign(value);
            case "word-wrap":
            case "overflow-wrap": return SetWordWrap(value);

            // Gap
            case "gap": return SetGap(value);
            case "row-gap": RowGap = Length.Parse(value); return RowGap.HasValue;
            case "column-gap": ColumnGap = Length.Parse(value); return ColumnGap.HasValue;

            // Aspect ratio
            case "aspect-ratio": return SetFloat(value, v => AspectRatio = v);

            default:
                return false;
        }
    }

    internal bool SetInternal(string styles, string filename, int lineoffset)
    {
        bool success = false;

        Parse p = new(styles, filename, lineoffset);

        while (!p.IsEnd)
        {
            p = p.SkipWhitespaceAndNewlines();
            var property = p.ReadUntilOrEnd(" :;");

            p = p.SkipWhitespaceAndNewlines();

            if (p.IsEnd)
                break;

            if (p.Current != ':')
                throw new System.Exception($"Error parsing style: {styles}");

            p.Pointer++;

            p = p.SkipWhitespaceAndNewlines();
            if (p.IsEnd)
                throw new System.Exception($"Error parsing style: {styles}");

            var line = p.CurrentLine;
            var value = p.ReadUntilOrEnd(";");
            p.Pointer++;

            bool wasSuccessful = Set(property, value);
            if (!wasSuccessful)
            {
                Console.WriteLine($"{value} is not valid with {property} {p.FileAndLine}");
            }

            var prop = new IStyleBlock.StyleProperty
            {
                Name = property,
                Value = value,
                OriginalValue = value,
                Line = line,
                IsValid = wasSuccessful
            };

            RawValues[property] = prop;
            success = wasSuccessful || success;

            p = p.SkipWhitespaceAndNewlines();
        }

        return success;
    }

    /// <summary>
    /// Try to find all panels using this style and mark them dirty so they'll
    /// redraw with the style.
    /// </summary>
    internal void MarkPanelsDirty()
    {
        // This would need access to RootPanels - for now a stub
        // In full implementation, iterate all RootPanels and call DirtyStylesWithStyle
    }

    #region Property Setters

    private bool SetFloat(string value, Action<float> setter)
    {
        if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var result))
        {
            setter(result);
            return true;
        }
        return false;
    }

    private bool SetInt(string value, Action<int> setter)
    {
        if (int.TryParse(value, out var result))
        {
            setter(result);
            return true;
        }
        return false;
    }

    private bool SetPosition(string value)
    {
        Position = value.ToLowerInvariant() switch
        {
            "static" => PositionMode.Static,
            "relative" => PositionMode.Relative,
            "absolute" => PositionMode.Absolute,
            _ => null
        };
        return Position.HasValue;
    }

    private bool SetFlexDirection(string value)
    {
        FlexDirection = value.ToLowerInvariant() switch
        {
            "row" => UI.FlexDirection.Row,
            "column" => UI.FlexDirection.Column,
            "row-reverse" => UI.FlexDirection.RowReverse,
            "column-reverse" => UI.FlexDirection.ColumnReverse,
            _ => null
        };
        return FlexDirection.HasValue;
    }

    private bool SetJustifyContent(string value)
    {
        JustifyContent = value.ToLowerInvariant() switch
        {
            "flex-start" or "start" => Justify.FlexStart,
            "flex-end" or "end" => Justify.FlexEnd,
            "center" => Justify.Center,
            "space-between" => Justify.SpaceBetween,
            "space-around" => Justify.SpaceAround,
            "space-evenly" => Justify.SpaceEvenly,
            _ => null
        };
        return JustifyContent.HasValue;
    }

    private bool SetAlign(string value, Action<Align> setter)
    {
        var align = value.ToLowerInvariant() switch
        {
            "auto" => (Align?)Align.Auto,
            "flex-start" or "start" => Align.FlexStart,
            "flex-end" or "end" => Align.FlexEnd,
            "center" => Align.Center,
            "stretch" => Align.Stretch,
            "baseline" => Align.Baseline,
            "space-between" => Align.SpaceBetween,
            "space-around" => Align.SpaceAround,
            _ => null
        };
        if (align.HasValue) setter(align.Value);
        return align.HasValue;
    }

    private bool SetFlexWrap(string value)
    {
        FlexWrap = value.ToLowerInvariant() switch
        {
            "nowrap" => Wrap.NoWrap,
            "wrap" => Wrap.Wrap,
            "wrap-reverse" => Wrap.WrapReverse,
            _ => null
        };
        return FlexWrap.HasValue;
    }

    private bool SetMargin(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var v = Length.Parse(parts[0]);
            MarginTop = MarginRight = MarginBottom = MarginLeft = v;
            return v.HasValue;
        }
        if (parts.Length == 2)
        {
            MarginTop = MarginBottom = Length.Parse(parts[0]);
            MarginLeft = MarginRight = Length.Parse(parts[1]);
            return true;
        }
        if (parts.Length == 4)
        {
            MarginTop = Length.Parse(parts[0]);
            MarginRight = Length.Parse(parts[1]);
            MarginBottom = Length.Parse(parts[2]);
            MarginLeft = Length.Parse(parts[3]);
            return true;
        }
        return false;
    }

    private bool SetPadding(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var v = Length.Parse(parts[0]);
            PaddingTop = PaddingRight = PaddingBottom = PaddingLeft = v;
            return v.HasValue;
        }
        if (parts.Length == 2)
        {
            PaddingTop = PaddingBottom = Length.Parse(parts[0]);
            PaddingLeft = PaddingRight = Length.Parse(parts[1]);
            return true;
        }
        if (parts.Length == 4)
        {
            PaddingTop = Length.Parse(parts[0]);
            PaddingRight = Length.Parse(parts[1]);
            PaddingBottom = Length.Parse(parts[2]);
            PaddingLeft = Length.Parse(parts[3]);
            return true;
        }
        return false;
    }

    private bool SetBorderWidth(string value)
    {
        var v = Length.Parse(value);
        BorderTopWidth = BorderRightWidth = BorderBottomWidth = BorderLeftWidth = v;
        return v.HasValue;
    }

    private bool SetBorderColor(string value)
    {
        var v = UI.Color.Parse(value);
        BorderTopColor = BorderRightColor = BorderBottomColor = BorderLeftColor = v;
        return v.HasValue;
    }

    private bool SetBorderRadius(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var v = Length.Parse(parts[0]);
            BorderTopLeftRadius = BorderTopRightRadius = BorderBottomRightRadius = BorderBottomLeftRadius = v;
            return v.HasValue;
        }
        if (parts.Length == 4)
        {
            BorderTopLeftRadius = Length.Parse(parts[0]);
            BorderTopRightRadius = Length.Parse(parts[1]);
            BorderBottomRightRadius = Length.Parse(parts[2]);
            BorderBottomLeftRadius = Length.Parse(parts[3]);
            return true;
        }
        return false;
    }

    private bool SetDisplay(string value)
    {
        Display = value.ToLowerInvariant() switch
        {
            "flex" => DisplayMode.Flex,
            "none" => DisplayMode.None,
            "contents" => DisplayMode.Contents,
            _ => null
        };
        return Display.HasValue;
    }

    private bool SetOverflow(string value)
    {
        Overflow = value.ToLowerInvariant() switch
        {
            "visible" => OverflowMode.Visible,
            "hidden" => OverflowMode.Hidden,
            "scroll" => OverflowMode.Scroll,
            _ => null
        };
        return Overflow.HasValue;
    }

    private bool SetPointerEvents(string value)
    {
        PointerEvents = value.ToLowerInvariant() switch
        {
            "all" or "auto" => UI.PointerEvents.All,
            "none" => UI.PointerEvents.None,
            _ => null
        };
        return PointerEvents.HasValue;
    }

    private bool SetFontWeight(string value)
    {
        if (int.TryParse(value, out var weight))
        {
            FontWeight = weight;
            return true;
        }
        FontWeight = value.ToLowerInvariant() switch
        {
            "normal" => 400,
            "bold" => 700,
            "lighter" => 300,
            "bolder" => 800,
            _ => null
        };
        return FontWeight.HasValue;
    }

    private bool SetTextAlign(string value)
    {
        TextAlign = value.ToLowerInvariant() switch
        {
            "left" => UI.TextAlign.Left,
            "center" => UI.TextAlign.Center,
            "right" => UI.TextAlign.Right,
            "justify" => UI.TextAlign.Justify,
            _ => null
        };
        return TextAlign.HasValue;
    }

    private bool SetWordWrap(string value)
    {
        WordWrap = value.ToLowerInvariant() switch
        {
            "normal" => UI.WordWrap.Normal,
            "nowrap" or "no-wrap" => UI.WordWrap.NoWrap,
            "break-word" => UI.WordWrap.BreakWord,
            _ => null
        };
        return WordWrap.HasValue;
    }

    private bool SetGap(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            var v = Length.Parse(parts[0]);
            RowGap = ColumnGap = v;
            return v.HasValue;
        }
        if (parts.Length == 2)
        {
            RowGap = Length.Parse(parts[0]);
            ColumnGap = Length.Parse(parts[1]);
            return true;
        }
        return false;
    }

    #endregion
}
