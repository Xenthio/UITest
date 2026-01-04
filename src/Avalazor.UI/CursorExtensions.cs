using Sandbox.UI;

namespace Avalazor.UI;

/// <summary>
/// Extension methods for cursor handling in Avalazor
/// </summary>
public static class CursorExtensions
{
    /// <summary>
    /// Maps a Sandbox.UI StandardCursor enum value to Silk.NET's StandardCursor enum.
    /// </summary>
    public static Silk.NET.Input.StandardCursor ToSilkCursor(this StandardCursor cursor)
    {
        return cursor switch
        {
            StandardCursor.Arrow => Silk.NET.Input.StandardCursor.Arrow,
            StandardCursor.IBeam => Silk.NET.Input.StandardCursor.IBeam,
            StandardCursor.Crosshair => Silk.NET.Input.StandardCursor.Crosshair,
            StandardCursor.Hand => Silk.NET.Input.StandardCursor.Hand,
            StandardCursor.SizeWE => Silk.NET.Input.StandardCursor.HResize,
            StandardCursor.SizeNS => Silk.NET.Input.StandardCursor.VResize,
            
            // Map other cursors to closest Silk.NET equivalent
            StandardCursor.HourGlass => Silk.NET.Input.StandardCursor.Arrow, // No direct equivalent
            StandardCursor.WaitArrow => Silk.NET.Input.StandardCursor.Arrow,
            StandardCursor.Up => Silk.NET.Input.StandardCursor.Arrow,
            StandardCursor.SizeNWSE => Silk.NET.Input.StandardCursor.Hand, // No direct equivalent
            StandardCursor.SizeNESW => Silk.NET.Input.StandardCursor.Hand, // No direct equivalent
            StandardCursor.SizeALL => Silk.NET.Input.StandardCursor.Hand,
            StandardCursor.No => Silk.NET.Input.StandardCursor.Arrow, // No direct equivalent
            StandardCursor.HandClosed => Silk.NET.Input.StandardCursor.Hand,
            
            _ => Silk.NET.Input.StandardCursor.Arrow,
        };
    }
}
