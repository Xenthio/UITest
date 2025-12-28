using SkiaSharp;
using Avalazor.UI.Yoga;

namespace Avalazor.UI;

/// <summary>
/// Base class for all UI elements in Avalazor.
/// Based on s&box's Sandbox.UI.Panel architecture (MIT licensed).
/// Provides a hierarchical structure with CSS-based styling and flexbox layout.
/// 
/// This class is split into multiple partial classes for organization:
/// - Panel.cs: Core properties and initialization
/// - Panel.Children.cs: Child management
/// - Panel.Classes.cs: CSS class management
/// - Panel.Layout.cs: Layout computation (PreLayout/FinalLayout)
/// - Panel.Render.cs: Rendering (Paint methods)
/// </summary>
public partial class Panel
{
    internal Panel? _parent;
    internal readonly List<Panel> _children = new();
    internal ComputedStyle? _computedStyle;
    internal bool needsPreLayout = true;
    internal StyleEngine? _styleEngine;

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
            needsPreLayout = true;
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
    /// Access to various bounding boxes of this panel.
    /// Based on s&box's Box property (Panel.Layout.cs line 13)
    /// </summary>
    public Box Box { get; init; } = new Box();

    /// <summary>
    /// Yoga layout node - each panel owns its own Yoga node.
    /// Based on s&box's YogaNode property (Panel.Layout.cs line 7)
    /// </summary>
    internal YogaWrapper? YogaNode { get; private set; }

    /// <summary>
    /// Computed style after CSS processing
    /// </summary>
    public ComputedStyle? ComputedStyle => _computedStyle;

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
        // Initialize Yoga node (s&box pattern)
        YogaNode = new YogaWrapper();
    }

    /// <summary>
    /// Mark this panel as needing layout recalculation
    /// </summary>
    protected void MarkNeedsLayout()
    {
        needsPreLayout = true;
        Parent?.MarkNeedsLayout();
    }

    /// <summary>
    /// Set the StyleEngine to use for style computation
    /// </summary>
    internal void SetStyleEngine(StyleEngine engine)
    {
        _styleEngine = engine;
    }

    public override string ToString()
    {
        var classes = Classes.Count > 0 ? $".{string.Join(".", Classes)}" : "";
        return $"<{Tag}{classes}>";
    }
}

/// <summary>
/// Mouse event arguments
/// </summary>
public class MouseEventArgs
{
    public float X { get; set; }
    public float Y { get; set; }
    public int Button { get; set; } // 0=left, 1=right, 2=middle
    public bool Handled { get; set; }
}
