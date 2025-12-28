namespace Sandbox.UI;

/// <summary>
/// Per-panel style handler. Manages inline styles and computed styles.
/// Based on s&box's PanelStyle from engine/Sandbox.Engine/Systems/UI/PanelStyle.cs
/// </summary>
public class PanelStyle : Styles
{
    private readonly Panel _panel;
    private bool _isDirty = true;

    public PanelStyle(Panel panel)
    {
        _panel = panel;
    }

    /// <summary>
    /// Whether the style needs recalculation
    /// </summary>
    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (value && !_isDirty)
            {
                _isDirty = true;
                _panel.SetNeedsPreLayout();
            }
            _isDirty = value;
        }
    }

    /// <summary>
    /// Mark this style as dirty, requiring recalculation
    /// </summary>
    public void Dirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Build the final computed styles for this panel
    /// </summary>
    public Styles BuildFinal(ref LayoutCascade cascade, out bool changed)
    {
        changed = _isDirty;
        _isDirty = false;

        // Create a new Styles instance with our values
        var result = new Styles();

        // First apply any stylesheet rules (TODO: implement stylesheet system)
        // For now, we just use inline styles

        // Apply our inline style properties
        result.Add(this);

        // Apply cascading from parent
        cascade.ApplyCascading(result);

        return result;
    }

    /// <summary>
    /// Build style rules in a thread-safe manner (for parallel processing)
    /// Returns true if rules changed
    /// </summary>
    public bool BuildRulesInThread()
    {
        // Simplified implementation - just mark as needing rebuild
        return _isDirty;
    }
}
