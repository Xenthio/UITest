using YogaSharp;

namespace Avalazor.UI;

/// <summary>
/// Yoga flexbox layout engine integration using antokhio.YogaSharp
/// Based on s&box's layout system
/// </summary>
public class YogaLayoutEngine
{
    public static void Layout(Panel panel, float availableWidth, float availableHeight)
    {
        var yogaNode = CreateYogaTree(panel);
        
        yogaNode.CalculateLayout();
        
        ApplyLayout(panel, yogaNode, 0, 0);
    }

    private static YogaNode CreateYogaTree(Panel panel)
    {
        var node = new YogaNode();
        var style = panel.ComputedStyle;

        if (style == null) return node;

        // Flex properties
        node.FlexDirection = style.FlexDirection switch
        {
            FlexDirection.Row => YogaFlexDirection.Row,
            FlexDirection.Column => YogaFlexDirection.Column,
            FlexDirection.RowReverse => YogaFlexDirection.RowReverse,
            FlexDirection.ColumnReverse => YogaFlexDirection.ColumnReverse,
            _ => YogaFlexDirection.Column
        };

        if (style.FlexGrow > 0) node.FlexGrow = style.FlexGrow;
        if (style.FlexShrink > 0) node.FlexShrink = style.FlexShrink;

        // Justify and align
        node.JustifyContent = style.JustifyContent switch
        {
            JustifyContent.FlexStart => YogaJustify.FlexStart,
            JustifyContent.Center => YogaJustify.Center,
            JustifyContent.FlexEnd => YogaJustify.FlexEnd,
            JustifyContent.SpaceBetween => YogaJustify.SpaceBetween,
            JustifyContent.SpaceAround => YogaJustify.SpaceAround,
            _ => YogaJustify.FlexStart
        };

        node.AlignItems = style.AlignItems switch
        {
            AlignItems.FlexStart => YogaAlign.FlexStart,
            AlignItems.Center => YogaAlign.Center,
            AlignItems.FlexEnd => YogaAlign.FlexEnd,
            AlignItems.Stretch => YogaAlign.Stretch,
            _ => YogaAlign.FlexStart
        };

        // Dimensions
        if (style.Width.HasValue) node.Width = style.Width.Value;
        if (style.Height.HasValue) node.Height = style.Height.Value;
        if (style.MinWidth.HasValue) node.MinWidth = style.MinWidth.Value;
        if (style.MinHeight.HasValue) node.MinHeight = style.MinHeight.Value;
        if (style.MaxWidth.HasValue) node.MaxWidth = style.MaxWidth.Value;
        if (style.MaxHeight.HasValue) node.MaxHeight = style.MaxHeight.Value;

        // Margins
        if (style.MarginTop > 0) node.MarginTop = style.MarginTop;
        if (style.MarginRight > 0) node.MarginRight = style.MarginRight;
        if (style.MarginBottom > 0) node.MarginBottom = style.MarginBottom;
        if (style.MarginLeft > 0) node.MarginLeft = style.MarginLeft;

        // Padding
        if (style.PaddingTop > 0) node.PaddingTop = style.PaddingTop;
        if (style.PaddingRight > 0) node.PaddingRight = style.PaddingRight;
        if (style.PaddingBottom > 0) node.PaddingBottom = style.PaddingBottom;
        if (style.PaddingLeft > 0) node.PaddingLeft = style.PaddingLeft;

        // Position
        if (style.Position == Position.Absolute)
        {
            node.PositionType = YogaPositionType.Absolute;
            if (style.Top.HasValue) node.Top = style.Top.Value;
            if (style.Right.HasValue) node.Right = style.Right.Value;
            if (style.Bottom.HasValue) node.Bottom = style.Bottom.Value;
            if (style.Left.HasValue) node.Left = style.Left.Value;
        }

        // Add children using YogaSharp API
        foreach (var child in panel.Children)
        {
            var childNode = CreateYogaTree(child);
            node.Children.Add(childNode);
        }

        return node;
    }

    private static void ApplyLayout(Panel panel, YogaNode node, float offsetX, float offsetY)
    {
        if (panel.LayoutNode == null) return;

        // Apply computed layout with offsets
        panel.LayoutNode.X = node.LayoutX + offsetX;
        panel.LayoutNode.Y = node.LayoutY + offsetY;
        panel.LayoutNode.Width = node.LayoutWidth;
        panel.LayoutNode.Height = node.LayoutHeight;

        // Update ComputedRect for rendering
        panel.ComputedRect = new SkiaSharp.SKRect(
            panel.LayoutNode.X,
            panel.LayoutNode.Y,
            panel.LayoutNode.X + panel.LayoutNode.Width,
            panel.LayoutNode.Y + panel.LayoutNode.Height
        );

        // Apply to children (with cumulative offset)
        for (int i = 0; i < panel.Children.Count && i < node.Children.Count; i++)
        {
            ApplyLayout(panel.Children[i], node.Children[i], panel.LayoutNode.X, panel.LayoutNode.Y);
        }
    }
}
