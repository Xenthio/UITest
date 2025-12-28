namespace Avalazor.UI;

/// <summary>
/// Panel partial class: Child management
/// Based on s&box's Panel.Children.cs
/// </summary>
public partial class Panel
{
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
        
        // Add to Yoga tree (s&box pattern)
        if (YogaNode != null && child.YogaNode != null)
        {
            YogaNode.AddChild(child.YogaNode);
        }
        
        needsPreLayout = true;
    }

    /// <summary>
    /// Remove a child panel
    /// </summary>
    public virtual void RemoveChild(Panel child)
    {
        if (_children.Remove(child))
        {
            // Remove from Yoga tree
            if (YogaNode != null && child.YogaNode != null)
            {
                YogaNode.RemoveChild(child.YogaNode);
            }
            
            child.Parent = null;
            needsPreLayout = true;
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
}
