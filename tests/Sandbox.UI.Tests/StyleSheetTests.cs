using Sandbox.UI;
using System.Reflection;
using Xunit;

namespace Sandbox.UI.Tests;

/// <summary>
/// Tests for the StyleSheet CSS parsing and selector matching system
/// </summary>
public class StyleSheetTests
{
    [Fact]
    public void StyleSheet_FromString_ParsesSimpleRule()
    {
        var css = ".button { width: 100px; height: 50px; }";
        var sheet = StyleSheet.FromString(css);

        Assert.NotNull(sheet);
        Assert.Single(sheet.Nodes);
        Assert.Equal(100, sheet.Nodes[0].Styles.Width?.Value);
        Assert.Equal(50, sheet.Nodes[0].Styles.Height?.Value);
    }

    [Fact]
    public void StyleSheet_FromString_ParsesMultipleRules()
    {
        var css = @"
            .button { width: 100px; }
            .label { height: 20px; }
        ";
        var sheet = StyleSheet.FromString(css);

        Assert.NotNull(sheet);
        Assert.Equal(2, sheet.Nodes.Count);
    }

    [Fact]
    public void StyleSheet_FromString_ParsesVariables()
    {
        var css = @"
            $primary-color: #ff0000;
            .button { background-color: $primary-color; }
        ";
        var sheet = StyleSheet.FromString(css);

        Assert.NotNull(sheet);
        Assert.Single(sheet.Nodes);
        // The variable should be replaced
        Assert.Equal(1f, sheet.Nodes[0].Styles.BackgroundColor?.r);
        Assert.Equal(0f, sheet.Nodes[0].Styles.BackgroundColor?.g);
        Assert.Equal(0f, sheet.Nodes[0].Styles.BackgroundColor?.b);
    }

    [Fact]
    public void StyleSheet_FromString_ParsesNestedRules()
    {
        var css = @"
            .button {
                width: 100px;
                &:hover {
                    width: 120px;
                }
            }
        ";
        var sheet = StyleSheet.FromString(css);

        Assert.NotNull(sheet);
        Assert.Equal(2, sheet.Nodes.Count);
    }

    [Fact]
    public void StyleSelector_MatchesClass()
    {
        var css = ".button { width: 100px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel();
        panel.AddClass("button");

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.NotNull(matchedSelector);
    }

    [Fact]
    public void StyleSelector_DoesNotMatchWrongClass()
    {
        var css = ".button { width: 100px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel();
        panel.AddClass("label");

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.Null(matchedSelector);
    }

    [Fact]
    public void StyleSelector_MatchesId()
    {
        var css = "#main { width: 100px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel { Id = "main" };

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.NotNull(matchedSelector);
    }

    [Fact]
    public void StyleSelector_MatchesElementName()
    {
        var css = "panel { width: 100px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel();
        // ElementName defaults to type name lowercased

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.NotNull(matchedSelector);
    }

    [Fact]
    public void StyleSelector_MatchesPseudoClass()
    {
        var css = ".button:hover { width: 120px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel();
        panel.AddClass("button");
        panel.Switch(PseudoClass.Hover, true);

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.NotNull(matchedSelector);
    }

    [Fact]
    public void StyleSelector_DoesNotMatchWithoutPseudoClass()
    {
        var css = ".button:hover { width: 120px; }";
        var sheet = StyleSheet.FromString(css);

        var panel = new Panel();
        panel.AddClass("button");
        // Not hovered

        var matchedSelector = sheet.Nodes[0].Test(panel);
        Assert.Null(matchedSelector);
    }

    [Fact]
    public void StyleSelector_Specificity_IdBeatsClass()
    {
        var css = @"
            .button { width: 100px; }
            #main { width: 200px; }
        ";
        var sheet = StyleSheet.FromString(css);

        // ID selector should have higher score
        var classSelector = sheet.Nodes[0].Selectors[0];
        var idSelector = sheet.Nodes[1].Selectors[0];

        Assert.True(idSelector.Score > classSelector.Score);
    }

