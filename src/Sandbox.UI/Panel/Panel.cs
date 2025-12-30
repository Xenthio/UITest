namespace Sandbox.UI;

/// <summary>
/// A simple User Interface panel. Can be styled with CSS.
/// Based on s&box's Panel from engine/Sandbox.Engine/Systems/UI/Panel/Panel.cs
/// </summary>
public partial class Panel : IDisposable, IStyleTarget
{
    /// <summary>
    /// The element name. This is typically the type name lowercased.
    /// </summary>
    public string ElementName { get; set; }

    /// <summary>
    /// HTML-like id for CSS selectors
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// A collection of stylesheets applied to this panel directly.
    /// </summary>
    public StyleSheetCollection StyleSheet;

    /// <summary>
    /// All stylesheets from this panel and its ancestors.
    /// </summary>
    public IEnumerable<StyleSheet> AllStyleSheets
    {
        get
        {
            foreach (var p in AncestorsAndSelf)
            {
                if (p.StyleSheet.List == null) continue;

                foreach (var sheet in p.StyleSheet.List)
                    yield return sheet;
            }
        }
    }

    /// <summary>
    /// Pseudo-class flags for styling (hover, active, etc.)
    /// </summary>
    public PseudoClass PseudoClass
    {
        get => _pseudoClass;
        set
        {
            if (_pseudoClass == value) return;
            _pseudoClass = value;
            StyleSelectorsChanged(true, true);
        }
    }
    private PseudoClass _pseudoClass = PseudoClass.Intro;

    /// <summary>
    /// Whether this panel has the :focus pseudo class active.
    /// </summary>
    public bool HasFocus => (PseudoClass & PseudoClass.Focus) != 0;

    /// <summary>
    /// Whether this panel has the :active pseudo class active.
    /// </summary>
    public bool HasActive => (PseudoClass & PseudoClass.Active) != 0;

    /// <summary>
    /// Whether this panel has the :hover pseudo class active.
    /// </summary>
    public bool HasHovered => (PseudoClass & PseudoClass.Hover) != 0;

    /// <summary>
    /// Whether this panel has the :intro pseudo class active.
    /// </summary>
    public bool HasIntro => (PseudoClass & PseudoClass.Intro) != 0;

    /// <summary>
    /// Whether this panel has the :outro pseudo class active.
    /// </summary>
    public bool HasOutro => (PseudoClass & PseudoClass.Outro) != 0;

    /// <summary>
    /// Direct style property access
    /// </summary>
    public PanelStyle Style { get; private set; }

    /// <summary>
    /// Computed style after CSS processing
    /// </summary>
    public Styles? ComputedStyle { get; internal set; }

    /// <summary>
    /// Yoga layout node
    /// </summary>
    public YogaWrapper? YogaNode { get; private set; }

    /// <summary>
    /// Whether this panel is valid and not deleted
    /// </summary>
    public bool IsValid() => YogaNode != null && !IsDeleted;

    /// <summary>
    /// Whether this panel is being deleted
    /// </summary>
    public bool IsDeleting { get; private set; }

    /// <summary>
    /// Whether this panel has been deleted
    /// </summary>
    public bool IsDeleted { get; private set; }

    public Panel()
    {
        YogaNode = new YogaWrapper(this);
        Style = new PanelStyle(this);
        StyleSheet = new StyleSheetCollection(this);

        ElementName = GetType().Name.ToLower();
        Switch(PseudoClass.Empty, true);

        LoadStyleSheet();
    }

    public Panel(Panel? parent) : this()
    {
        if (parent != null)
            Parent = parent;
    }

    public Panel(Panel? parent, string? classnames) : this(parent)
    {
        if (classnames != null)
            AddClass(classnames);
    }

    /// <summary>
    /// Switch a pseudo class on or off.
    /// </summary>
    public bool Switch(PseudoClass c, bool state)
    {
        if (state == ((PseudoClass & c) != 0)) return false;

        if (state)
            PseudoClass |= c;
        else
            PseudoClass &= ~c;

        return true;
    }

    /// <summary>
    /// Return true if this panel isn't hidden by opacity or display mode.
    /// </summary>
    public bool IsVisible { get; internal set; } = true;

