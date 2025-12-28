namespace Sandbox.UI;

/// <summary>
/// Per-panel style handler. Manages inline styles and computed styles.
/// Based on s&box's PanelStyle from engine/Sandbox.Engine/Systems/UI/PanelStyle.cs
/// </summary>
public class PanelStyle : Styles
{
    private readonly Panel _panel;
    private bool _isDirty = true;
    private bool _broadphaseDirty = true;

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
    public new void Dirty()
    {
        IsDirty = true;
    }

    /// <summary>
    /// Invalidate the broadphase cache for style matching
    /// </summary>
    public void InvalidateBroadphase()
    {
        _broadphaseDirty = true;
        _panel?.StyleSelectorsChanged(true, true);
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

        // Apply stylesheet rules from all stylesheets
        ApplyStyleSheetRules(result);

        // Apply our inline style properties (inline styles override stylesheet)
        result.Add(this);

        // Apply cascading from parent
        cascade.ApplyCascading(result);

        return result;
    }

    /// <summary>
    /// Apply matching CSS rules from stylesheets
    /// </summary>
    private void ApplyStyleSheetRules(Styles result)
    {
        if (_panel == null) return;

        var matchingRules = new List<(StyleSelector selector, StyleBlock block)>();

        // Collect all matching rules from all stylesheets
        int sheetCount = 0;
        foreach (var sheet in _panel.AllStyleSheets)
        {
            sheetCount++;
            foreach (var block in sheet.Nodes)
            {
                var selector = block.Test(_panel);
                if (selector != null)
                {
                    matchingRules.Add((selector, block));
                }
            }
        }

        // Debug: Log if no stylesheets or rules found for window elements
        if (_panel.HasClass("window") && matchingRules.Count == 0)
        {
            System.Console.WriteLine($"Warning: Panel with class 'window' has {sheetCount} stylesheets but no matching rules. Panel: {_panel.GetType().Name}, Classes: {string.Join(", ", _panel.Classes)}");
        }

        // Sort by specificity (score)
        matchingRules.Sort((a, b) => a.selector.Score.CompareTo(b.selector.Score));

        // Apply rules in order
        foreach (var (selector, block) in matchingRules)
        {
            result.Add(block.Styles);
        }
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
