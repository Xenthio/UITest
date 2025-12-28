namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Child management
/// Based on s&box's Panel.Children.cs
/// </summary>
public partial class Panel
{
    internal List<Panel>? _children;
    internal List<Panel>? _renderChildren;
    internal HashSet<Panel>? _childrenHash;
    internal Panel? _parent;
    internal bool _renderChildrenDirty;
    internal bool ParentHasChanged;
    internal bool IndexesDirty;

    /// <summary>
    /// List of panels that are attached/parented directly to this one.
    /// </summary>
    public IEnumerable<Panel> Children => _children == null ? Enumerable.Empty<Panel>() : _children.Where(x => x != null);

    /// <summary>
    /// Whether this panel has any child panels at all.
    /// </summary>
    public bool HasChildren => _children != null && _children.Count > 0;

    /// <summary>
    /// Amount of panels directly parented to this panel.
    /// </summary>
    public int ChildrenCount => _children?.Count ?? 0;

    /// <summary>
    /// The panel we are directly attached to.
    /// </summary>
    public Panel? Parent
    {
        get => _parent;
        set
        {
            if (this is RootPanel && value != null)
                throw new Exception("Can't parent a RootPanel");

            if (value == this)
                throw new Exception("Can't parent a panel to itself");

            if (_parent == value) return;

            // Can't parent to panels without children (Label, Image)
            if (value is Label or Image)
            {
                Parent = value.Parent;
                Parent?.SetChildIndex(this, Parent.GetChildIndex(value) + 1);
                return;
            }

            var oldParent = _parent;
            _parent = null;

            oldParent?.RemoveChild(this);

            _parent = value;

            if (_parent != null)
            {
                _parent.InternalAddChild(this);
                ScaleToScreen = _parent.ScaleToScreen;
            }

            ParentHasChanged = true;
        }
    }

    /// <summary>
    /// The index of this panel in its parent's child list.
    /// </summary>
    public int SiblingIndex { get; internal set; } = -1;

    private void RemoveChild(Panel p)
    {
        if (IsDeleted) return;
        if (_children == null) throw new Exception("RemoveChild but no children!");

        if (_childrenHash?.Remove(p) ?? false)
        {
            _children.Remove(p);
            _renderChildren?.Remove(p);
            _renderChildrenDirty = true;

            if (p.YogaNode != null)
            {
                YogaNode?.RemoveChild(p.YogaNode);
            }

            OnChildRemoved(p);
            IndexesDirty = true;
            SetNeedsPreLayout();
        }
    }

    protected virtual void OnChildRemoved(Panel child) { }

    /// <summary>
    /// Deletes all child panels.
    /// </summary>
    public void DeleteChildren(bool immediate = false)
    {
        foreach (var child in Children.ToArray())
        {
            child.Delete(immediate);
        }
    }

    /// <summary>
    /// Add given panel as a child to this panel.
    /// </summary>
    public T AddChild<T>(T p) where T : Panel
    {
        if (IsDeleted) throw new Exception("Cannot add child to deleted panel");
        p.Parent = this;
        return p;
    }

    private void InternalAddChild(Panel child)
    {
        if (YogaNode?.IsMeasureDefined == true)
            throw new Exception($"{this} can not have children.");

        _children ??= new(4);
        _renderChildren ??= new(4);
        _childrenHash ??= new(4);

        if (_childrenHash.Contains(child))
            throw new Exception("AddChild but already have child!");

        YogaNode?.AddChild(child.YogaNode);

        _childrenHash.Add(child);
        _children.Add(child);
        _renderChildren.Add(child);
        _renderChildrenDirty = true;

        child.UpdateSiblingIndex(_children.Count - 1, _children.Count);
        OnChildAdded(child);
        SetNeedsPreLayout();

        IndexesDirty = true;
    }

    protected virtual void OnChildAdded(Panel child) { }

    /// <summary>
    /// Sort the children using given comparison function.
    /// </summary>
    public void SortChildren(Comparison<Panel> sorter)
    {
        if (_children == null || _children.Count <= 0) return;

        _children.RemoveAll(x => x == null);
        _children.Sort(sorter);

        int i = 0;
        foreach (var child in _children)
        {
            child.UpdateSiblingIndex(i++, _children.Count);
            YogaNode?.RemoveChild(child.YogaNode);
            YogaNode?.AddChild(child.YogaNode);
        }

        IndexesDirty = true;
    }

