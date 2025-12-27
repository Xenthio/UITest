using System;
using System.Collections.Generic;
using Avalazor.UI.Yoga;
using SkiaSharp;

namespace Avalazor.UI
{
    /// <summary>
    /// Layout engine using Facebook's Yoga flexbox implementation via native P/Invoke
    /// </summary>
    public class YogaLayoutEngine : IDisposable
    {
        private readonly Dictionary<Panel, YGNodeRef> _nodeMap = new();
        private YGConfigRef _config;

        public YogaLayoutEngine()
        {
            // Create Yoga configuration
            _config = YogaNative.YGConfigNew();
            YogaNative.YGConfigSetUseWebDefaults(_config, true);
        }

        public void CalculateLayout(Panel rootPanel, float availableWidth, float availableHeight)
        {
            // Create or update Yoga node tree
            var rootNode = GetOrCreateNode(rootPanel);
            
            // Apply styles to root
            ApplyStylesToNode(rootPanel, rootNode);
            
            // Recursively build tree
            BuildYogaTree(rootPanel, rootNode);
            
            // Calculate layout
            YogaNative.YGNodeCalculateLayout(rootNode, availableWidth, availableHeight, YGDirection.LTR);
            
            // Apply calculated layout back to panels
            ApplyLayoutToPanels(rootPanel, rootNode, 0, 0);
        }

        private void BuildYogaTree(Panel panel, YGNodeRef node)
        {
            // Remove existing children
            uint childCount = YogaNative.YGNodeGetChildCount(node);
            for (uint i = 0; i < childCount; i++)
            {
                var child = YogaNative.YGNodeGetChild(node, 0); // Always remove first child
                YogaNative.YGNodeRemoveChild(node, child);
            }
            
            // Add children
            for (int i = 0; i < panel.Children.Count; i++)
            {
                var child = panel.Children[i];
                var childNode = GetOrCreateNode(child);
                
                ApplyStylesToNode(child, childNode);
                YogaNative.YGNodeInsertChild(node, childNode, (uint)i);
                
                // Recurse
                BuildYogaTree(child, childNode);
            }
        }

        private YGNodeRef GetOrCreateNode(Panel panel)
        {
            if (!_nodeMap.TryGetValue(panel, out var node))
            {
                node = YogaNative.YGNodeNewWithConfig(_config);
                _nodeMap[panel] = node;
            }
            return node;
        }

