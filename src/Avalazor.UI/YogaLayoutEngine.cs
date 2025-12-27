using Facebook.Yoga;

namespace Avalazor.UI;

/// <summary>
/// Yoga flexbox layout engine integration
/// Based on s&box's layout system
/// </summary>
public class YogaLayoutEngine
{
    public static void Layout(Panel panel, float availableWidth, float availableHeight)
    {
        var yogaNode = CreateYogaTree(panel);
        
        yogaNode.CalculateLayout(availableWidth, availableHeight);
        
        ApplyLayout(panel, yogaNode);
    }

    private static YogaNode CreateYogaTree(Panel panel)
    {
        var node = new YogaNode();
        var style = panel.ComputedStyle;

        // Flex properties
        if (style.FlexDirection.HasValue)
        {
            node.FlexDirection = style.FlexDirection.Value switch
            {
                "row" => YogaFlexDirection.Row,
                "column" => YogaFlexDirection.Column,
                "row-reverse" => YogaFlexDirection.RowReverse,
                "column-reverse" => YogaFlexDirection.ColumnReverse,
                _ => YogaFlexDirection.Column
            };
        }
        else
        {
            node.FlexDirection = YogaFlexDirection.Column; // Default flex column
        }

        if (style.FlexGrow > 0) node.FlexGrow = style.FlexGrow;
        if (style.FlexShrink > 0) node.FlexShrink = style.FlexShrink;

        // Justify and align
        if (style.JustifyContent.HasValue)
        {
            node.JustifyContent = style.JustifyContent.Value switch
            {
                "flex-start" => YogaJustify.FlexStart,
                "center" => YogaJustify.Center,
                "flex-end" => YogaJustify.FlexEnd,
                "space-between" => YogaJustify.SpaceBetween,
                "space-around" => YogaJustify.SpaceAround,
                _ => YogaJustify.FlexStart
            };
        }

        if (style.AlignItems.HasValue)
        {
            node.AlignItems = style.AlignItems.Value switch
            {
                "flex-start" => YogaAlign.FlexStart,
                "center" => YogaAlign.Center,
                "flex-end" => YogaAlign.FlexEnd,
                "stretch" => YogaAlign.Stretch,
                _ => YogaAlign.FlexStart
            };
        }

        // Dimensions
        if (style.Width > 0) node.Width = style.Width;
        if (style.Height > 0) node.Height = style.Height;
        if (style.MinWidth > 0) node.MinWidth = style.MinWidth;
        if (style.MinHeight > 0) node.MinHeight = style.MinHeight;
        if (style.MaxWidth > 0) node.MaxWidth = style.MaxWidth;
        if (style.MaxHeight > 0) node.MaxHeight = style.MaxHeight;

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
        if (style.Position == "absolute")
        {
            node.PositionType = YogaPositionType.Absolute;
            if (style.Top > 0) node.Top = style.Top;
            if (style.Right > 0) node.Right = style.Right;
            if (style.Bottom > 0) node.Bottom = style.Bottom;
            if (style.Left > 0) node.Left = style.Left;
        }

        // Add children
        foreach (var child in panel.Children)
        {
            var childNode = CreateYogaTree(child);
            node.AddChild(childNode);
        }

        return node;
    }

    private static void ApplyLayout(Panel panel, YogaNode node)
    {
        // Apply computed layout
        panel.LayoutNode.X = node.LayoutX;
        panel.LayoutNode.Y = node.LayoutY;
        panel.LayoutNode.Width = node.LayoutWidth;
        panel.LayoutNode.Height = node.LayoutHeight;

        // Apply to children
        for (int i = 0; i < panel.Children.Count && i < node.Count; i++)
        {
            ApplyLayout(panel.Children[i], node[i]);
        }
    }
}