    void UpdateChildrenIndexes()
    {
        IndexesDirty = false;
        Switch(PseudoClass.Empty, IsPanelEmpty());

        var count = ChildrenCount;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            _children![i].UpdateSiblingIndex(i, count);
        }
    }

    internal void UpdateSiblingIndex(int index, int siblings)
    {
        Switch(PseudoClass.FirstChild, index == 0);
        Switch(PseudoClass.LastChild, index == siblings - 1);
        Switch(PseudoClass.OnlyChild, index == 0 && siblings == 1);
        SiblingIndex = index;
    }

    protected virtual bool IsPanelEmpty() => ChildrenCount == 0;

    /// <summary>
    /// Creates a panel of given type and makes it our child.
    /// </summary>
    public T AddChild<T>(string? classnames = null) where T : Panel, new()
    {
        var t = new T();
        t.Parent = this;
        if (classnames != null)
            t.AddClass(classnames);
        return t;
    }

    /// <summary>
    /// Returns this panel and all its ancestors.
    /// </summary>
    public IEnumerable<Panel> AncestorsAndSelf
    {
        get
        {
            var p = this;
            while (p != null)
            {
                yield return p;
                p = p.Parent;
            }
        }
    }

    /// <summary>
    /// Returns all ancestors.
    /// </summary>
    public IEnumerable<Panel> Ancestors
    {
        get
        {
            var p = Parent;
            while (p != null)
            {
                yield return p;
                p = p.Parent;
            }
        }
    }

    /// <summary>
    /// All descendants (children, grandchildren, etc.)
    /// </summary>
    public IEnumerable<Panel> Descendants
    {
        get
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var descendant in child.Descendants)
                    yield return descendant;
            }
        }
    }

    /// <summary>
    /// Is the given panel a parent, grandparent, etc.
    /// </summary>
    public bool IsAncestor(Panel panel)
    {
        if (panel == this) return true;
        if (Parent != null) return Parent.IsAncestor(panel);
        return false;
    }

    /// <summary>
    /// Returns the RootPanel we are ultimately attached to.
    /// </summary>
    public RootPanel? FindRootPanel()
    {
        if (this is RootPanel rp) return rp;
        return Parent?.FindRootPanel();
    }

    /// <summary>
    /// Returns the index at which the given panel is parented.
    /// </summary>
    public int GetChildIndex(Panel? panel)
    {
        if (panel == null || panel.Parent != this) return -1;
        if (_children == null || _children.Count == 0) return -1;
        return _children.IndexOf(panel);
    }

    /// <summary>
    /// Set the index of a child panel.
    /// </summary>
    public void SetChildIndex(Panel panel, int index)
    {
        if (panel.Parent != this) return;
        if (_children == null) return;

        _children.Remove(panel);
        index = Math.Clamp(index, 0, _children.Count);
        _children.Insert(index, panel);

        YogaNode?.RemoveChild(panel.YogaNode);

        // Re-add all children to yoga in new order
        for (int i = 0; i < _children.Count; i++)
        {
            var child = _children[i];
            YogaNode?.RemoveChild(child.YogaNode);
            YogaNode?.AddChild(child.YogaNode);
        }

        IndexesDirty = true;
    }

    /// <summary>
    /// Return a child at given index.
    /// </summary>
    public Panel? GetChild(int index, bool loop = false)
    {
        if (_children == null || _children.Count == 0) return null;

        if (loop)
        {
            index = ((index % _children.Count) + _children.Count) % _children.Count;
        }
        else
        {
            if (index < 0) return null;
            if (index >= _children.Count) return null;
        }

        return _children[index];
    }

    /// <summary>
    /// Returns a list of child panels of given type.
    /// </summary>
    public IEnumerable<T> ChildrenOfType<T>() where T : Panel
    {
        if (_children == null || _children.Count == 0)
            yield break;

        for (int i = _children.Count - 1; i >= 0; i--)
        {
            if (_children[i] is T t)
                yield return t;
        }
    }
}
