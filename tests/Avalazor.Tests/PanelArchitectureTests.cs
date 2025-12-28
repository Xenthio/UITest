using Avalazor.UI;
using Xunit;
using SkiaSharp;

namespace Avalazor.Tests;

/// <summary>
/// Tests for the s&box Panel architecture refactor
/// Validates Box, LayoutCascade, and Panel partial classes functionality
/// </summary>
public class PanelArchitectureTests
{
    [Fact]
    public void Box_InitializesWithZeroRects()
    {
        // Arrange & Act
        var box = new Box();

        // Assert
        Assert.Equal(SKRect.Empty, box.Rect);
        Assert.Equal(SKRect.Empty, box.RectInner);
        Assert.Equal(SKRect.Empty, box.RectOuter);
    }

    [Fact]
    public void Box_CanSetRectValues()
    {
        // Arrange
        var box = new Box();
        var testRect = new SKRect(10, 20, 100, 200);

        // Act
        box.Rect = testRect;
        box.RectInner = new SKRect(15, 25, 95, 195);
        box.RectOuter = new SKRect(5, 15, 105, 205);

        // Assert
        Assert.Equal(testRect, box.Rect);
        Assert.Equal(15, box.RectInner.Left);
        Assert.Equal(5, box.RectOuter.Left);
    }

    [Fact]
    public void Panel_HasBoxProperty()
    {
        // Arrange & Act
        var panel = new Panel();

        // Assert
        Assert.NotNull(panel.Box);
        Assert.IsType<Box>(panel.Box);
    }

    [Fact]
    public void Panel_HasComputedStyle_Initially()
    {
        // Arrange & Act
        var panel = new Panel();

        // Assert
        // ComputedStyle is computed during PreLayout, initially null
        Assert.Null(panel.ComputedStyle);
    }

