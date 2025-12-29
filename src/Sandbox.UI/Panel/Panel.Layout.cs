namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Layout computation
/// Based on s&box's Panel.Layout.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Access to various bounding boxes of this panel.
    /// </summary>
    public Box Box { get; } = new Box();

    /// <summary>
    /// Scale of the panel on the screen.
    /// </summary>
    public float ScaleToScreen { get; internal set; } = 1.0f;

    /// <summary>
    /// Inverse scale
    /// </summary>
    public float ScaleFromScreen => 1.0f / ScaleToScreen;

    /// <summary>
    /// Offset of children for scrolling
    /// </summary>
    public Vector2 ScrollOffset { get; set; }

    /// <summary>
    /// The velocity of the current scroll
    /// </summary>
    public Vector2 ScrollVelocity;

    /// <summary>
    /// Size of the scrollable area
    /// </summary>
    public Vector2 ScrollSize { get; private set; }

    /// <summary>
    /// The currently calculated opacity.
    /// </summary>
    public float Opacity { get; private set; } = 1.0f;

    /// <summary>
    /// If true, calls DrawContent
    /// </summary>
    public virtual bool HasContent => false;

    /// <summary>
    /// The computed style has a renderable background
    /// </summary>
    public bool HasBackground { get; internal set; }

    internal bool needsPreLayout = true;
    internal bool needsFinalLayout = true;

    internal void SetNeedsPreLayout()
    {
        if (needsPreLayout) return;
        needsPreLayout = true;
        needsFinalLayout = true;
        Parent?.SetNeedsPreLayout();
    }

    internal virtual void PreLayout(LayoutCascade cascade)
    {
        if (YogaNode == null) return;

        if (!needsPreLayout && !cascade.SelectorChanged && !cascade.ParentChanged)
            return;

        needsPreLayout = false;

        if (IndexesDirty)
        {
            UpdateChildrenIndexes();
        }

        // Build computed style
        ComputedStyle = Style.BuildFinal(ref cascade, out bool changed);
        cascade.ParentStyles = ComputedStyle;

        PushLengthValues();

        ScaleToScreen = cascade.Scale;
        Opacity = (ComputedStyle.Opacity ?? 1.0f) * (Parent?.Opacity ?? 1.0f);
        UpdateVisibility();

        if (changed || !YogaNode.Initialized)
        {
            UpdateYoga();
        }

        // Update HasBackground based on current ComputedStyle
        // This needs to be checked every PreLayout, not just when changed is true
        HasBackground = ComputedStyle.BackgroundColor?.a > 0f ||
            (ComputedStyle.BackgroundGradient?.IsValid ?? false) ||
            (ComputedStyle.BorderLeftColor?.a > 0f && ComputedStyle.BorderLeftWidth?.GetPixels(1.0f) > 0f) ||
            (ComputedStyle.BorderTopColor?.a > 0f && ComputedStyle.BorderTopWidth?.GetPixels(1.0f) > 0f) ||
            (ComputedStyle.BorderRightColor?.a > 0f && ComputedStyle.BorderRightWidth?.GetPixels(1.0f) > 0f) ||
            (ComputedStyle.BorderBottomColor?.a > 0f && ComputedStyle.BorderBottomWidth?.GetPixels(1.0f) > 0f);

        if (changed)
        {
            _renderChildrenDirty = true;
        }

        if (!IsVisibleSelf) return;

        if (_children == null || _children.Count == 0) return;

        cascade.ParentChanged = cascade.ParentChanged || changed;

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].PreLayout(cascade);
        }

        SortChildrenOrder();
    }

    internal void UpdateYoga()
    {
        if (ComputedStyle == null || YogaNode == null) return;

        YogaNode.Width = ComputedStyle.Width;
        YogaNode.Height = ComputedStyle.Height;
        YogaNode.MaxWidth = ComputedStyle.MaxWidth;
        YogaNode.MaxHeight = ComputedStyle.MaxHeight;
        YogaNode.MinWidth = ComputedStyle.MinWidth;
        YogaNode.MinHeight = ComputedStyle.MinHeight;
        YogaNode.Display = ComputedStyle.Display;

        YogaNode.Left = ComputedStyle.Left;
        YogaNode.Right = ComputedStyle.Right;
        YogaNode.Top = ComputedStyle.Top;
        YogaNode.Bottom = ComputedStyle.Bottom;

        YogaNode.MarginLeft = ComputedStyle.MarginLeft;
        YogaNode.MarginRight = ComputedStyle.MarginRight;
        YogaNode.MarginTop = ComputedStyle.MarginTop;
        YogaNode.MarginBottom = ComputedStyle.MarginBottom;

        YogaNode.PaddingLeft = ComputedStyle.PaddingLeft;
        YogaNode.PaddingRight = ComputedStyle.PaddingRight;
        YogaNode.PaddingTop = ComputedStyle.PaddingTop;
        YogaNode.PaddingBottom = ComputedStyle.PaddingBottom;

        YogaNode.BorderLeftWidth = ComputedStyle.BorderLeftWidth;
        YogaNode.BorderTopWidth = ComputedStyle.BorderTopWidth;
        YogaNode.BorderRightWidth = ComputedStyle.BorderRightWidth;
        YogaNode.BorderBottomWidth = ComputedStyle.BorderBottomWidth;

        YogaNode.PositionType = ComputedStyle.Position;
        YogaNode.AspectRatio = ComputedStyle.AspectRatio;
        YogaNode.FlexGrow = ComputedStyle.FlexGrow;
        YogaNode.FlexShrink = ComputedStyle.FlexShrink;
        YogaNode.FlexBasis = ComputedStyle.FlexBasis;
        YogaNode.Wrap = ComputedStyle.FlexWrap;

        YogaNode.AlignContent = ComputedStyle.AlignContent;
        YogaNode.AlignItems = ComputedStyle.AlignItems;
        YogaNode.AlignSelf = ComputedStyle.AlignSelf;
        YogaNode.FlexDirection = ComputedStyle.FlexDirection;
        YogaNode.JustifyContent = ComputedStyle.JustifyContent;
        YogaNode.Overflow = ComputedStyle.Overflow;

        YogaNode.RowGap = ComputedStyle.RowGap;
        YogaNode.ColumnGap = ComputedStyle.ColumnGap;

        YogaNode.Initialized = true;
    }

    private void PushLengthValues()
    {
        Length.CurrentFontSize = ComputedStyle?.FontSize?.GetPixels(16f) ?? 16f;
    }

    internal void UpdateVisibility()
    {
        bool old = IsVisible;

        IsVisibleSelf = ComputedStyle?.Display != DisplayMode.None && (ComputedStyle?.Opacity ?? 1f) > 0;
        IsVisible = IsVisibleSelf && (Parent?.IsVisible ?? true);

        if (old == IsVisible) return;

        IndexesDirty = true;

        var c = _children?.Count ?? 0;
        for (int i = 0; i < c; i++)
        {
            _children![i].UpdateVisibility();
        }
    }

    /// <summary>
    /// This panel has just been laid out. You can modify its position now.
    /// </summary>
    public virtual void OnLayout(ref Rect layoutRect)
    {
    }

    private int layoutHash;

    /// <summary>
    /// Final layout pass - reads Yoga results and updates Box rects.
    /// </summary>
    public virtual void FinalLayout(Vector2 offset)
    {
        if (ComputedStyle == null) return;
        if (YogaNode == null) return;

        PushLengthValues();

        var hash = HashCode.Combine(offset, ScrollOffset, ScrollVelocity, Opacity, ComputedStyle.Display);
        if (layoutHash == hash && !YogaNode.HasNewLayout && !needsFinalLayout) return;

        needsFinalLayout = false;
        layoutHash = hash;

        Box.Rect = YogaNode.YogaRect;
        Box.Rect.Position += offset;

        OnLayout(ref Box.Rect);

        Box.Padding = YogaNode.Padding;
        Box.Margin = YogaNode.Margin;
        Box.Border = YogaNode.Border;

        Box.RectOuter = Box.Rect.Grow(YogaNode.Margin.Left, YogaNode.Margin.Top, YogaNode.Margin.Right, YogaNode.Margin.Bottom);
        Box.RectInner = Box.Rect.Shrink(YogaNode.Padding.Left, YogaNode.Padding.Top, YogaNode.Padding.Right, YogaNode.Padding.Bottom);
        Box.ClipRect = Box.Rect.Shrink(YogaNode.Border.Left, YogaNode.Border.Top, YogaNode.Border.Right, YogaNode.Border.Bottom);

        Box.Rect = Box.Rect.Floor();
        Box.RectOuter = Box.RectOuter.Floor();
        Box.RectInner = Box.RectInner.Floor();
        Box.ClipRect = Box.ClipRect.Floor();

        // Remove intro pseudo class after first layout
        if (HasIntro)
        {
            Switch(PseudoClass.Intro, false);
        }

        if (ComputedStyle.Display == DisplayMode.None) return;
        if (Opacity <= 0.0f) return;

        offset = new Vector2(Box.Rect.Left, Box.Rect.Top) - ScrollOffset;
        FinalLayoutChildren(offset);
    }

    protected virtual void FinalLayoutChildren(Vector2 offset)
    {
        if (!HasChildren) return;

        for (int i = 0; i < _children!.Count; i++)
        {
            try
            {
                _children[i].FinalLayout(offset);
            }
            catch (Exception e)
            {
                Console.WriteLine($"FinalLayout error: {e}");
            }
        }

        if (ComputedStyle?.Overflow == OverflowMode.Scroll)
        {
            var rect = Box.Rect;
            rect.Position -= ScrollOffset;

            for (int i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                if (child.IsVisible)
                {
                    rect.Add(child.Box.RectOuter);
                }
            }

            rect.Height += Box.Padding.Bottom;
            rect.Right += Box.Padding.Right;

            ConstrainScrolling(new Vector2(rect.Width, rect.Height));
        }
        else
        {
            ScrollOffset = Vector2.Zero;
        }
    }

    protected virtual void ConstrainScrolling(Vector2 size)
    {
        size -= new Vector2(Box.Rect.Width, Box.Rect.Height);
        ScrollSize = size;

        var so = ScrollOffset;

        // Constrain
        if (so.y < 0) so.y = 0;
        if (so.x < 0) so.x = 0;
        if (so.y > ScrollSize.y) so.y = ScrollSize.y;
        if (so.x > ScrollSize.x) so.x = ScrollSize.x;

        ScrollOffset = so;
    }

    internal void SortChildrenOrder()
    {
        // Sort children by z-index for rendering
        if (_renderChildren != null && _renderChildrenDirty)
        {
            _renderChildren.Sort((x, y) => (x.ComputedStyle?.ZIndex ?? 0) - (y.ComputedStyle?.ZIndex ?? 0));
            _renderChildrenDirty = false;
        }
    }

    internal int GetRenderOrderIndex()
    {
        return SiblingIndex + (ComputedStyle?.ZIndex ?? 0);
    }

    /// <summary>
    /// Convert a point from the screen to a position relative to this panel
    /// </summary>
    public Vector2 ScreenPositionToPanelPosition(Vector2 pos)
    {
        return new Vector2(pos.x - Box.Rect.Left, pos.y - Box.Rect.Top);
    }

    /// <summary>
    /// Convert a point from local space to screen space
    /// </summary>
    public Vector2 PanelPositionToScreenPosition(Vector2 pos)
    {
        return new Vector2(pos.x + Box.Rect.Left, pos.y + Box.Rect.Top);
    }
}
