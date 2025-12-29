namespace Sandbox.UI;

/// <summary>
/// Style property container - holds CSS property values.
/// Based on s&box's Styles class from engine/Sandbox.Engine/Systems/UI/Styles/
/// Inherits from BaseStyles to get all ~200+ CSS properties from BaseStyles.Generated.cs
/// </summary>
public class Styles : BaseStyles
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

    // Note: Border properties (BorderLeftWidth, BorderTopColor, BorderTopLeftRadius, etc.)
    // are inherited from BaseStyles.Generated.cs - do not duplicate them here!
    
    // Note: Visual properties (BackgroundColor, Display, Opacity, etc.) 
    // are inherited from BaseStyles.Generated.cs - do not duplicate them here!

    // Background gradient (for linear-gradient, radial-gradient)
    public GradientInfo? BackgroundGradient { get; set; }

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
    /// Implement abstract Dirty() method from BaseStyles.
    /// Called when any CSS properties are changed.
    /// </summary>
    public override void Dirty()
    {
        // Mark panels using this style as dirty for re-rendering
        MarkPanelsDirty();
    }

    /// <summary>
    /// Implement abstract Clone() method from ICloneable
    /// </summary>
    public object Clone()
    {
        var clone = new Styles();
        clone.From(this);
        return clone;
    }

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

        // Border properties inherited from BaseStyles - use base class
        if (other.BorderLeftWidth.HasValue) base.BorderLeftWidth = other.BorderLeftWidth;
        if (other.BorderTopWidth.HasValue) base.BorderTopWidth = other.BorderTopWidth;
        if (other.BorderRightWidth.HasValue) base.BorderRightWidth = other.BorderRightWidth;
        if (other.BorderBottomWidth.HasValue) base.BorderBottomWidth = other.BorderBottomWidth;
        if (other.BorderLeftColor.HasValue) base.BorderLeftColor = other.BorderLeftColor;
        if (other.BorderTopColor.HasValue) base.BorderTopColor = other.BorderTopColor;
        if (other.BorderRightColor.HasValue) base.BorderRightColor = other.BorderRightColor;
        if (other.BorderBottomColor.HasValue) base.BorderBottomColor = other.BorderBottomColor;
        if (other.BorderTopLeftRadius.HasValue) base.BorderTopLeftRadius = other.BorderTopLeftRadius;
        if (other.BorderTopRightRadius.HasValue) base.BorderTopRightRadius = other.BorderTopRightRadius;
        if (other.BorderBottomLeftRadius.HasValue) base.BorderBottomLeftRadius = other.BorderBottomLeftRadius;
        if (other.BorderBottomRightRadius.HasValue) base.BorderBottomRightRadius = other.BorderBottomRightRadius;

        // Visual properties inherited from BaseStyles - use base class
        if (other.Display.HasValue) base.Display = other.Display;
        if (other.Overflow.HasValue) base.Overflow = other.Overflow;
        if (other.Opacity.HasValue) base.Opacity = other.Opacity;
        if (other.BackgroundColor.HasValue) base.BackgroundColor = other.BackgroundColor;
        if (other.BackgroundGradient.HasValue) BackgroundGradient = other.BackgroundGradient;
        if (other.ZIndex.HasValue) base.ZIndex = other.ZIndex;
        if (other.PointerEvents.HasValue) base.PointerEvents = other.PointerEvents;

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

        // Border properties inherited from BaseStyles - use base class
        base.BorderLeftWidth = other.BorderLeftWidth;
        base.BorderTopWidth = other.BorderTopWidth;
        base.BorderRightWidth = other.BorderRightWidth;
        base.BorderBottomWidth = other.BorderBottomWidth;
        base.BorderLeftColor = other.BorderLeftColor;
        base.BorderTopColor = other.BorderTopColor;
        base.BorderRightColor = other.BorderRightColor;
        base.BorderBottomColor = other.BorderBottomColor;
        base.BorderTopLeftRadius = other.BorderTopLeftRadius;
        base.BorderTopRightRadius = other.BorderTopRightRadius;
        base.BorderBottomLeftRadius = other.BorderBottomLeftRadius;
        base.BorderBottomRightRadius = other.BorderBottomRightRadius;

        // Visual properties inherited from BaseStyles - use base class
        base.Display = other.Display;
        base.Overflow = other.Overflow;
        base.Opacity = other.Opacity;
        base.BackgroundColor = other.BackgroundColor;
        BackgroundGradient = other.BackgroundGradient;
        base.ZIndex = other.ZIndex;
        base.PointerEvents = other.PointerEvents;

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
    /// Set a CSS property by name.
    /// First tries BaseStyles.Set() for all properties from BaseStyles.Generated.cs,
    /// then falls back to legacy property handling for backwards compatibility.
    /// </summary>
    public override bool Set(string property, string value)
    {
        // Try BaseStyles first - this handles ~200+ CSS properties from BaseStyles.Generated.cs
        if (base.Set(property, value))
            return true;

        // Fall back to legacy handling for any remaining properties
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

            // Note: Border properties (width, color, radius) are now handled by BaseStyles.Set()
            // Note: Visual properties (display, opacity, background-color, z-index, pointer-events) 
            // are now handled by BaseStyles.Set()
            
            // Background (for gradients)
            case "background": return SetBackground(value);

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
            // Note: Many CSS properties from s&box's BaseStyles.Generated.cs are not yet implemented
            // Unsupported properties are silently ignored to avoid console spam

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

    /// <summary>
    /// Parse background property - supports solid colors and gradients
    /// Ported from s&box's Styles.Set.cs SetBackground method
    /// </summary>
    private bool SetBackground(string value)
    {
        value = value.Trim();
        
        // Check for linear-gradient
        if (value.StartsWith("linear-gradient(", StringComparison.OrdinalIgnoreCase))
        {
            var contentStart = "linear-gradient(".Length;
            var contentEnd = value.LastIndexOf(')');
            if (contentEnd > contentStart)
            {
                var gradient = value.Substring(contentStart, contentEnd - contentStart);
                BackgroundGradient = ParseLinearGradient(gradient);
                BackgroundColor = null; // Clear solid color when using gradient
                return BackgroundGradient.HasValue;
            }
        }

        // Check for radial-gradient
        if (value.StartsWith("radial-gradient(", StringComparison.OrdinalIgnoreCase))
        {
            var contentStart = "radial-gradient(".Length;
            var contentEnd = value.LastIndexOf(')');
            if (contentEnd > contentStart)
            {
                var gradient = value.Substring(contentStart, contentEnd - contentStart);
                BackgroundGradient = ParseRadialGradient(gradient);
                BackgroundColor = null;
                return BackgroundGradient.HasValue;
            }
        }

        // Try parsing as a solid color
        BackgroundColor = UI.Color.Parse(value);
        BackgroundGradient = null; // Clear gradient when using solid color
        return BackgroundColor.HasValue;
    }

    /// <summary>
    /// Parse linear-gradient() syntax
    /// Ported from s&box's gradient parsing
    /// </summary>
    private GradientInfo? ParseLinearGradient(string token)
    {
        var info = new GradientInfo
        {
            GradientType = GradientInfo.GradientTypes.Linear,
            Angle = 180f * (MathF.PI / 180f) // Default: top to bottom (180deg in radians)
        };

        var p = new Parse(token);
        p = p.SkipWhitespaceAndNewlines();

        // Try to parse angle first
        var restoreP = p;
        var angleStr = p.ReadSentence();
        if (TryParseAngle(angleStr, out float angle))
        {
            info.Angle = angle;
            if (p.Current == ',')
                p.Pointer++;
        }
        else
        {
            p = restoreP;
        }

        // Parse color stops
        var colorOffsets = ParseGradientColorStops(p.ReadRemaining());
        if (colorOffsets.Count == 0)
            return null;

        info.ColorOffsets = System.Collections.Immutable.ImmutableArray.CreateRange(colorOffsets);
        return info;
    }

    /// <summary>
    /// Parse radial-gradient() syntax
    /// </summary>
    private GradientInfo? ParseRadialGradient(string token)
    {
        var info = new GradientInfo
        {
            GradientType = GradientInfo.GradientTypes.Radial,
            OffsetX = Length.Percent(50),
            OffsetY = Length.Percent(50),
            SizeMode = GradientInfo.RadialSizeMode.FarthestCorner
        };

        var p = new Parse(token);
        p = p.SkipWhitespaceAndNewlines();

        // Parse color stops (simplified - skip shape/size parsing for now)
        var colorOffsets = ParseGradientColorStops(p.ReadRemaining());
        if (colorOffsets.Count == 0)
            return null;

        info.ColorOffsets = System.Collections.Immutable.ImmutableArray.CreateRange(colorOffsets);
        return info;
    }

    /// <summary>
    /// Parse gradient color stops from CSS gradient syntax
    /// Ported from s&box's Styles.GradientGenerator.cs ParseGradient method
    /// </summary>
    private List<GradientInfo.GradientColorOffset> ParseGradientColorStops(string token)
    {
        var colorOffsets = new List<GradientInfo.GradientColorOffset>();

        var p = new Parse(token);

        while (!p.IsEnd)
        {
            p = p.SkipWhitespaceAndNewlines();

            // Read up to a comma or end
            var w = p.ReadSentence();
            if (string.IsNullOrWhiteSpace(w))
                break;

            w = w.Trim();
            
            // Split the sentence into color and optional offset
            // e.g., "#007acc 0%" -> color="#007acc", offset="0%"
            // e.g., "red" -> color="red", offset=null
            string colorPart;
            string? offsetPart = null;
            
            // Handle different color formats
            if (w.StartsWith("#"))
            {
                // Hex color - find the space after the hex
                var spaceIdx = w.IndexOf(' ');
                if (spaceIdx > 0)
                {
                    colorPart = w.Substring(0, spaceIdx);
                    offsetPart = w.Substring(spaceIdx + 1).Trim();
                }
                else
                {
                    colorPart = w;
                }
            }
            else if (w.StartsWith("rgb", StringComparison.OrdinalIgnoreCase) ||
                     w.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
            {
                // Function color - find closing paren
                var closeIdx = w.IndexOf(')');
                if (closeIdx >= 0 && closeIdx + 1 < w.Length)
                {
                    colorPart = w.Substring(0, closeIdx + 1);
                    offsetPart = w.Substring(closeIdx + 1).Trim();
                }
                else
                {
                    colorPart = w;
                }
            }
            else
            {
                // Named color or other - find first space
                var spaceIdx = w.IndexOf(' ');
                if (spaceIdx > 0)
                {
                    colorPart = w.Substring(0, spaceIdx);
                    offsetPart = w.Substring(spaceIdx + 1).Trim();
                }
                else
                {
                    colorPart = w;
                }
            }

            // Parse the color
            var c = UI.Color.Parse(colorPart);
            if (!c.HasValue)
                break;

            // Parse optional offset
            float? offset = null;
            if (!string.IsNullOrEmpty(offsetPart))
            {
                if (offsetPart.EndsWith("%"))
                {
                    if (float.TryParse(offsetPart.AsSpan(0, offsetPart.Length - 1),
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var pct))
                    {
                        offset = pct / 100f;
                    }
                }
                else if (float.TryParse(offsetPart,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var val))
                {
                    // Plain numeric values are treated as direct offsets (0.0-1.0 range)
                    // If value > 1, assume it's a percentage without the % sign
                    offset = val > 1f ? val / 100f : val;
                }
            }

            colorOffsets.Add(new GradientInfo.GradientColorOffset
            {
                color = c.Value,
                offset = offset
            });

            if (p.Is(','))
            {
                p.Pointer++;
            }
            else
            {
                break;
            }
        }

        // Fill in missing offset values
        if (colorOffsets.Count > 0)
        {
            // First color defaults to 0%
            if (!colorOffsets[0].offset.HasValue)
            {
                var first = colorOffsets[0];
                first.offset = 0f;
                colorOffsets[0] = first;
            }

            // Last color defaults to 100%
            if (colorOffsets.Count > 1 && !colorOffsets[^1].offset.HasValue)
            {
                var last = colorOffsets[^1];
                last.offset = 1f;
                colorOffsets[^1] = last;
            }

            // Interpolate missing middle values
            for (int i = 1; i < colorOffsets.Count - 1; i++)
            {
                if (!colorOffsets[i].offset.HasValue)
                {
                    var item = colorOffsets[i];
                    item.offset = (float)i / (colorOffsets.Count - 1);
                    colorOffsets[i] = item;
                }
            }
        }

        return colorOffsets;
    }

    /// <summary>
    /// Try to parse CSS angle value (deg, rad, turn, grad)
    /// </summary>
    private bool TryParseAngle(string value, out float outAngle)
    {
        outAngle = 0f;
        value = value.Trim();

        if (string.IsNullOrEmpty(value))
            return false;

        // Handle "to <direction>" syntax
        if (value.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
        {
            var direction = value.Substring(3).Trim().ToLowerInvariant();
            outAngle = direction switch
            {
                "top" => 0f,
                "right" => 90f,
                "bottom" => 180f,
                "left" => 270f,
                "top right" or "right top" => 45f,
                "bottom right" or "right bottom" => 135f,
                "bottom left" or "left bottom" => 225f,
                "top left" or "left top" => 315f,
                _ => 180f
            };
            outAngle = outAngle * (MathF.PI / 180f); // Convert to radians
            return true;
        }

        // Try parsing numeric angle with unit
        if (value.EndsWith("deg", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(value.AsSpan(0, value.Length - 3), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var deg))
            {
                outAngle = deg * (MathF.PI / 180f);
                return true;
            }
        }
        else if (value.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(value.AsSpan(0, value.Length - 3), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var rad))
            {
                outAngle = rad;
                return true;
            }
        }
        else if (value.EndsWith("turn", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(value.AsSpan(0, value.Length - 4), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var turn))
            {
                outAngle = turn * 2f * MathF.PI;
                return true;
            }
        }
        else if (value.EndsWith("grad", StringComparison.OrdinalIgnoreCase))
        {
            if (float.TryParse(value.AsSpan(0, value.Length - 4), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var grad))
            {
                outAngle = grad * (MathF.PI / 200f);
                return true;
            }
        }
        else
        {
            // Try parsing as plain number (assume degrees)
            if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var num))
            {
                outAngle = num * (MathF.PI / 180f);
                return true;
            }
        }

        return false;
    }

    #endregion
}
