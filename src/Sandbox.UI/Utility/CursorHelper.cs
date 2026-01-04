namespace Sandbox.UI;

/// <summary>
/// Utility methods for working with cursors.
/// Based on s&box's cursor handling.
/// </summary>
public static class CursorHelper
{
    /// <summary>
    /// Maps a CSS cursor string to a StandardCursor enum value.
    /// Returns null if the cursor string is "auto" or unrecognized.
    /// </summary>
    /// <param name="cursorString">CSS cursor value (e.g., "pointer", "text", "default")</param>
    /// <returns>StandardCursor enum value or null</returns>
    public static StandardCursor? FromCssString(string? cursorString)
    {
        if (string.IsNullOrWhiteSpace(cursorString) || cursorString == "auto")
            return null;

        // Map common CSS cursor values to StandardCursor enum
        // Reference: https://developer.mozilla.org/en-US/docs/Web/CSS/cursor
        return cursorString.ToLowerInvariant() switch
        {
            "default" => StandardCursor.Arrow,
            "pointer" => StandardCursor.Hand,
            "hand" => StandardCursor.Hand,
            "text" => StandardCursor.IBeam,
            "wait" => StandardCursor.HourGlass,
            "progress" => StandardCursor.WaitArrow,
            "crosshair" => StandardCursor.Crosshair,
            "not-allowed" => StandardCursor.No,
            "no-drop" => StandardCursor.No,
            
            // Resize cursors
            "n-resize" => StandardCursor.SizeNS,
            "s-resize" => StandardCursor.SizeNS,
            "ns-resize" => StandardCursor.SizeNS,
            "e-resize" => StandardCursor.SizeWE,
            "w-resize" => StandardCursor.SizeWE,
            "ew-resize" => StandardCursor.SizeWE,
            "ne-resize" => StandardCursor.SizeNESW,
            "sw-resize" => StandardCursor.SizeNESW,
            "nesw-resize" => StandardCursor.SizeNESW,
            "nw-resize" => StandardCursor.SizeNWSE,
            "se-resize" => StandardCursor.SizeNWSE,
            "nwse-resize" => StandardCursor.SizeNWSE,
            "move" => StandardCursor.SizeALL,
            "all-scroll" => StandardCursor.SizeALL,
            
            // Grab cursors
            "grab" => StandardCursor.Hand,
            "grabbing" => StandardCursor.HandClosed,
            
            _ => null, // Unrecognized or unsupported cursor
        };
    }
}
