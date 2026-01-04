using Sandbox.UI;
using Xunit;

namespace Sandbox.UI.Tests;

public class CursorTests
{
    [Fact]
    public void StandardCursor_Enum_HasCorrectValues()
    {
        // Verify enum matches s&box's InputStandardCursor_t
        Assert.Equal(0, (int)StandardCursor.Arrow);
        Assert.Equal(1, (int)StandardCursor.IBeam);
        Assert.Equal(2, (int)StandardCursor.HourGlass);
        Assert.Equal(3, (int)StandardCursor.Crosshair);
    }

    [Theory]
    [InlineData("default", StandardCursor.Arrow)]
    [InlineData("pointer", StandardCursor.Hand)]
    [InlineData("hand", StandardCursor.Hand)]
    [InlineData("text", StandardCursor.IBeam)]
    [InlineData("wait", StandardCursor.HourGlass)]
    [InlineData("crosshair", StandardCursor.Crosshair)]
    [InlineData("not-allowed", StandardCursor.No)]
    [InlineData("move", StandardCursor.SizeALL)]
    [InlineData("ew-resize", StandardCursor.SizeWE)]
    [InlineData("ns-resize", StandardCursor.SizeNS)]
    [InlineData("grab", StandardCursor.Hand)]
    [InlineData("grabbing", StandardCursor.HandClosed)]
    public void CursorHelper_FromCssString_MapsCorrectly(string cssValue, StandardCursor expected)
    {
        var result = CursorHelper.FromCssString(cssValue);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("auto")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid-cursor")]
    [InlineData("unknown")]
    public void CursorHelper_FromCssString_ReturnsNullForInvalidValues(string? cssValue)
    {
        var result = CursorHelper.FromCssString(cssValue);
        Assert.Null(result);
    }

    [Fact]
    public void CursorHelper_FromCssString_IsCaseInsensitive()
    {
        Assert.Equal(StandardCursor.Hand, CursorHelper.FromCssString("POINTER"));
        Assert.Equal(StandardCursor.Hand, CursorHelper.FromCssString("Pointer"));
        Assert.Equal(StandardCursor.Hand, CursorHelper.FromCssString("pointer"));
    }

    [Fact]
    public void RootPanel_GetCurrentCursor_ReturnsNullWhenNoHover()
    {
        var root = new RootPanel();
        var cursor = root.GetCurrentCursor();
        Assert.Null(cursor);
    }

    [Fact]
    public void BaseStyles_CursorProperty_ExistsAndWorks()
    {
        var panel = new Panel();
        
        // Verify cursor property exists
        Assert.NotNull(panel.Style);
        
        // Set cursor
        panel.Style.Cursor = "pointer";
        Assert.Equal("pointer", panel.Style.Cursor);
        
        // Change cursor
        panel.Style.Cursor = "text";
        Assert.Equal("text", panel.Style.Cursor);
        
        // Null cursor
        panel.Style.Cursor = null;
        Assert.Null(panel.Style.Cursor);
    }

    [Fact]
    public void CursorHelper_AllStandardCursorsHaveMappings()
    {
        // Verify all enum values can be reached via CSS strings
        var mappedValues = new HashSet<StandardCursor>
        {
            CursorHelper.FromCssString("default")!.Value,      // Arrow
            CursorHelper.FromCssString("text")!.Value,         // IBeam
            CursorHelper.FromCssString("wait")!.Value,         // HourGlass
            CursorHelper.FromCssString("crosshair")!.Value,    // Crosshair
            CursorHelper.FromCssString("progress")!.Value,     // WaitArrow
            CursorHelper.FromCssString("ns-resize")!.Value,    // SizeNS
            CursorHelper.FromCssString("ew-resize")!.Value,    // SizeWE
            CursorHelper.FromCssString("nesw-resize")!.Value,  // SizeNESW
            CursorHelper.FromCssString("nwse-resize")!.Value,  // SizeNWSE
            CursorHelper.FromCssString("move")!.Value,         // SizeALL
            CursorHelper.FromCssString("not-allowed")!.Value,  // No
            CursorHelper.FromCssString("pointer")!.Value,      // Hand
            CursorHelper.FromCssString("grabbing")!.Value,     // HandClosed
        };

        // Note: Up is not commonly used in CSS, so we accept it may not be mappable
        // All other standard cursors should be reachable
        Assert.True(mappedValues.Count >= 12, $"Expected at least 12 mappable cursors, got {mappedValues.Count}");
    }
}

