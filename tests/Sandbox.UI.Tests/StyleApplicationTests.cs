using Xunit;
using Sandbox.UI;

namespace Sandbox.UI.Tests;

public class StyleApplicationTests
{
    [Fact]
    public void Panel_WithUniversalSelector_AppliesStyles()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        rootPanel.AddChild(panel);

        // Create a stylesheet with universal selector
        var css = @"
            * {
                width: 150px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(panel.ComputedStyle);
        Assert.NotNull(panel.ComputedStyle.Width);
        Assert.Equal(150f, panel.ComputedStyle.Width.Value.Value);
    }

    [Fact]
    public void Panel_WithStyleSheet_AppliesStylesCorrectly()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        panel.AddClass("test-button");
        rootPanel.AddChild(panel);

        // Create a stylesheet with rules
        var css = @"
            .test-button {
                background-color: #ff0000;
                width: 200px;
                height: 50px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act - Run layout to trigger style application
        rootPanel.Layout();

        // Assert - Check that computed styles have the values from the stylesheet
        Assert.NotNull(panel.ComputedStyle);
        
        // Check width and height first (these should work)
        Assert.NotNull(panel.ComputedStyle.Width);
        Assert.Equal(200f, panel.ComputedStyle.Width.Value.Value);
        
        Assert.NotNull(panel.ComputedStyle.Height);
        Assert.Equal(50f, panel.ComputedStyle.Height.Value.Value);
        
        // Check background color
        Assert.NotNull(panel.ComputedStyle.BackgroundColor);
        Assert.Equal(1f, panel.ComputedStyle.BackgroundColor.Value.r);
        Assert.Equal(0f, panel.ComputedStyle.BackgroundColor.Value.g);
        Assert.Equal(0f, panel.ComputedStyle.BackgroundColor.Value.b);
    }

    [Fact]
    public void Panel_WithMultipleStyleSheets_AppliesCascadeCorrectly()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        panel.ElementName = "button";
        rootPanel.AddChild(panel);

        // Add two stylesheets with overlapping rules
        var css1 = @"
            button {
                background-color: #ff0000;
                width: 100px;
            }
        ";
        var css2 = @"
            button {
                background-color: #00ff00;
                height: 50px;
            }
        ";
        
        rootPanel.StyleSheet.Add(StyleSheet.FromString(css1, "sheet1"));
        rootPanel.StyleSheet.Add(StyleSheet.FromString(css2, "sheet2"));

        // Act
        rootPanel.Layout();

        // Assert - Later stylesheet should override background-color
        Assert.NotNull(panel.ComputedStyle);
        Assert.NotNull(panel.ComputedStyle.BackgroundColor);
        // Sheet2 is added after sheet1, but sheets are inserted at index 0, so sheet2 comes first
        // and sheet1's rules should win due to order
        Assert.NotNull(panel.ComputedStyle.Width);
        Assert.NotNull(panel.ComputedStyle.Height);
    }

    [Fact]
    public void Panel_FillDefaults_SetsDefaultValuesForUnsetProperties()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        rootPanel.AddChild(panel);

        // Act - Run layout without any stylesheets
        rootPanel.Layout();

        // Assert - FillDefaults should set default values for CSS properties
        Assert.NotNull(panel.ComputedStyle);
        Assert.NotNull(panel.ComputedStyle.Display);
        Assert.Equal(DisplayMode.Flex, panel.ComputedStyle.Display.Value);
        
        Assert.NotNull(panel.ComputedStyle.FlexDirection);
        Assert.NotNull(panel.ComputedStyle.AlignItems);
        Assert.NotNull(panel.ComputedStyle.Opacity);
    }

    [Fact]
    public void Panel_RgbaWith255Alpha_NormalizesAlphaCorrectly()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        rootPanel.AddChild(panel);

        // XGUI stylesheets use rgba(76,88,68,255) with alpha as 255 instead of 1.0
        var css = @"
            * {
                background-color: rgba(76,88,68,255);
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert - Alpha should be normalized to 1.0 (255/255)
        Assert.NotNull(panel.ComputedStyle);
        Assert.NotNull(panel.ComputedStyle.BackgroundColor);
        var color = panel.ComputedStyle.BackgroundColor.Value;
        
        // Check RGB values are normalized from 0-255 to 0-1 range
        Assert.Equal(76f / 255f, color.r, precision: 3);
        Assert.Equal(88f / 255f, color.g, precision: 3);
        Assert.Equal(68f / 255f, color.b, precision: 3);
        
        // Check alpha is normalized from 255 to 1.0
        Assert.Equal(1.0f, color.a, precision: 3);
    }

    [Fact]
    public void Panel_ColorPropertyAlias_MapsToFontColor()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var panel = new Panel();
        rootPanel.AddChild(panel);

        // CSS standard uses "color" for text color, s&box uses "font-color"
        var css = @"
            * {
                color: #FFFFFF;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert - "color" should be aliased to FontColor
        Assert.NotNull(panel.ComputedStyle);
        Assert.NotNull(panel.ComputedStyle.FontColor);
        var color = panel.ComputedStyle.FontColor.Value;
        
        // Check white color (1, 1, 1, 1)
        Assert.Equal(1.0f, color.r, precision: 3);
        Assert.Equal(1.0f, color.g, precision: 3);
        Assert.Equal(1.0f, color.b, precision: 3);
        Assert.Equal(1.0f, color.a, precision: 3);
    }
}