    [Fact]
    public void Styles_Set_ParsesPixelValue()
    {
        var styles = new Styles();
        var result = styles.Set("width", "100px");

        Assert.True(result);
        Assert.Equal(100, styles.Width?.Value);
        Assert.Equal(LengthUnit.Pixels, styles.Width?.Unit);
    }

    [Fact]
    public void Styles_Set_ParsesPercentValue()
    {
        var styles = new Styles();
        var result = styles.Set("width", "50%");

        Assert.True(result);
        Assert.Equal(50, styles.Width?.Value);
        Assert.Equal(LengthUnit.Percentage, styles.Width?.Unit);
    }

    [Fact]
    public void Styles_Set_ParsesColorHex()
    {
        var styles = new Styles();
        var result = styles.Set("color", "#ff0000");

        Assert.True(result);
        Assert.Equal(1f, styles.FontColor?.r);
        Assert.Equal(0f, styles.FontColor?.g);
        Assert.Equal(0f, styles.FontColor?.b);
    }

    [Fact]
    public void Styles_Set_ParsesColorName()
    {
        var styles = new Styles();
        var result = styles.Set("color", "red");

        Assert.True(result);
        Assert.Equal(Color.Red, styles.FontColor);
    }

    [Fact]
    public void Styles_Set_ParsesFlexDirection()
    {
        var styles = new Styles();
        var result = styles.Set("flex-direction", "column");

        Assert.True(result);
        Assert.Equal(FlexDirection.Column, styles.FlexDirection);
    }

    [Fact]
    public void Styles_Set_ParsesPadding()
    {
        var styles = new Styles();
        var result = styles.Set("padding", "10px");

        Assert.True(result);
        Assert.Equal(10, styles.PaddingTop?.Value);
        Assert.Equal(10, styles.PaddingRight?.Value);
        Assert.Equal(10, styles.PaddingBottom?.Value);
        Assert.Equal(10, styles.PaddingLeft?.Value);
    }

    [Fact]
    public void Color_Parse_HandlesShortHex()
    {
        var result = Color.Parse("#f00");

        Assert.NotNull(result);
        Assert.Equal(1f, result.Value.r);
        Assert.Equal(0f, result.Value.g);
        Assert.Equal(0f, result.Value.b);
    }

    [Fact]
    public void StyleSheetCollection_Parse_AppliesStyles()
    {
        var panel = new Panel();
        panel.StyleSheet.Parse(".panel { width: 100px; }");

        // The stylesheet should be added to the collection
        Assert.True(panel.AllStyleSheets.Any());
    }

    [Fact]
    public void Panel_WithStyleSheetAttribute_LoadsStylesheet()
    {
        // Test that StyleSheetAttribute can be read from a panel
        var type = typeof(TestPanelWithStyleSheet);
        var attrs = type.GetCustomAttributes(typeof(StyleSheetAttribute), false);
        
        Assert.NotEmpty(attrs);
        var styleAttr = (StyleSheetAttribute)attrs[0];
        Assert.Equal("test.scss", styleAttr.Name);
    }

    [Fact]
    public void Panel_WithSourceLocationAttribute_ReturnsPath()
    {
        var type = typeof(TestPanelWithSourceLocation);
        var attr = type.GetCustomAttribute(typeof(SourceLocationAttribute), false) as SourceLocationAttribute;
        
        Assert.NotNull(attr);
        Assert.Equal("/test/path/TestPanel.razor", attr.FilePath);
    }

    [Fact]
    public void Styles_Set_ParsesLinearGradient()
    {
        var styles = new Styles();
        var result = styles.Set("background", "linear-gradient(135deg, #007acc 0%, #005a9e 100%)");

        Assert.True(result);
        Assert.NotNull(styles.BackgroundGradient);
        Assert.True(styles.BackgroundGradient.Value.IsValid);
        Assert.Equal(GradientInfo.GradientTypes.Linear, styles.BackgroundGradient.Value.GradientType);
        Assert.Equal(2, styles.BackgroundGradient.Value.ColorOffsets.Length);
    }

