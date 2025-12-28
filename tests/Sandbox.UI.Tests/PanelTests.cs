using Sandbox.UI;
using Xunit;

namespace Sandbox.UI.Tests;

/// <summary>
/// Tests for the core Panel types (without Yoga dependency)
/// </summary>
public class PanelTests
{
    [Fact]
    public void Box_InitializesWithZeroRects()
    {
        var box = new Box();

        Assert.Equal(0, box.Rect.Width);
        Assert.Equal(0, box.Rect.Height);
    }

    [Fact]
    public void Length_PixelsCreatesCorrectly()
    {
        var length = Length.Pixels(100);

        Assert.Equal(100, length.Value);
        Assert.Equal(LengthUnit.Pixels, length.Unit);
    }

    [Fact]
    public void Length_PercentCreatesCorrectly()
    {
        var length = Length.Percent(50);

        Assert.Equal(50, length.Value);
        Assert.Equal(LengthUnit.Percentage, length.Unit);
    }

    [Fact]
    public void Length_GetPixels_CalculatesPercentage()
    {
        var length = Length.Percent(50);

        var pixels = length.GetPixels(200);

        Assert.Equal(100, pixels);
    }

    [Fact]
    public void Color_FromRgba_CreatesCorrectColor()
    {
        var color = Color.FromRgba(255, 128, 64, 255);

        Assert.Equal(1.0f, color.r, 0.01f);
        Assert.Equal(0.5f, color.g, 0.02f);
        Assert.Equal(0.25f, color.b, 0.02f);
        Assert.Equal(1.0f, color.a, 0.01f);
    }

    [Fact]
    public void Rect_ContainsPoint()
    {
        var rect = new Rect(10, 10, 100, 100);

        Assert.True(rect.Contains(50, 50));
        Assert.False(rect.Contains(0, 0));
        Assert.False(rect.Contains(200, 200));
    }

    [Fact]
    public void Vector2_BasicOperations()
    {
        var v1 = new Vector2(10, 20);
        var v2 = new Vector2(5, 5);

        var sum = v1 + v2;
        Assert.Equal(15, sum.x);
        Assert.Equal(25, sum.y);

        var diff = v1 - v2;
        Assert.Equal(5, diff.x);
        Assert.Equal(15, diff.y);
    }

    [Fact]
    public void Margin_InitializesCorrectly()
    {
        var margin = new Margin(10, 20, 30, 40);

        Assert.Equal(10, margin.Left);
        Assert.Equal(20, margin.Top);
        Assert.Equal(30, margin.Right);
        Assert.Equal(40, margin.Bottom);
    }

    [Fact]
    public void Styles_CanSetProperties()
    {
        var styles = new Styles();

        styles.Width = Length.Pixels(200);
        styles.Height = Length.Pixels(100);
        styles.BackgroundColor = Color.Red;

        Assert.Equal(200, styles.Width?.Value);
        Assert.Equal(100, styles.Height?.Value);
        Assert.Equal(Color.Red, styles.BackgroundColor);
    }

    [Fact]
    public void Styles_Add_MergesCorrectly()
    {
        var styles1 = new Styles { Width = Length.Pixels(100) };
        var styles2 = new Styles { Height = Length.Pixels(50), BackgroundColor = Color.Blue };

        styles1.Add(styles2);

        Assert.Equal(100, styles1.Width?.Value);
        Assert.Equal(50, styles1.Height?.Value);
        Assert.Equal(Color.Blue, styles1.BackgroundColor);
    }
}