    [Fact]
    public void Panel_AddChild_AddsToChildren()
    {
        // Arrange
        var parent = new Panel();
        var child = new Panel();

        // Act
        parent.AddChild(child);

        // Assert
        Assert.Single(parent.Children);
        Assert.Equal(child, parent.Children[0]);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Panel_RemoveChild_RemovesFromChildren()
    {
        // Arrange
        var parent = new Panel();
        var child = new Panel();
        parent.AddChild(child);

        // Act
        parent.RemoveChild(child);

        // Assert
        Assert.Empty(parent.Children);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void Panel_AddClass_AddsToClasses()
    {
        // Arrange
        var panel = new Panel();

        // Act
        panel.AddClass("test-class");
        panel.AddClass("another-class");

        // Assert
        Assert.Equal(2, panel.Classes.Count);
        Assert.Contains("test-class", panel.Classes);
        Assert.Contains("another-class", panel.Classes);
    }

    [Fact]
    public void Panel_RemoveClass_RemovesFromClasses()
    {
        // Arrange
        var panel = new Panel();
        panel.AddClass("test-class");
        panel.AddClass("another-class");

        // Act
        panel.RemoveClass("test-class");

        // Assert
        Assert.Single(panel.Classes);
        Assert.DoesNotContain("test-class", panel.Classes);
        Assert.Contains("another-class", panel.Classes);
    }

    [Fact]
    public void Panel_SetClass_ReplacesAllClasses()
    {
        // Arrange
        var panel = new Panel();
        panel.AddClass("old-class");
        panel.AddClass("another-class");

        // Act
        panel.SetClass("new-class");

        // Assert
        Assert.Single(panel.Classes);
        Assert.Contains("new-class", panel.Classes);
        Assert.DoesNotContain("old-class", panel.Classes);
    }

    [Fact]
    public void Panel_HasClass_ChecksForClass()
    {
        // Arrange
        var panel = new Panel();
        panel.AddClass("test-class");

        // Act & Assert
        Assert.True(panel.HasClass("test-class"));
        Assert.False(panel.HasClass("nonexistent"));
    }

    [Fact]
    public void Panel_ToggleClass_TogglesClass()
    {
        // Arrange
        var panel = new Panel();

        // Act - add by toggle
        panel.ToggleClass("test-class");

        // Assert
        Assert.Contains("test-class", panel.Classes);

        // Act - remove by toggle
        panel.ToggleClass("test-class");

        // Assert
        Assert.DoesNotContain("test-class", panel.Classes);
    }

    [Fact]
    public void Panel_ComputedStyle_ReadOnly()
    {
        // Arrange
        var panel = new Panel();

        // Assert - ComputedStyle is initially null and is computed internally
        Assert.Null(panel.ComputedStyle);
        
        // ComputedStyle is read-only and computed during PreLayout
        // This test verifies the property exists and is accessible
        var style = panel.ComputedStyle;
        Assert.True(style == null || style is ComputedStyle);
    }

    [Fact]
    public void Panel_Children_InitiallyEmpty()
    {
        // Arrange & Act
        var panel = new Panel();

        // Assert
        Assert.Empty(panel.Children);
        Assert.IsAssignableFrom<IReadOnlyList<Panel>>(panel.Children);
    }

    [Fact]
    public void Panel_Tag_IsSetCorrectly()
    {
        // Arrange & Act
        var panel = new Panel { Tag = "div" };

        // Assert
        Assert.Equal("div", panel.Tag);
    }

    [Fact]
    public void Panel_IsVisible_DefaultsToTrue()
    {
        // Arrange & Act
        var panel = new Panel();

        // Assert
        Assert.True(panel.IsVisible);
    }

    [Fact]
    public void Panel_Style_CanBeSet()
    {
        // Arrange
        var panel = new Panel();

        // Act
        panel.Style = "background-color: red; padding: 10px;";

        // Assert
        Assert.Equal("background-color: red; padding: 10px;", panel.Style);
    }

    [Fact]
    public void LayoutCascade_CanBeCreated()
    {
        // Arrange & Act
        var cascade = new LayoutCascade();

        // Assert
        Assert.NotNull(cascade);
        // Properties can be set after construction
        cascade.AvailableWidth = 800;
        cascade.AvailableHeight = 600;
        Assert.Equal(800, cascade.AvailableWidth);
        Assert.Equal(600, cascade.AvailableHeight);
    }

    [Fact]
    public void Panel_MultipleChildren_MaintainsHierarchy()
    {
        // Arrange
        var root = new Panel { Tag = "div" };
        var header = new Panel { Tag = "header" };
        var main = new Panel { Tag = "main" };
        var footer = new Panel { Tag = "footer" };

        // Act
        root.AddChild(header);
        root.AddChild(main);
        root.AddChild(footer);

        // Assert
        Assert.Equal(3, root.Children.Count);
        Assert.Equal(header, root.Children[0]);
        Assert.Equal(main, root.Children[1]);
        Assert.Equal(footer, root.Children[2]);
        Assert.All(root.Children, child => Assert.Equal(root, child.Parent));
    }

    [Fact]
    public void Panel_DeepHierarchy_MaintainsParentChild()
    {
        // Arrange
        var grandparent = new Panel { Tag = "div" };
        var parent = new Panel { Tag = "section" };
        var child = new Panel { Tag = "p" };

        // Act
        grandparent.AddChild(parent);
        parent.AddChild(child);

        // Assert
        Assert.Equal(parent, grandparent.Children[0]);
        Assert.Equal(child, parent.Children[0]);
        Assert.Equal(grandparent, parent.Parent);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Panel_AddChild_MultipleTimesOnlyAddsOnce()
    {
        // Arrange
        var parent = new Panel();
        var child = new Panel();

        // Act
        parent.AddChild(child);
        parent.AddChild(child); // Try to add again

        // Assert
        Assert.Single(parent.Children);
    }

    [Fact]
    public void Panel_Box_HasAllRectProperties()
    {
        // Arrange
        var panel = new Panel();

        // Assert - Box should have all three rect properties
        Assert.NotNull(panel.Box);
        Assert.Equal(SKRect.Empty, panel.Box.Rect);
        Assert.Equal(SKRect.Empty, panel.Box.RectInner);
        Assert.Equal(SKRect.Empty, panel.Box.RectOuter);
        
        // Act - Set a rect value
        panel.Box.Rect = new SKRect(0, 0, 100, 50);
        
        // Assert
        Assert.Equal(100, panel.Box.Rect.Width);
        Assert.Equal(50, panel.Box.Rect.Height);
    }
}
