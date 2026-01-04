namespace Sandbox.UI;

/// <summary>
/// Base Panel event.
/// Based on s&box's PanelEvent from engine/Sandbox.Engine/Systems/UI/Panel/Event/PanelEvent.cs
/// </summary>
public class PanelEvent
{
    public string Name { get; init; }
    public object? Value { get; set; }
    public float Time { get; set; }
    public string? Button { get; set; }

    /// <summary>
    /// The panel on which the event is being called.
    /// </summary>
    public Panel? This { get; set; }
    
    /// <summary>
    /// The original panel that triggered the event.
    /// </summary>
    public Panel? Target { get; set; }

    internal bool Propagate = true;

    public PanelEvent(string eventName, Panel? active = null)
    {
        Name = eventName;
        Target = active;
    }

    public bool Is(string name)
    {
        return string.Equals(name, Name, StringComparison.OrdinalIgnoreCase);
    }

    public void StopPropagation()
    {
        Propagate = false;
    }
}

/// <summary>
/// Mouse related PanelEvent.
/// Based on s&box's MousePanelEvent from engine/Sandbox.Engine/Systems/UI/Panel/Event/MousePanelEvent.cs
/// </summary>
public class MousePanelEvent : PanelEvent
{
    /// <summary>
    /// Position of the cursor relative to the panel's top left corner at the time the event was triggered.
    /// </summary>
    public Vector2 LocalPosition;

    /// <summary>
    /// Position of the cursor in screen coordinates (not panel-relative).
    /// </summary>
    public Vector2 ScreenPosition
    {
        get
        {
            if (Target?.FindRootPanel() is RootPanel root)
            {
                return root.MousePos;
            }
            return LocalPosition;
        }
    }

    /// <summary>
    /// Which button triggered the event, in string form.
    /// </summary>
    public new string? Button;

    /// <summary>
    /// Which button triggered the event, as a MouseButtons enum.
    /// </summary>
    public MouseButtons MouseButton { get; set; }

    public MousePanelEvent(string event_name, Panel? active, string button) : base(event_name, active)
    {
        Name = event_name;
        Target = active;
        LocalPosition = Target?.MousePosition ?? Vector2.Zero;
        Button = button;

        if (button == "mouseleft") MouseButton = MouseButtons.Left;
        if (button == "mouseright") MouseButton = MouseButtons.Right;
        if (button == "mousemiddle") MouseButton = MouseButtons.Middle;
    }
}

/// <summary>
/// Mouse button enum
/// </summary>
public enum MouseButtons
{
    Left,
    Right,
    Middle
}
