namespace Sandbox.UI;

/// <summary>
/// A root panel. Serves as a container for other panels, handles things such as rendering.
/// Based on s&box's RootPanel from engine/Sandbox.Engine/Systems/UI/RootPanel.cs
/// </summary>
public partial class RootPanel : Panel
{
    /// <summary>
    /// Bounds of the panel (size and position on screen)
    /// </summary>
    public Rect PanelBounds { get; set; } = new Rect(0, 0, 512, 512);

    /// <summary>
    /// If any of our panels are visible and want mouse input
    /// </summary>
    internal bool ChildrenWantMouseInput { get; set; }

    /// <summary>
    /// The scale of this panel and its children
    /// </summary>
    public float Scale { get; protected set; } = 1.0f;

    /// <summary>
    /// If set to true this panel won't be rendered to the screen like a normal panel
    /// </summary>
    public bool RenderedManually { get; set; }

    /// <summary>
    /// True if this is a world panel
    /// </summary>
    public virtual bool IsWorldPanel { get; set; }

    /// <summary>
    /// Current global mouse position
    /// </summary>
    internal Vector2 MousePos;

    public RootPanel()
    {
        Style.Width = Length.Percent(100);
        Style.Height = Length.Percent(100);
    }

    public override void Delete(bool immediate = true)
    {
        base.Delete(immediate);
    }

    /// <summary>
    /// This is called from tests to emulate the regular root panel simulate loop
    /// </summary>
    public void Layout()
    {
        TickInternal();
        PreLayout();
        CalculateLayout();
        PostLayout();
    }

    /// <summary>
    /// Force a complete re-layout of all children.
    /// Call this when the root panel bounds have changed significantly.
    /// </summary>
    public void InvalidateLayout()
    {
        layoutHash = 0;
        StyleSelectorsChanged(true, true);
        SkipAllTransitions();
    }

    private int layoutHash;

    /// <summary>
    /// Called before layout to lock the bounds of this root panel
    /// </summary>
    protected virtual void UpdateBounds(Rect rect)
    {
        PanelBounds = rect;
    }

    /// <summary>
    /// Work out scaling here. Default is to scale relative to 1080p height.
    /// </summary>
    protected virtual void UpdateScale(Rect screenSize)
    {
        Scale = screenSize.Height / 1080.0f;
        Scale = Math.Clamp(Scale, 0.1f, 10.0f);
    }

    internal void TickInputInternal()
    {
        ChildrenWantMouseInput = WantsMouseInput();
    }

    internal void PreLayout(Rect screenSize)
    {
        UpdateBounds(screenSize);
        UpdateScale(PanelBounds);
        PreLayout();
    }

    internal void PreLayout()
    {
        var cascade = new LayoutCascade
        {
            Scale = Scale,
            Root = this,
        };

        Style.Left = 0.0f;
        Style.Top = 0.0f;
        Style.Width = Length.Pixels(PanelBounds.Width * (1 / Scale));
        Style.Height = Length.Pixels(PanelBounds.Height * (1 / Scale));

        var hash = HashCode.Combine(PanelBounds.Width, PanelBounds.Height, Scale);
        if (hash != layoutHash)
        {
            layoutHash = hash;
            StyleSelectorsChanged(true, true);
            SkipAllTransitions();
            cascade.SelectorChanged = true;
        }

        BuildStyleRules();

        PushRootValues();

        base.PreLayout(cascade);
    }

    internal void CalculateLayout()
    {
        if (YogaNode == null) return;

        PushRootValues();
        YogaNode.CalculateLayout();
    }

    internal void PostLayout()
    {
        PushRootValues();
        FinalLayout(Vector2.Zero);
    }

    internal void PushRootValues()
    {
        Length.RootSize = new Vector2(PanelBounds.Width, PanelBounds.Height);
        Length.RootFontSize = ComputedStyle?.FontSize?.GetPixels(16f) ?? 16f;
        Length.RootScale = ScaleToScreen;
    }

    public override void OnLayout(ref Rect layoutRect)
    {
        layoutRect = PanelBounds;
    }

    /// <summary>
    /// Render this panel manually. Must have RenderedManually set to true.
    /// </summary>
    public void RenderManual(IPanelRenderer renderer, float opacity = 1.0f)
    {
        if (!RenderedManually && !IsWorldPanel)
            throw new Exception($"{nameof(RenderedManually)} must be set to true to render this panel manually.");

        renderer.Render(this, opacity);
    }

    internal void SkipAllTransitions()
    {
        // Skip transitions implementation
    }

    /// <summary>
    /// A list of panels waiting to have their styles re-evaluated
    /// </summary>
    private readonly HashSet<Panel> styleRuleUpdates = new();

    /// <summary>
    /// Add panel to list to have styles re-evaluated
    /// </summary>
    internal void AddToBuildStyleRulesList(Panel panel)
    {
        styleRuleUpdates.Add(panel);
    }

    /// <summary>
    /// Run through all panels pending a re-check on their style rules
    /// </summary>
    internal void BuildStyleRules()
    {
        if (styleRuleUpdates.Count == 0) return;

        foreach (var panel in styleRuleUpdates)
        {
            if (!panel.IsValid) continue;

            if (panel.Style.BuildRulesInThread())
            {
                panel.SetNeedsPreLayout();
            }

            panel.MarkStylesRebuilt();
        }

        styleRuleUpdates.Clear();
    }
}