    /// <summary>
    /// Return true if this panel itself is visible (not affected by parent).
    /// </summary>
    public bool IsVisibleSelf { get; internal set; } = true;

    /// <summary>
    /// Called every frame.
    /// </summary>
    public virtual void Tick()
    {
    }

    /// <summary>
    /// Called after the parent of this panel has changed.
    /// </summary>
    public virtual void OnParentChanged()
    {
    }

    /// <summary>
    /// Returns true if this panel would like the mouse cursor to be visible.
    /// </summary>
    public virtual bool WantsMouseInput()
    {
        if (ComputedStyle == null) return false;
        if (!IsVisibleSelf) return false;
        if (ComputedStyle.PointerEvents == UI.PointerEvents.All) return true;
        if (_children == null) return false;

        foreach (var child in _children)
        {
            if (child?.WantsMouseInput() ?? false)
                return true;
        }

        return false;
    }

    internal void TickInternal()
    {
        if (IsDeleting)
        {
            SetNeedsPreLayout();
            return;
        }

        try
        {
            if (ParentHasChanged)
            {
                ParentHasChanged = false;
                OnParentChanged();
                StyleSelectorsChanged(true, true);
            }

            // Tick styles if dirty
            if (Style.IsDirty)
            {
                SetNeedsPreLayout();
            }

            // Tick visible children
            if (IsVisible && _children != null && _children.Count > 0)
            {
                for (int i = _children.Count - 1; _children != null && i >= 0; i--)
                {
                    _children[i]?.TickInternal();
                }
            }

            Tick();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Panel.TickInternal error: {e}");
        }
    }

    /// <summary>
    /// Called when the panel should mark that styles need to be rebuilt
    /// </summary>
    internal void MarkStylesRebuilt()
    {
        inRebuildStyleRulesList = false;
        inRebuildStyleRulesList_Ancestors = false;
        inRebuildStyleRulesList_Children = false;
    }

    bool inRebuildStyleRulesList;
    bool inRebuildStyleRulesList_Ancestors;
    bool inRebuildStyleRulesList_Children;

    /// <summary>
    /// Should be called when something happens that means that this panel's stylesheets need to be
    /// re-evaluated. Like becoming hovered or classes changed. You don't call this when changing styles
    /// directly on the panel, just on anything that will change which stylesheets should get selected.
    /// </summary>
    /// <param name="ancestors">Also re-evaluate all ancestor panels.</param>
    /// <param name="descendants">Also re-evaluate all child panels.</param>
    /// <param name="root">Root panel cache so we don't need to keep looking it up.</param>
    internal virtual void StyleSelectorsChanged(bool ancestors, bool descendants, RootPanel? root = null)
    {
        root ??= FindRootPanel();
        if (root == null)
            return;

        if (ancestors && !inRebuildStyleRulesList_Ancestors)
        {
            inRebuildStyleRulesList_Ancestors = true;
            Parent?.StyleSelectorsChanged(true, false, root);
        }

        if (descendants && !inRebuildStyleRulesList_Children && HasChildren)
        {
            inRebuildStyleRulesList_Children = true;

            foreach (var child in Children)
            {
                child.StyleSelectorsChanged(false, true, root);
            }
        }

        if (!inRebuildStyleRulesList)
        {
            inRebuildStyleRulesList = true;
            root.AddToBuildStyleRulesList(this);
        }
    }

    public override string ToString()
    {
        var classes = _classes?.Count > 0 ? $".{string.Join(".", _classes)}" : "";
        return $"<{ElementName}{classes}>";
    }

    #region IDisposable

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        YogaNode?.Dispose();
        YogaNode = null;

        GC.SuppressFinalize(this);
    }

    ~Panel()
    {
        Dispose();
    }
    // Source location tracking (for debugging Razor-generated panels)
    public string? SourceFile { get; set; }
    public int SourceLine { get; set; }
    
    // Event listener management (stub for S&box compatibility)
    public void AddEventListener(string eventName, Action<PanelEvent> handler)
    {
        // TODO: Implement event system
    }
    
    public void RemoveEventListener(string eventName, Action<PanelEvent> handler)
    {
        // TODO: Implement event system
    }
    
    // Parameter change notification (stub for S&box compatibility)
    public void ParametersChanged(bool firstTime)
    {
        // TODO: Implement parameter tracking
    }
}
    #endregion
