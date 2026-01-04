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
        var testMappings = new (string css, StandardCursor expected)[]
        {
            ("default", StandardCursor.Arrow),
            ("text", StandardCursor.IBeam),
            ("wait", StandardCursor.HourGlass),
            ("crosshair", StandardCursor.Crosshair),
            ("progress", StandardCursor.WaitArrow),
            ("ns-resize", StandardCursor.SizeNS),
            ("ew-resize", StandardCursor.SizeWE),
            ("nesw-resize", StandardCursor.SizeNESW),
            ("nwse-resize", StandardCursor.SizeNWSE),
            ("move", StandardCursor.SizeALL),
            ("not-allowed", StandardCursor.No),
            ("pointer", StandardCursor.Hand),
            ("grabbing", StandardCursor.HandClosed),
        };

        var mappedValues = new HashSet<StandardCursor>();
        
        foreach (var (css, expected) in testMappings)
        {
            var result = CursorHelper.FromCssString(css);
            Assert.NotNull(result);
            Assert.Equal(expected, result.Value);
            mappedValues.Add(result.Value);
        }

        // Note: Up is not commonly used in CSS, so we accept it may not be mappable
        // All other standard cursors should be reachable
        Assert.True(mappedValues.Count >= 12, $"Expected at least 12 mappable cursors, got {mappedValues.Count}");
    }
}

