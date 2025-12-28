namespace Sandbox.UI;

/// <summary>
/// Holds state that cascades down during layout computation.
/// Based on s&box's LayoutCascade from engine/Sandbox.Engine/Systems/UI/Data/LayoutCascade.cs
/// </summary>
public struct LayoutCascade
{
    /// <summary>
    /// True if style selectors need recalculation
    /// </summary>
    public bool SelectorChanged;

    /// <summary>
    /// True if parent's cascading styles changed
    /// </summary>
    public bool ParentChanged;

    /// <summary>
    /// Skip transitions during this layout pass
    /// </summary>
    public bool SkipTransitions;

    /// <summary>
    /// Parent's computed styles (for cascading properties)
    /// </summary>
    public Styles? ParentStyles;

    /// <summary>
    /// The root panel being laid out
    /// </summary>
    public RootPanel? Root;

    /// <summary>
    /// Current scale factor (e.g., for HiDPI displays)
    /// </summary>
    public float Scale;

    /// <summary>
    /// Apply cascading properties from parent to child styles
    /// </summary>
    public void ApplyCascading(Styles cached)
    {
        if (ParentStyles == null) return;
        cached.ApplyCascading(ParentStyles);
    }
}
