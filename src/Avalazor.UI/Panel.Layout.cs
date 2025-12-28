using SkiaSharp;
using Avalazor.UI.Yoga;

namespace Avalazor.UI;

/// <summary>
/// Panel partial class: Layout computation
/// Based on s&box's Panel.Layout.cs
/// Implements PreLayout/FinalLayout pattern with Yoga integration
/// </summary>
public partial class Panel
{
    /// <summary>
    /// PreLayout phase - computes styles and applies to Yoga.
    /// Based on s&box's Panel.Layout.cs PreLayout() method (line 139)
    /// This is called recursively before FinalLayout()
    /// </summary>
    internal virtual void PreLayout(LayoutCascade cascade)
    {
        Console.WriteLine($"PreLayout ENTRY: {Tag}, YogaNode={YogaNode != null}, needsPreLayout={needsPreLayout}");
        
        if (YogaNode == null)
            return;

        if (!needsPreLayout)
            return;

        needsPreLayout = false;

        // Compute styles for this panel
        if (_styleEngine != null)
        {
            Console.WriteLine($"PreLayout: Computing styles for {Tag} with inline style: '{Style}'");
            _computedStyle = _styleEngine.ComputeStyle(this);
            
            // Debug logging
            if (_computedStyle?.BackgroundColor != null)
            {
                Console.WriteLine($"PreLayout: {Tag} (classes: {string.Join(", ", Classes)}) has background: {_computedStyle.BackgroundColor}");
            }
            else
            {
                Console.WriteLine($"PreLayout: {Tag} has NO background color computed");
            }
        }
        else
        {
            Console.WriteLine($"PreLayout: {Tag} has NO StyleEngine!");
        }

        // Update visibility
        IsVisible = _computedStyle?.Display != "none";

        // Apply computed styles to Yoga node
        UpdateYoga();

        // Recursively process children
        if (_children != null && _children.Count > 0)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].SetStyleEngine(_styleEngine);
                _children[i].PreLayout(cascade);
            }
        }
        
        // Summary log
        Console.WriteLine($"PreLayout COMPLETE: {Tag}, Background={_computedStyle?.BackgroundColor != null}, Box.Rect=({Box.Rect.Left},{Box.Rect.Top},{Box.Rect.Width},{Box.Rect.Height})");
    }

    /// <summary>
    /// Update Yoga node properties from computed style.
    /// Based on s&box's Panel.Layout.cs UpdateYoga() method (line 233)
    /// </summary>
    internal void UpdateYoga()
    {
        if (_computedStyle == null || YogaNode == null)
            return;

        // Display
        if (_computedStyle.Display == "none")
        {
            YogaNode.Display = YGDisplay.None;
        }
        else
        {
            YogaNode.Display = YGDisplay.Flex;
        }

        // Flex direction
        YogaNode.FlexDirection = _computedStyle.FlexDirection switch
        {
            FlexDirection.Row => YGFlexDirection.Row,
            FlexDirection.Column => YGFlexDirection.Column,
            FlexDirection.RowReverse => YGFlexDirection.RowReverse,
            FlexDirection.ColumnReverse => YGFlexDirection.ColumnReverse,
            _ => YGFlexDirection.Column
        };

        // Justify content
        YogaNode.JustifyContent = _computedStyle.JustifyContent switch
        {
            JustifyContent.FlexStart => YGJustify.FlexStart,
            JustifyContent.FlexEnd => YGJustify.FlexEnd,
            JustifyContent.Center => YGJustify.Center,
            JustifyContent.SpaceBetween => YGJustify.SpaceBetween,
            JustifyContent.SpaceAround => YGJustify.SpaceAround,
            _ => YGJustify.FlexStart
        };

        // Align items
        YogaNode.AlignItems = _computedStyle.AlignItems switch
        {
            AlignItems.FlexStart => YGAlign.FlexStart,
            AlignItems.FlexEnd => YGAlign.FlexEnd,
            AlignItems.Center => YGAlign.Center,
            AlignItems.Stretch => YGAlign.Stretch,
            _ => YGAlign.Stretch
        };

        // Dimensions
        if (_computedStyle.Width.HasValue && _computedStyle.Width.Value > 0)
            YogaNode.SetWidth(new YGValue { Value = _computedStyle.Width.Value, Unit = YGUnit.Point });
        else
            YogaNode.SetWidth(new YGValue { Unit = YGUnit.Auto });

        if (_computedStyle.Height.HasValue && _computedStyle.Height.Value > 0)
            YogaNode.SetHeight(new YGValue { Value = _computedStyle.Height.Value, Unit = YGUnit.Point });
        else
            YogaNode.SetHeight(new YGValue { Unit = YGUnit.Auto });

        // Flex properties
        YogaNode.FlexGrow = _computedStyle.FlexGrow;
        YogaNode.FlexShrink = _computedStyle.FlexShrink;

        // Padding
        if (_computedStyle.PaddingTop > 0)
            YogaNode.SetPadding(YGEdge.Top, new YGValue { Value = _computedStyle.PaddingTop, Unit = YGUnit.Point });
        if (_computedStyle.PaddingRight > 0)
            YogaNode.SetPadding(YGEdge.Right, new YGValue { Value = _computedStyle.PaddingRight, Unit = YGUnit.Point });
        if (_computedStyle.PaddingBottom > 0)
            YogaNode.SetPadding(YGEdge.Bottom, new YGValue { Value = _computedStyle.PaddingBottom, Unit = YGUnit.Point });
        if (_computedStyle.PaddingLeft > 0)
            YogaNode.SetPadding(YGEdge.Left, new YGValue { Value = _computedStyle.PaddingLeft, Unit = YGUnit.Point });

        // Margin
        if (_computedStyle.MarginTop != 0)
            YogaNode.SetMargin(YGEdge.Top, new YGValue { Value = _computedStyle.MarginTop, Unit = YGUnit.Point });
        if (_computedStyle.MarginRight != 0)
            YogaNode.SetMargin(YGEdge.Right, new YGValue { Value = _computedStyle.MarginRight, Unit = YGUnit.Point });
        if (_computedStyle.MarginBottom != 0)
            YogaNode.SetMargin(YGEdge.Bottom, new YGValue { Value = _computedStyle.MarginBottom, Unit = YGUnit.Point });
        if (_computedStyle.MarginLeft != 0)
            YogaNode.SetMargin(YGEdge.Left, new YGValue { Value = _computedStyle.MarginLeft, Unit = YGUnit.Point });
    }

    /// <summary>
    /// FinalLayout phase - reads Yoga results and updates Box rects.
    /// Based on s&box's Panel.Layout.cs FinalLayout() method (line 307)
    /// This is called recursively after Yoga calculation
    /// </summary>
    internal virtual void FinalLayout(LayoutCascade cascade)
    {
        if (YogaNode == null || !IsVisible)
            return;

        // Read layout results from Yoga
        var x = YogaNode.LayoutX;
        var y = YogaNode.LayoutY;
        var width = YogaNode.LayoutWidth;
        var height = YogaNode.LayoutHeight;

        // Update Box rects
        Box.Rect = new SKRect(x, y, x + width, y + height);

        // Calculate inner rect (excluding padding)
        var paddingLeft = YogaNode.GetLayoutPadding(YGEdge.Left);
        var paddingTop = YogaNode.GetLayoutPadding(YGEdge.Top);
        var paddingRight = YogaNode.GetLayoutPadding(YGEdge.Right);
        var paddingBottom = YogaNode.GetLayoutPadding(YGEdge.Bottom);

        Box.RectInner = new SKRect(
            x + paddingLeft,
            y + paddingTop,
            x + width - paddingRight,
            y + height - paddingBottom
        );

        // RectOuter includes margin (same as Rect for now)
        Box.RectOuter = Box.Rect;

        // Recursively process children
        if (_children != null && _children.Count > 0)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                _children[i].FinalLayout(cascade);
            }
        }
    }
}
