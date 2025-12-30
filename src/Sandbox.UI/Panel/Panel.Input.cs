using System.Numerics;

namespace Sandbox.UI;

/// <summary>
/// Panel input handling methods.
/// Based on S&box's Panel.Input.cs from engine/Sandbox.Engine/Systems/UI/Panel/Panel.Input.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Current mouse position local to this panels top left corner.
    /// </summary>
    public Vector2 MousePosition
    {
        get
        {
            if (FindRootPanel() is not RootPanel root)
                return default;

            var mp = root.MousePos;
            return mp - Box.Rect.Position;
        }
    }

    /// <summary>
    /// Called by <see cref="PanelInput.CheckHover(Panel, Vector2, ref Panel)" /> to transform
    /// the current mouse position using the panel's LocalMatrix (by default). This can be overriden for special cases.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public virtual Vector2 GetTransformPosition(Vector2 pos)
    {
        // TODO: Implement LocalMatrix transform when we add transform support
        // return LocalMatrix?.Transform(pos) ?? pos;
        return pos;
    }

    /// <summary>
    /// Whether given screen position is within this panel.
    /// </summary>
    /// <param name="pos">The position to test, in screen coordinates.</param>
    public bool IsInside(Vector2 pos)
    {
        var rect = Box.Rect;

        if (pos.x < rect.Left || pos.x > rect.Right) return false;
        if (pos.y < rect.Top || pos.y > rect.Bottom) return false;

        return true;
    }

    /// <summary>
    /// False by default, can this element accept keyboard focus. If an element accepts
    /// focus it'll be able to receive keyboard input.
    /// </summary>
    public bool AcceptsFocus { get; set; }

    /// <summary>
    /// Give input focus to this panel.
    /// </summary>
    public bool Focus()
    {
        return InputFocus.Set(this);
    }

    /// <summary>
    /// Remove input focus from this panel.
    /// </summary>
    public bool Blur()
    {
        return InputFocus.Clear(this);
    }

    /// <summary>
    /// Called when any button, mouse (except for mouse4/5) and keyboard, are pressed or released while hovering this panel.
    /// </summary>
    public virtual void OnButtonEvent(ButtonEvent e)
    {
        if (e.StopPropagation) return;
        Parent?.OnButtonEvent(e);
    }

    /// <summary>
    /// Called when a printable character has been typed (pressed) while this panel has input focus.
    /// </summary>
    public virtual void OnKeyTyped(char k)
    {
        Parent?.OnKeyTyped(k);
    }

    /// <summary>
    /// Called when any keyboard button has been typed (pressed) while this panel has input focus.
    /// </summary>
    public virtual void OnButtonTyped(ButtonEvent e)
    {
        if (e.StopPropagation) return;
        Parent?.OnButtonTyped(e);
    }

    /// <summary>
    /// Called when the player scrolls their mouse wheel while hovering this panel.
    /// </summary>
    /// <param name="value">The scroll wheel delta. Positive values are scrolling down, negative - up.</param>
    public virtual void OnMouseWheel(Vector2 value)
    {
        if (TryScroll(value))
            return;

        Parent?.OnMouseWheel(value);
    }

    /// <summary>
    /// Called from <see cref="OnMouseWheel"/> to try to scroll.
    /// </summary>
    /// <param name="value">The scroll wheel delta.</param>
    /// <returns>Return true to NOT propagate the event to the parent.</returns>
    public bool TryScroll(Vector2 value)
    {
        if (ComputedStyle == null) return false;
        // Note: HasScrollY/HasScrollX don't exist yet, skip scrolling for now
        // TODO: Implement proper scrolling support

        return false;
    }

    internal static Panel? MouseCapture { get; private set; }

    /// <summary>
    /// Captures the mouse cursor while active.
    /// </summary>
    /// <param name="b">Whether to enable or disable the capture.</param>
    public void SetMouseCapture(bool b)
    {
        if (b)
        {
            MouseCapture = this;
            return;
        }

        if (MouseCapture == this)
        {
            MouseCapture = null;
            return;
        }
    }

    /// <summary>
    /// Whether this panel is capturing the mouse cursor.
    /// </summary>
    public bool HasMouseCapture => MouseCapture == this;

    /// <summary>
    /// Get the panel at a specific screen position (used for inspector picking)
    /// </summary>
    public Panel? GetPanelAt(Vector2 position, bool checkVisible = true, bool checkPointerEvents = true)
    {
        if (checkVisible && !IsVisible)
            return null;

        if (checkPointerEvents && ComputedStyle?.PointerEvents == PointerEvents.None)
            return null;

        if (!IsInside(position))
            return null;

        // Check children first (reverse order for z-index)
        if (_children != null)
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                if (child != null)
                {
                    var result = child.GetPanelAt(position, checkVisible, checkPointerEvents);
                    if (result != null)
                        return result;
                }
            }
        }

        return this;
    }
}
