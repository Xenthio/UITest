using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Avalazor.UI;
using Sandbox.UI;
using Xunit;

namespace Avalazor.Tests;

/// <summary>
/// Tests for RazorRenderer to ensure RenderTree processing works correctly
/// </summary>
public class RazorRendererTests
{
    private readonly IServiceProvider _serviceProvider;

    public RazorRendererTests()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task RenderComponent_CreatesRootPanel()
    {
        // Arrange
        var renderer = new RazorRenderer(_serviceProvider);

        // Act
        var panel = await renderer.RenderComponent<TestComponent>();

        // Assert
        Assert.NotNull(panel);
    }

    [Fact]
    public async Task RenderComponent_ProcessesElementFrames()
    {
        // Arrange
        var renderer = new RazorRenderer(_serviceProvider);

        // Act
        var panel = await renderer.RenderComponent<TestComponentWithDiv>();

        // Assert
        Assert.NotNull(panel);
        // The panel should have been created from the div element
        Assert.True(panel.ChildrenCount > 0 || panel.ElementName == "div");
    }

    [Fact]
    public async Task RenderComponent_ExtractsTextContent()
    {
        // Arrange
        var renderer = new RazorRenderer(_serviceProvider);

        // Act
        var panel = await renderer.RenderComponent<TestComponentWithText>();

        // Assert
        Assert.NotNull(panel);
        
        // Find the label with text
        var hasTextLabel = FindLabelWithText(panel, "Test Content");
        Assert.True(hasTextLabel, "Should have created a Label with 'Test Content'");
    }

    [Fact]
    public async Task RenderComponent_CreatesNestedStructure()
    {
        // Arrange
        var renderer = new RazorRenderer(_serviceProvider);

        // Act
        var panel = await renderer.RenderComponent<TestComponentNested>();

        // Assert
        Assert.NotNull(panel);
        
        // Should have children for nested structure
        int totalPanels = CountPanels(panel);
        Assert.True(totalPanels > 1, $"Should have multiple panels in hierarchy, but got {totalPanels}");
    }

    [Fact]
    public async Task RenderComponent_HandlesMultipleElements()
    {
        // Arrange
        var renderer = new RazorRenderer(_serviceProvider);

        // Act
        var panel = await renderer.RenderComponent<TestComponentMultiple>();

        // Assert
        Assert.NotNull(panel);
        
        // Should have multiple child elements
        int childCount = panel.ChildrenCount;
        Assert.True(childCount >= 2, $"Should have at least 2 children, but got {childCount}");
    }

    private bool FindLabelWithText(Panel panel, string text)
    {
        if (panel is Label label && label.Text == text)
            return true;

        foreach (var child in panel.Children)
        {
            if (FindLabelWithText(child, text))
                return true;
        }

        return false;
    }

    private int CountPanels(Panel panel)
    {
        int count = 1;
        foreach (var child in panel.Children)
        {
            count += CountPanels(child);
        }
        return count;
    }
}

// Test components
public class TestComponent : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.CloseElement();
    }
}

public class TestComponentWithDiv : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "test-class");
        builder.CloseElement();
    }
}

public class TestComponentWithText : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "p");
        builder.AddContent(1, "Test Content");
        builder.CloseElement();
    }
}

public class TestComponentNested : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "header");
        builder.OpenElement(2, "h1");
        builder.AddContent(3, "Title");
        builder.CloseElement(); // h1
        builder.CloseElement(); // header
        builder.CloseElement(); // div
    }
}

public class TestComponentMultiple : ComponentBase
{
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.OpenElement(1, "p");
        builder.AddContent(2, "First");
        builder.CloseElement();
        builder.OpenElement(3, "p");
        builder.AddContent(4, "Second");
        builder.CloseElement();
        builder.CloseElement();
    }
}
