namespace Sandbox.UI;

/// <summary>
/// Manages drag selection state and events for text selection with mouse drag.
/// Based on S&box's Selection class from engine/Sandbox.Engine/Systems/UI/Input/Selection.cs
/// </summary>
internal class Selection
{
    Panel? SelectionStart;
    Vector2 SelectionStartPos;
    Vector2 SelectionEndPos;

    public void UpdateSelection(Panel root, Panel? hovered, bool dragging, bool started, bool ended, Vector2 pos)
    {
        if (started)
        {
            SelectionStart = null;

            if (hovered == null)
                return;

            ClearSelection();

            SelectionStart = hovered;
            SelectionStartPos = ScreenPositionToPanelPosition(SelectionStart, pos);
            SelectionEndPos = SelectionStartPos;
            return;
        }

        if (SelectionStart == null)
            return;

        if (dragging || ended)
        {
            var hash = HashCode.Combine(SelectionStart, SelectionStartPos, SelectionEndPos);

            SelectionEndPos = ScreenPositionToPanelPosition(SelectionStart, pos);
            var newHash = HashCode.Combine(SelectionStart, SelectionStartPos, SelectionEndPos);
            if (newHash == hash && !ended) return; // Allow ended event even if position unchanged

            SelectionEvent e = new SelectionEvent("ondragselect", SelectionStart);
            e.StartPoint = PanelPositionToScreenPosition(SelectionStart, SelectionStartPos);
            e.EndPoint = PanelPositionToScreenPosition(SelectionStart, SelectionEndPos);
            
            // Create rect from two points
            float left = Math.Min(e.StartPoint.x, e.EndPoint.x);
            float top = Math.Min(e.StartPoint.y, e.EndPoint.y);
            float right = Math.Max(e.StartPoint.x, e.EndPoint.x);
            float bottom = Math.Max(e.StartPoint.y, e.EndPoint.y);
            e.SelectionRect = new Rect(left, top, right - left, bottom - top);

            SelectionStart.CreateEvent(e);
            
            // Clear selection state when drag ends
            if (ended)
            {
                SelectionStart = null;
            }
        }
    }

    void ClearSelection()
    {
        // Placeholder for future selection clearing logic
    }

    // Helper methods to convert between screen and panel coordinate spaces
    private Vector2 ScreenPositionToPanelPosition(Panel panel, Vector2 screenPos)
    {
        return screenPos - panel.Box.Rect.Position;
    }

    private Vector2 PanelPositionToScreenPosition(Panel panel, Vector2 panelPos)
    {
        return panelPos + panel.Box.Rect.Position;
    }
}
