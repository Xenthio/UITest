namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Delete/Dispose
/// Based on s&box's Panel.Delete.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Delete this panel and all its children.
    /// </summary>
    public virtual void Delete(bool immediate = true)
    {
        if (IsDeleted) return;

        IsDeleting = true;

        // Delete all children first
        DeleteChildren(immediate);

        // Remove from parent
        if (Parent != null)
        {
            var p = Parent;
            Parent = null;
            p.SetNeedsPreLayout();
        }

        IsDeleted = true;
        IsDeleting = false;

        OnDeleted();
    }

    /// <summary>
    /// Called when this panel has been deleted.
    /// </summary>
    public virtual void OnDeleted()
    {
        Dispose();
    }
}