    [Fact]
    public void Styles_Set_ParsesLinearGradientWithoutAngle()
    {
        var styles = new Styles();
        var result = styles.Set("background", "linear-gradient(#ff0000, #0000ff)");

        Assert.True(result);
        Assert.NotNull(styles.BackgroundGradient);
        Assert.True(styles.BackgroundGradient.Value.IsValid);
        Assert.Equal(2, styles.BackgroundGradient.Value.ColorOffsets.Length);
        // First color should be red
        Assert.Equal(1f, styles.BackgroundGradient.Value.ColorOffsets[0].color.r);
        Assert.Equal(0f, styles.BackgroundGradient.Value.ColorOffsets[0].color.g);
        // Second color should be blue
        Assert.Equal(0f, styles.BackgroundGradient.Value.ColorOffsets[1].color.r);
        Assert.Equal(0f, styles.BackgroundGradient.Value.ColorOffsets[1].color.g);
        Assert.Equal(1f, styles.BackgroundGradient.Value.ColorOffsets[1].color.b);
    }

    [Fact]
    public void Styles_Set_ParsesLinearGradientWithToDirection()
    {
        var styles = new Styles();
        var result = styles.Set("background", "linear-gradient(to right, red, blue)");

        Assert.True(result);
        Assert.NotNull(styles.BackgroundGradient);
        Assert.True(styles.BackgroundGradient.Value.IsValid);
        // "to right" should be 90 degrees = PI/2 radians
        Assert.True(Math.Abs(styles.BackgroundGradient.Value.Angle - (MathF.PI / 2f)) < 0.01f);
    }

    [Fact]
    public void Styles_Set_ParsesRadialGradient()
    {
        var styles = new Styles();
        var result = styles.Set("background", "radial-gradient(#ff0000, #0000ff)");

        Assert.True(result);
        Assert.NotNull(styles.BackgroundGradient);
        Assert.True(styles.BackgroundGradient.Value.IsValid);
        Assert.Equal(GradientInfo.GradientTypes.Radial, styles.BackgroundGradient.Value.GradientType);
    }

    [Fact]
    public void Styles_Set_BackgroundColor_ClearsGradient()
    {
        var styles = new Styles();
        
        // First set a gradient
        styles.Set("background", "linear-gradient(#ff0000, #0000ff)");
        Assert.NotNull(styles.BackgroundGradient);
        
        // Then set a solid color
        styles.Set("background", "#00ff00");
        Assert.Null(styles.BackgroundGradient);
        Assert.NotNull(styles.BackgroundColor);
        Assert.Equal(0f, styles.BackgroundColor.Value.r);
        Assert.Equal(1f, styles.BackgroundColor.Value.g);
        Assert.Equal(0f, styles.BackgroundColor.Value.b);
    }

    [Fact]
    public void StyleSheet_FromString_ParsesGradientRule()
    {
        var css = ".button { background: linear-gradient(135deg, #007acc 0%, #005a9e 100%); }";
        var sheet = StyleSheet.FromString(css);

        Assert.NotNull(sheet);
        Assert.Single(sheet.Nodes);
        Assert.NotNull(sheet.Nodes[0].Styles.BackgroundGradient);
        Assert.True(sheet.Nodes[0].Styles.BackgroundGradient.Value.IsValid);
    }
}

/// <summary>
/// Test panel with StyleSheet attribute for testing
/// </summary>
[StyleSheet("test.scss")]
internal class TestPanelWithStyleSheet : Panel { }

/// <summary>
/// Test panel with SourceLocation attribute for testing
/// </summary>
[SourceLocation("/test/path/TestPanel.razor", 1)]
internal class TestPanelWithSourceLocation : Panel { }
