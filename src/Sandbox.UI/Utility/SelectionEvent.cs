namespace Sandbox.UI;

/// <summary>
/// Event for drag selection (text selection with mouse drag)
/// </summary>
public class SelectionEvent : PanelEvent
{
	/// <summary>
	/// The rectangular selection area in screen coordinates
	/// </summary>
	public Rect SelectionRect { get; set; }
	
	/// <summary>
	/// The starting point of the selection in screen coordinates
	/// </summary>
	public Vector2 StartPoint { get; set; }
	
	/// <summary>
	/// The ending point of the selection in screen coordinates
	/// </summary>
	public Vector2 EndPoint { get; set; }

	public SelectionEvent(string eventName, Panel target) : base(eventName, target)
	{
		Name = eventName;
		Target = target;
	}
}
