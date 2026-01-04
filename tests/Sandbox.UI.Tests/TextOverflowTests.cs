using Xunit;
using Sandbox.UI;

namespace Sandbox.UI.Tests;

/// <summary>
/// Tests for text overflow properties (text-overflow, white-space, word-break)
/// </summary>
public class TextOverflowTests
{
    [Fact]
    public void Label_WithTextOverflowEllipsis_SetsProperty()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("This is a very long text that should be truncated");
        rootPanel.AddChild(label);

        // Create stylesheet with text-overflow
        var css = @"
            label {
                text-overflow: ellipsis;
                width: 100px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(TextOverflow.Ellipsis, label.ComputedStyle.TextOverflow);
    }

    [Fact]
    public void Label_WithTextOverflowClip_SetsProperty()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("This is a very long text that should be clipped");
        rootPanel.AddChild(label);

        // Create stylesheet with text-overflow
        var css = @"
            label {
                text-overflow: clip;
                width: 100px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(TextOverflow.Clip, label.ComputedStyle.TextOverflow);
    }

    [Fact]
    public void Label_WithWhiteSpaceNoWrap_SetsProperty()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("This text should not wrap to new lines");
        rootPanel.AddChild(label);

        // Create stylesheet with white-space: nowrap
        var css = @"
            label {
                white-space: nowrap;
                width: 100px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(WhiteSpace.NoWrap, label.ComputedStyle.WhiteSpace);
    }

    [Fact]
    public void Label_WithWhiteSpacePre_SetsProperty()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("This text\nhas newlines\nthat should be preserved");
        rootPanel.AddChild(label);

        // Create stylesheet with white-space: pre
        var css = @"
            label {
                white-space: pre;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(WhiteSpace.Pre, label.ComputedStyle.WhiteSpace);
    }

    [Fact]
    public void Label_WithWordBreakBreakAll_SetsProperty()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("verylongwordthatshouldbreakatanycharacter");
        rootPanel.AddChild(label);

        // Create stylesheet with word-break
        var css = @"
            label {
                word-break: break-all;
                width: 100px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(WordBreak.BreakAll, label.ComputedStyle.WordBreak);
    }

    [Fact]
    public void Label_WithCombinedOverflowProperties_SetsAllProperties()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("Combined overflow test text");
        rootPanel.AddChild(label);

        // Create stylesheet with multiple overflow properties
        var css = @"
            label {
                text-overflow: ellipsis;
                white-space: nowrap;
                word-break: normal;
                width: 100px;
            }
        ";
        var sheet = StyleSheet.FromString(css);
        rootPanel.StyleSheet.Add(sheet);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        Assert.Equal(TextOverflow.Ellipsis, label.ComputedStyle.TextOverflow);
        Assert.Equal(WhiteSpace.NoWrap, label.ComputedStyle.WhiteSpace);
        Assert.Equal(WordBreak.Normal, label.ComputedStyle.WordBreak);
    }

    [Fact]
    public void Label_WithoutOverflowProperties_UsesDefaults()
    {
        // Arrange
        var rootPanel = new RootPanel();
        var label = new Label("Default overflow behavior");
        rootPanel.AddChild(label);

        // Act
        rootPanel.Layout();

        // Assert
        Assert.NotNull(label.ComputedStyle);
        // Default values should be set during layout
        Assert.Equal(TextOverflow.None, label.ComputedStyle.TextOverflow ?? TextOverflow.None);
        Assert.Equal(WhiteSpace.Normal, label.ComputedStyle.WhiteSpace ?? WhiteSpace.Normal);
        Assert.Equal(WordBreak.Normal, label.ComputedStyle.WordBreak ?? WordBreak.Normal);
    }
}
