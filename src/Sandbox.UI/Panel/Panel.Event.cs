namespace Sandbox.UI;

/// <summary>
/// Panel event handling methods.
/// Based on s&box's Panel.Event.cs from engine/Sandbox.Engine/Systems/UI/Panel/Panel.Event.cs
/// </summary>
public partial class Panel
{
    /// <summary>
    /// Pending events that will be processed
    /// </summary>
    internal List<PanelEvent>? PendingEvents { get; set; }

    /// <summary>
    /// Event listeners registered on this panel
    /// </summary>
    internal List<EventCallback>? EventListeners { get; set; }

    internal struct EventCallback
    {
        public string EventName;
        public Action<PanelEvent>? Action;
        public Panel? Panel;
        public Panel? Context;
    }

    /// <summary>
    /// Create a new event and pass it to the panels event queue.
    /// </summary>
    /// <param name="name">Event name.</param>
    /// <param name="value">Event value.</param>
    public virtual void CreateEvent(string name, object? value = null)
    {
        var e = new PanelEvent(name, this);
        e.Value = value;
        CreateEvent(e);
    }

    /// <summary>
    /// Pass given event to the event queue.
    /// </summary>
    public virtual void CreateEvent(PanelEvent evnt)
    {
        PendingEvents ??= new List<PanelEvent>();
        PendingEvents.Add(evnt);
    }

    /// <summary>
    /// Call this when the value has changed due to user input etc.
    /// This triggers $"{name}.changed" event, with value being the Value on the event.
    /// </summary>
    protected void CreateValueEvent(string name, object? value)
    {
        CreateEvent($"{name}.changed", value);
    }

    /// <summary>
    /// Process pending events.
    /// Events are processed by calling OnEvent() which handles event listeners and propagation.
    /// Event propagation is done directly via Parent?.OnEvent(e) inside OnEvent, matching S&box behavior.
    /// This ensures that when a child panel (like a label) receives an event, it properly propagates
    /// up to parent panels (like buttons), allowing parent event listeners to fire.
    /// </summary>
    internal void ProcessPendingEvents()
    {
        if (PendingEvents == null || PendingEvents.Count == 0)
            return;

        var events = PendingEvents.ToList();
        PendingEvents.Clear();

        foreach (var e in events)
        {
            OnEvent(e);
        }
    }

    /// <summary>
    /// Called when various PanelEvents happen.
    /// </summary>
    protected virtual void OnEvent(PanelEvent e)
    {
        e.This = this;
        
        // Debug: Log event handling
        if (e.Is("onclick"))
        {
            System.Console.WriteLine($"[OnEvent] {GetType().Name} handling onclick, has listeners: {EventListeners?.Count ?? 0}");
        }

        if (e is MousePanelEvent mpe)
        {
            if (e.Is("onclick")) OnClick(mpe);
            if (e.Is("onmiddleclick")) OnMiddleClick(mpe);
            if (e.Is("onrightclick")) OnRightClick(mpe);
            if (e.Is("onmousedown")) OnMouseDown(mpe);
            if (e.Is("onmouseup")) OnMouseUp(mpe);
            if (e.Is("ondoubleclick")) OnDoubleClick(mpe);
            if (e.Is("onmousemove")) OnMouseMove(mpe);
            if (e.Is("onmouseover")) OnMouseOver(mpe);
            if (e.Is("onmouseout")) OnMouseOut(mpe);
        }

        if (!e.Propagate)
            return;

        // Call event listeners
        if (EventListeners != null)
        {
            foreach (var listener in EventListeners.ToList())
            {
                if (string.Equals(listener.EventName, e.Name, StringComparison.OrdinalIgnoreCase))
                {
                    listener.Action?.Invoke(e);
                }
            }
        }

        if (!e.Propagate)
            return;

        // Propagate event up the parent chain (matches S&box line 276 in Panel.Event.cs)
        if (Parent != null && e.Is("onclick"))
        {
            System.Console.WriteLine($"[OnEvent] {GetType().Name} propagating onclick to {Parent.GetType().Name}");
        }
        Parent?.OnEvent(e);
    }

    /// <summary>
    /// Called when the player releases their left mouse button (Mouse 1) while hovering this panel.
    /// </summary>
    protected virtual void OnClick(MousePanelEvent e) { }

    /// <summary>
    /// Called when the player releases their middle mouse button (Mouse 3) while hovering this panel.
    /// </summary>
    protected virtual void OnMiddleClick(MousePanelEvent e) { }

    /// <summary>
    /// Called when the player releases their right mouse button (Mouse 2) while hovering this panel.
    /// </summary>
    protected virtual void OnRightClick(MousePanelEvent e) { }

    /// <summary>
    /// Called when the player presses down the left or right mouse buttons while hovering this panel.
    /// </summary>
    protected virtual void OnMouseDown(MousePanelEvent e) { }

    /// <summary>
    /// Called when the player releases left or right mouse button.
    /// </summary>
    protected virtual void OnMouseUp(MousePanelEvent e) { }

    /// <summary>
    /// Called when the player double clicks the panel with the left mouse button.
    /// </summary>
    protected virtual void OnDoubleClick(MousePanelEvent e) { }

    /// <summary>
    /// Called when the cursor moves while hovering this panel.
    /// </summary>
    protected virtual void OnMouseMove(MousePanelEvent e) { }

    /// <summary>
    /// Called when the cursor enters this panel.
    /// </summary>
    protected virtual void OnMouseOver(MousePanelEvent e) { }

    /// <summary>
    /// Called when the cursor leaves this panel.
    /// </summary>
    protected virtual void OnMouseOut(MousePanelEvent e) { }
}
