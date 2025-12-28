namespace Sandbox.UI;

/// <summary>
/// A simple User Interface panel. Can be styled with CSS.
/// Based on s&box's Panel from engine/Sandbox.Engine/Systems/UI/Panel/Panel.cs
/// </summary>
public partial class Panel : IDisposable
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
    public bool IsValid => YogaNode != null && !IsDeleted;

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

        ElementName = GetType().Name.ToLower();
        Switch(PseudoClass.Empty, true);
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
    /// Called when style selectors may have changed
    /// </summary>
    internal virtual void StyleSelectorsChanged(bool includeChildren, bool includeSiblings)
    {
        SetNeedsPreLayout();

        if (includeChildren && _children != null)
        {
            foreach (var child in _children)
            {
                child?.StyleSelectorsChanged(true, false);
            }
        }

        // Add to root panel's style rebuild list
        FindRootPanel()?.AddToBuildStyleRulesList(this);
    }

    /// <summary>
    /// Called when the panel should mark that styles need to be rebuilt
    /// </summary>
    internal void MarkStylesRebuilt()
    {
        // Nothing needed in simplified implementation
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

    #endregion
}
