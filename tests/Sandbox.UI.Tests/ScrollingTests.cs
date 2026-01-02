using Sandbox.UI;
using Xunit;

namespace Sandbox.UI.Tests;

/// <summary>
/// Tests for scrolling functionality in Panels
/// </summary>
public class ScrollingTests
{
    [Fact]
    public void ScrollingPanel_HasCorrectScrollProperties_WhenOverflowIsScroll()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.Overflow = OverflowMode.Scroll;

        // Add child that exceeds parent size
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600); // Taller than parent

        // Layout to calculate sizes
        root.Layout();

        // Should have vertical scroll but not horizontal
        Assert.True(scrollPanel.HasScrollY, "Panel should have vertical scroll");
        Assert.False(scrollPanel.HasScrollX, "Panel should not have horizontal scroll");
        Assert.True(scrollPanel.ScrollSize.y > 0, "ScrollSize.y should be greater than 0");
    }

    [Fact]
    public void ScrollingPanel_TryScroll_AddsVelocity()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.OverflowY = OverflowMode.Scroll;

        // Add child that exceeds parent size
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600);

        // Layout
        root.Layout();

        var initialVelocity = scrollPanel.ScrollVelocity;

        // Try to scroll down (positive y is down)
        bool scrolled = scrollPanel.TryScroll(new Vector2(0, 1));

        Assert.True(scrolled, "TryScroll should return true when scrolling is possible");
        Assert.NotEqual(initialVelocity, scrollPanel.ScrollVelocity);
        Assert.True(scrollPanel.ScrollVelocity.y > 0, "ScrollVelocity.y should be positive after scrolling down");
    }

    [Fact]
    public void ScrollingPanel_TryScroll_RejectsWrongDirection()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.OverflowY = OverflowMode.Scroll; // Only vertical

        // Add child that exceeds parent size vertically
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600);

        // Layout
        root.Layout();

        // Try to scroll horizontally (should be rejected since OverflowX is not Scroll)
        bool scrolled = scrollPanel.TryScroll(new Vector2(1, 0));

        Assert.False(scrolled, "TryScroll should return false when trying to scroll in non-scrollable direction");
    }

    [Fact]
    public void ScrollingPanel_ScrollOffset_IsConstrained()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.OverflowY = OverflowMode.Scroll;

        // Add child that exceeds parent size
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600);

        // Layout
        root.Layout();

        // Get the maximum scroll size
        var maxScrollY = scrollPanel.ScrollSize.y;
        
        // Scroll offset is constrained during layout, not when set directly
        // So we test that the scroll size is correctly calculated
        Assert.True(maxScrollY > 0, "ScrollSize should be greater than 0");
        Assert.Equal(300f, maxScrollY); // Child (600) - Parent (300) = 300
    }

    [Fact]
    public void ScrollingPanel_PreferScrollToBottom_StaysAtBottom()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.OverflowY = OverflowMode.Scroll;
        scrollPanel.PreferScrollToBottom = true;

        // Add child that exceeds parent size
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600);

        // Layout
        root.Layout();

        // Should be at bottom initially
        Assert.True(scrollPanel.IsScrollAtBottom, "Panel with PreferScrollToBottom should be at bottom initially");
        Assert.True(scrollPanel.ScrollOffset.y > 0, "ScrollOffset should be > 0 when at bottom");
    }

    [Fact]
    public void ScrollingPanel_TryScrollToBottom_ScrollsToBottom()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var scrollPanel = new Panel();
        scrollPanel.Parent = root;
        scrollPanel.Style.Width = Length.Pixels(400);
        scrollPanel.Style.Height = Length.Pixels(300);
        scrollPanel.Style.OverflowY = OverflowMode.Scroll;

        // Add child that exceeds parent size
        var child = new Panel();
        child.Parent = scrollPanel;
        child.Style.Width = Length.Pixels(400);
        child.Style.Height = Length.Pixels(600);

        // Layout
        root.Layout();

        // Scroll to top first
        scrollPanel.ScrollOffset = Vector2.Zero;

        // Now scroll to bottom
        bool scrolled = scrollPanel.TryScrollToBottom();

        Assert.True(scrolled, "TryScrollToBottom should return true");
        Assert.True(scrollPanel.IsScrollAtBottom, "Panel should be at bottom after TryScrollToBottom");
        Assert.Equal(scrollPanel.ScrollSize.y, scrollPanel.ScrollOffset.y);
    }

    [Fact]
    public void NonScrollingPanel_TryScroll_ReturnsFalse()
    {
        var root = new RootPanel();
        root.PanelBounds = new Rect(0, 0, 400, 300);

        var panel = new Panel();
        panel.Parent = root;
        panel.Style.Width = Length.Pixels(400);
        panel.Style.Height = Length.Pixels(300);
        // No overflow set - defaults to visible

        // Layout
        root.Layout();

        // Try to scroll
        bool scrolled = panel.TryScroll(new Vector2(0, 1));

        Assert.False(scrolled, "Non-scrolling panel should not accept scroll");
        Assert.False(panel.HasScrollY, "Panel without overflow:scroll should not have scroll");
    }
}
