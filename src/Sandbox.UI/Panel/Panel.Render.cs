namespace Sandbox.UI;

/// <summary>
/// Panel partial class: Rendering (abstract interface)
/// Based on s&box's Panel.Render.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Draw the panel's background.
    /// Called by renderer, override for custom background rendering.
    /// </summary>
    public virtual void DrawBackground(ref RenderState state)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Draw the panel's content (text, images, etc.)
    /// Called by renderer when HasContent is true.
    /// </summary>
    public virtual void DrawContent(ref RenderState state)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Render this panel's children
    /// </summary>
    internal void RenderChildren(IPanelRenderer renderer, ref RenderState state)
    {
        if (_renderChildrenDirty && _renderChildren != null)
        {
            _renderChildren.Sort((x, y) => x.GetRenderOrderIndex() - y.GetRenderOrderIndex());
            _renderChildrenDirty = false;
        }

        if (_renderChildren == null) return;

        for (int i = 0; i < _renderChildren.Count; i++)
        {
            renderer.Render(_renderChildren[i], state);
        }
    }
}

/// <summary>
/// Render state passed during panel rendering
/// </summary>
public struct RenderState
{
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public float RenderOpacity;
}

/// <summary>
/// Interface for panel renderers. Implement this to render panels with different backends.
/// </summary>
public interface IPanelRenderer
{
    /// <summary>
    /// Render a root panel
    /// </summary>
    void Render(RootPanel panel, float opacity = 1.0f);

    /// <summary>
    /// Render a panel with the given state
    /// </summary>
    void Render(Panel panel, RenderState state);

    /// <summary>
    /// Get the current screen/viewport rect
    /// </summary>
    Rect Screen { get; }
}
