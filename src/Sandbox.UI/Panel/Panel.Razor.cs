using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Razor/Blazor integration methods
/// Based on s&box's Panel.Razor.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// If true, the panel will render its tree if the hash changes.
    /// </summary>
    public bool IsRazorTreeable { get; private set; }

    /// <summary>
    /// Get a checksum based on files used to generate the render tree.
    /// Override this if the component uses resources that may change.
    /// </summary>
    protected virtual string GetRenderTreeChecksum() => GetType().FullName ?? "unknown";

    /// <summary>
    /// Build the Razor render tree for this panel.
    /// This is called by the Razor compiler for .razor components that inherit Panel.
    /// </summary>
    protected virtual void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Base implementation does nothing - subclasses override this
    }

    /// <summary>
    /// Build a hash of data that affects the render tree.
    /// If this hash changes, the tree is rebuilt.
    /// Override this to make the component reactive to state changes.
    /// </summary>
    protected virtual int BuildHash() => 0;

    /// <summary>
    /// Called after the render tree has been built.
    /// </summary>
    protected virtual void OnAfterTreeRender(bool firstTime)
    {
    }

    /// <summary>
    /// Called when a parameter is set on the panel.
    /// </summary>
    public virtual void OnParameterSet()
    {
    }

    /// <summary>
    /// Force a state change and rebuild the tree.
    /// </summary>
    public void StateHasChanged()
    {
        _needsTreeRebuild = true;
    }

    private bool _needsTreeRebuild = true;
    private int _lastHash;

    /// <summary>
    /// Rebuild the render tree if needed.
    /// </summary>
    internal void RebuildRenderTreeIfNeeded()
    {
        if (!IsRazorTreeable) return;

        var hash = BuildHash();
        if (!_needsTreeRebuild && hash == _lastHash) return;

        _lastHash = hash;
        _needsTreeRebuild = false;

        // Clear existing children
        DeleteChildren();

        // Build new tree
        // Note: In the real implementation, this would use Razor's rendering pipeline
        // For now, we rely on external rendering (RazorRenderer)
    }

    /// <summary>
    /// Mark this panel as a Razor panel that has a render tree.
    /// Called by the Razor compiler.
    /// </summary>
    protected void InitializeRazorPanel()
    {
        IsRazorTreeable = true;
    }
}
