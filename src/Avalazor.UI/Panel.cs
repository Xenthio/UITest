using SkiaSharp;

namespace Avalazor.UI;

/// <summary>
/// Base class for all UI elements in Avalazor.
/// Based on s&box's Sandbox.UI.Panel architecture (MIT licensed).
/// Provides a hierarchical structure with CSS-based styling and flexbox layout.
/// </summary>
public partial class Panel
{
    private Panel? _parent;
    private readonly List<Panel> _children = new();
    private ComputedStyle? _computedStyle;
    private LayoutNode? _layoutNode;
    private bool _needsLayout = true;
    private bool _needsStyleCompute = true;

    /// <summary>
    /// Parent panel in the hierarchy
    /// </summary>
    public Panel? Parent
    {
        get => _parent;
        internal set
        {
            if (_parent == value) return;
            _parent = value;
            MarkNeedsStyleCompute();
        }
    }

    /// <summary>
    /// Child panels
    /// </summary>
    public IReadOnlyList<Panel> Children => _children.AsReadOnly();

    /// <summary>
    /// CSS classes applied to this panel
    /// </summary>
    public HashSet<string> Classes { get; } = new();

    /// <summary>
    /// Inline style string (CSS)
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Computed position after layout
    /// </summary>
    public SKRect ComputedRect { get; internal set; }

    /// <summary>
    /// Whether this panel is visible
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Tag name for CSS selector matching (e.g., "div", "button")
    /// </summary>
    public string Tag { get; set; } = "div";

    public Panel()
    {
    }

    /// <summary>
    /// Add a child panel
    /// </summary>
    public virtual void AddChild(Panel child)
    {
        if (child.Parent != null)
        {
            child.Parent.RemoveChild(child);
        }

        _children.Add(child);
        child.Parent = this;
        MarkNeedsLayout();
    }

    /// <summary>
    /// Remove a child panel
    /// </summary>
    public virtual void RemoveChild(Panel child)
    {
        if (_children.Remove(child))
        {
            child.Parent = null;
            MarkNeedsLayout();
        }
    }

    /// <summary>
    /// Remove all children
    /// </summary>
    public virtual void RemoveAllChildren()
    {
        foreach (var child in _children.ToList())
        {
            RemoveChild(child);
        }
    }

    /// <summary>
    /// Mark this panel and all ancestors as needing layout recalculation
    /// </summary>
    protected void MarkNeedsLayout()
    {
        _needsLayout = true;
        Parent?.MarkNeedsLayout();
    }

    /// <summary>
    /// Mark this panel and all descendants as needing style recomputation
    /// </summary>
    protected void MarkNeedsStyleCompute()
    {
        _needsStyleCompute = true;
        foreach (var child in _children)
        {
            child.MarkNeedsStyleCompute();
        }
    }

    /// <summary>
    /// Add a CSS class
    /// </summary>
    public void AddClass(string className)
    {
        if (Classes.Add(className))
        {
            MarkNeedsStyleCompute();
        }
    }

    /// <summary>
    /// Remove a CSS class
    /// </summary>
    public void RemoveClass(string className)
    {
        if (Classes.Remove(className))
        {
            MarkNeedsStyleCompute();
        }
    }

    /// <summary>
    /// Toggle a CSS class
    /// </summary>
    public void ToggleClass(string className)
    {
        if (Classes.Contains(className))
            RemoveClass(className);
        else
            AddClass(className);
    }

    /// <summary>
    /// Check if panel has a CSS class
    /// </summary>
    public bool HasClass(string className) => Classes.Contains(className);

    /// <summary>
    /// Called when panel needs to paint itself
    /// Override to provide custom rendering
    /// </summary>
    protected virtual void OnPaint(SKCanvas canvas)
    {
        // Base implementation paints background, border, etc.
        PaintBackground(canvas);
        PaintBorder(canvas);
    }

    /// <summary>
    /// Paint the panel and its children
    /// </summary>
    internal void Paint(SKCanvas canvas)
    {
        if (!IsVisible) return;

        canvas.Save();
        canvas.Translate(ComputedRect.Left, ComputedRect.Top);

        OnPaint(canvas);

        // Paint children
        foreach (var child in _children)
        {
            child.Paint(canvas);
        }

        canvas.Restore();
    }

    private void PaintBackground(SKCanvas canvas)
    {
        if (_computedStyle?.BackgroundColor != null)
        {
            using var paint = new SKPaint
            {
                Color = _computedStyle.BackgroundColor.Value,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var rect = new SKRect(0, 0, ComputedRect.Width, ComputedRect.Height);
            var borderRadius = _computedStyle.BorderRadius ?? 0;

            if (borderRadius > 0)
            {
                canvas.DrawRoundRect(rect, borderRadius, borderRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
    }

    private void PaintBorder(SKCanvas canvas)
    {
        if (_computedStyle?.BorderWidth > 0 && _computedStyle?.BorderColor != null)
        {
            using var paint = new SKPaint
            {
                Color = _computedStyle.BorderColor.Value,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = _computedStyle.BorderWidth,
                IsAntialias = true
            };

            var rect = new SKRect(0, 0, ComputedRect.Width, ComputedRect.Height);
            var borderRadius = _computedStyle.BorderRadius ?? 0;

            if (borderRadius > 0)
            {
                canvas.DrawRoundRect(rect, borderRadius, borderRadius, paint);
            }
            else
            {
                canvas.DrawRect(rect, paint);
            }
        }
    }

    public override string ToString()
    {
        var classes = Classes.Count > 0 ? $".{string.Join(".", Classes)}" : "";
        return $"<{Tag}{classes}>";
    }
}
