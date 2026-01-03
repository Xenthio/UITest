using Xunit;
using Sandbox.UI;

namespace Sandbox.UI.Tests;

public class WindowSizingTests
{
    [Fact]
    public void Window_WithoutExplicitSize_ShouldNotHaveExplicitSizeFlag()
    {
        // Arrange
        var window = new Window();
        
        // Act
        var (width, height, hasExplicitSize) = window.GetCalculatedWindowSize();
        
        // Assert
        Assert.False(hasExplicitSize, "Window without explicit size should not have hasExplicitSize flag set");
    }
    
    [Fact]
    public void Window_WithExplicitWidth_ShouldHaveExplicitSizeFlag()
    {
        // Arrange
        var window = new Window();
        
        // Act
        window.WindowWidth = 800;
        var (width, height, hasExplicitSize) = window.GetCalculatedWindowSize();
        
        // Assert - partial explicit size (only width) still needs height
        Assert.False(hasExplicitSize, "Window with only width set should not have hasExplicitSize flag set");
    }
    
    [Fact]
    public void Window_WithExplicitWidthAndHeight_ShouldHaveExplicitSizeFlag()
    {
        // Arrange
        var window = new Window();
        
        // Act
        window.WindowWidth = 800;
        window.WindowHeight = 600;
        var (width, height, hasExplicitSize) = window.GetCalculatedWindowSize();
        
        // Assert
        Assert.True(hasExplicitSize, "Window with both width and height set should have hasExplicitSize flag set");
        Assert.Equal(800, width);
        Assert.Equal(600, height);
    }
    
    [Fact]
    public void Window_AfterLayout_ShouldCalculateSizeFromContent()
    {
        // Arrange
        var window = new Window();
        var rootPanel = new RootPanel();
        rootPanel.PanelBounds = new Rect(0, 0, 10000, 10000); // Large bounds for natural sizing
        rootPanel.AddChild(window);
        
        // Add some content
        var label = new Label("Test Content");
        window.AddChild(label);
        
        // Act
        rootPanel.Layout();
        var (width, height, hasExplicitSize) = window.GetCalculatedWindowSize();
        
        // Assert
        Assert.False(hasExplicitSize, "Window without explicit size should not have hasExplicitSize flag");
        Assert.True(width > 0, "Calculated width should be greater than 0");
        Assert.True(height > 0, "Calculated height should be greater than 0");
    }
    
    [Fact]
    public void Window_WithMinSize_ShouldRespectMinimumConstraints()
    {
        // Arrange
        var window = new Window();
        window.MinSize = new Vector2(200, 100);
        var rootPanel = new RootPanel();
        rootPanel.PanelBounds = new Rect(0, 0, 10000, 10000);
        rootPanel.AddChild(window);
        
        // Act
        rootPanel.Layout();
        var (width, height, hasExplicitSize) = window.GetCalculatedWindowSize();
        
        // Assert
        Assert.True(width >= 200, $"Calculated width {width} should be at least MinSize.x (200)");
        Assert.True(height >= 100, $"Calculated height {height} should be at least MinSize.y (100)");
    }
}