        private void ApplyStylesToNode(Panel panel, YGNodeRef node)
        {
            var style = panel.ComputedStyle;
            if (style == null) return;

            // Display
            if (style.Display == "none")
            {
                YogaNative.YGNodeStyleSetDisplay(node, YGDisplay.None);
                return;
            }
            YogaNative.YGNodeStyleSetDisplay(node, YGDisplay.Flex);

            // Flex direction
            if (style.FlexDirection == FlexDirection.Row)
                YogaNative.YGNodeStyleSetFlexDirection(node, YGFlexDirection.Row);
            else if (style.FlexDirection == FlexDirection.Column)
                YogaNative.YGNodeStyleSetFlexDirection(node, YGFlexDirection.Column);
            else if (style.FlexDirection == FlexDirection.RowReverse)
                YogaNative.YGNodeStyleSetFlexDirection(node, YGFlexDirection.RowReverse);
            else if (style.FlexDirection == FlexDirection.ColumnReverse)
                YogaNative.YGNodeStyleSetFlexDirection(node, YGFlexDirection.ColumnReverse);

            // Justify content
            if (style.JustifyContent == JustifyContent.FlexStart)
                YogaNative.YGNodeStyleSetJustifyContent(node, YGJustify.FlexStart);
            else if (style.JustifyContent == JustifyContent.Center)
                YogaNative.YGNodeStyleSetJustifyContent(node, YGJustify.Center);
            else if (style.JustifyContent == JustifyContent.FlexEnd)
                YogaNative.YGNodeStyleSetJustifyContent(node, YGJustify.FlexEnd);
            else if (style.JustifyContent == JustifyContent.SpaceBetween)
                YogaNative.YGNodeStyleSetJustifyContent(node, YGJustify.SpaceBetween);
            else if (style.JustifyContent == JustifyContent.SpaceAround)
                YogaNative.YGNodeStyleSetJustifyContent(node, YGJustify.SpaceAround);

            // Align items
            if (style.AlignItems == AlignItems.FlexStart)
                YogaNative.YGNodeStyleSetAlignItems(node, YGAlign.FlexStart);
            else if (style.AlignItems == AlignItems.Center)
                YogaNative.YGNodeStyleSetAlignItems(node, YGAlign.Center);
            else if (style.AlignItems == AlignItems.FlexEnd)
                YogaNative.YGNodeStyleSetAlignItems(node, YGAlign.FlexEnd);
            else if (style.AlignItems == AlignItems.Stretch)
                YogaNative.YGNodeStyleSetAlignItems(node, YGAlign.Stretch);

            // Dimensions
            if (style.Width.HasValue)
                YogaNative.YGNodeStyleSetWidth(node, style.Width.Value);
            else
                YogaNative.YGNodeStyleSetWidthAuto(node);

            if (style.Height.HasValue)
                YogaNative.YGNodeStyleSetHeight(node, style.Height.Value);
            else
                YogaNative.YGNodeStyleSetHeightAuto(node);

            // Min/Max dimensions
            if (style.MinWidth.HasValue)
                YogaNative.YGNodeStyleSetMinWidth(node, style.MinWidth.Value);
            if (style.MaxWidth.HasValue)
                YogaNative.YGNodeStyleSetMaxWidth(node, style.MaxWidth.Value);
            if (style.MinHeight.HasValue)
                YogaNative.YGNodeStyleSetMinHeight(node, style.MinHeight.Value);
            if (style.MaxHeight.HasValue)
                YogaNative.YGNodeStyleSetMaxHeight(node, style.MaxHeight.Value);

            // Flex properties
            YogaNative.YGNodeStyleSetFlexGrow(node, style.FlexGrow);
            YogaNative.YGNodeStyleSetFlexShrink(node, style.FlexShrink);

            // Padding
            if (style.PaddingTop > 0)
                YogaNative.YGNodeStyleSetPadding(node, YGEdge.Top, style.PaddingTop);
            if (style.PaddingRight > 0)
                YogaNative.YGNodeStyleSetPadding(node, YGEdge.Right, style.PaddingRight);
            if (style.PaddingBottom > 0)
                YogaNative.YGNodeStyleSetPadding(node, YGEdge.Bottom, style.PaddingBottom);
            if (style.PaddingLeft > 0)
                YogaNative.YGNodeStyleSetPadding(node, YGEdge.Left, style.PaddingLeft);

            // Margin
            if (style.MarginTop != 0)
                YogaNative.YGNodeStyleSetMargin(node, YGEdge.Top, style.MarginTop);
            if (style.MarginRight != 0)
                YogaNative.YGNodeStyleSetMargin(node, YGEdge.Right, style.MarginRight);
            if (style.MarginBottom != 0)
                YogaNative.YGNodeStyleSetMargin(node, YGEdge.Bottom, style.MarginBottom);
            if (style.MarginLeft != 0)
                YogaNative.YGNodeStyleSetMargin(node, YGEdge.Left, style.MarginLeft);
        }

        private void ApplyLayoutToPanels(Panel panel, YGNodeRef node, float parentX, float parentY)
        {
            // Get calculated layout from Yoga
            var left = YogaNative.YGNodeLayoutGetLeft(node);
            var top = YogaNative.YGNodeLayoutGetTop(node);
            var width = YogaNative.YGNodeLayoutGetWidth(node);
            var height = YogaNative.YGNodeLayoutGetHeight(node);

            // Set computed rect (absolute coordinates)
            panel.ComputedRect = new SKRect(
                parentX + left,
                parentY + top,
                parentX + left + width,
                parentY + top + height
            );

            // Apply to children recursively
            for (int i = 0; i < panel.Children.Count; i++)
            {
                var child = panel.Children[i];
                if (_nodeMap.TryGetValue(child, out var childNode))
                {
                    ApplyLayoutToPanels(child, childNode, panel.ComputedRect.Left, panel.ComputedRect.Top);
                }
            }
        }

        public void Dispose()
        {
            // Free all Yoga nodes
            foreach (var node in _nodeMap.Values)
            {
                if (!node.IsNull)
                {
                    YogaNative.YGNodeFree(node);
                }
            }
            _nodeMap.Clear();

            // Free config
            if (!_config.IsNull)
            {
                YogaNative.YGConfigFree(_config);
                _config = YGConfigRef.Null;
            }
        }
    }
}
