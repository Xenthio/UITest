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
    public void RenderChildren(IPanelRenderer renderer, ref RenderState state)
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

    /// <summary>
    /// Measure text dimensions for layout calculations.
    /// </summary>
    /// <param name="text">The text to measure</param>
    /// <param name="fontFamily">Font family name</param>
    /// <param name="fontSize">Font size in pixels</param>
    /// <param name="fontWeight">Font weight (100-900)</param>
    /// <returns>Vector2 containing width and height of the text</returns>
    Vector2 MeasureText(string text, string? fontFamily, float fontSize, int fontWeight);

    /// <summary>
    /// Register this renderer as the active renderer for text measurement.
    /// Call this once when the renderer is created to enable accurate Label layout.
    /// </summary>
    void RegisterAsActiveRenderer();
}
