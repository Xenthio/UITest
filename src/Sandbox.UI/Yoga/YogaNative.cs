using System.Runtime.InteropServices;

namespace Sandbox.UI;

#pragma warning disable CS8981

/// <summary>
/// Yoga native bindings
/// Based on s&box's Yoga integration
/// </summary>
public static class YogaNative
{
    private const string DllName = "yoga";

    // Config
    [DllImport(DllName)] public static extern YGConfigRef YGConfigNew();
    [DllImport(DllName)] public static extern void YGConfigSetUseWebDefaults(YGConfigRef config, bool enabled);
    [DllImport(DllName)] public static extern void YGConfigSetPointScaleFactor(YGConfigRef config, float scale);

    // Node creation/destruction
    [DllImport(DllName)] public static extern YGNodeRef YGNodeNew();
    [DllImport(DllName)] public static extern YGNodeRef YGNodeNewWithConfig(YGConfigRef config);
    [DllImport(DllName)] public static extern void YGNodeFree(YGNodeRef node);

    // Children
    [DllImport(DllName)] public static extern void YGNodeInsertChild(YGNodeRef node, YGNodeRef child, int index);
    [DllImport(DllName)] public static extern void YGNodeRemoveChild(YGNodeRef node, YGNodeRef child);
    [DllImport(DllName)] public static extern uint YGNodeGetChildCount(YGNodeRef node);

    // Layout
    [DllImport(DllName)] public static extern void YGNodeCalculateLayout(YGNodeRef node, float availableWidth, float availableHeight, YGDirection direction);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetLeft(YGNodeRef node);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetTop(YGNodeRef node);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetWidth(YGNodeRef node);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetHeight(YGNodeRef node);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetMargin(YGNodeRef node, YGEdge edge);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetPadding(YGNodeRef node, YGEdge edge);
    [DllImport(DllName)] public static extern float YGNodeLayoutGetBorder(YGNodeRef node, YGEdge edge);

    [DllImport(DllName)] public static extern bool YGNodeGetHasNewLayout(YGNodeRef node);
    [DllImport(DllName)] public static extern void YGNodeSetHasNewLayout(YGNodeRef node, bool hasNewLayout);
    [DllImport(DllName)] public static extern void YGNodeMarkDirty(YGNodeRef node);

    // Measure function
    [DllImport(DllName)] public static extern void YGNodeSetMeasureFunc(YGNodeRef node, IntPtr measureFunc);

    // Style setters - Width/Height
    [DllImport(DllName)] public static extern void YGNodeStyleSetWidth(YGNodeRef node, float width);
    [DllImport(DllName)] public static extern void YGNodeStyleSetWidthPercent(YGNodeRef node, float width);
    [DllImport(DllName)] public static extern void YGNodeStyleSetWidthAuto(YGNodeRef node);
    [DllImport(DllName)] public static extern void YGNodeStyleSetHeight(YGNodeRef node, float height);
    [DllImport(DllName)] public static extern void YGNodeStyleSetHeightPercent(YGNodeRef node, float height);
    [DllImport(DllName)] public static extern void YGNodeStyleSetHeightAuto(YGNodeRef node);

    [DllImport(DllName)] public static extern void YGNodeStyleSetMinWidth(YGNodeRef node, float minWidth);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMinWidthPercent(YGNodeRef node, float minWidth);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMinHeight(YGNodeRef node, float minHeight);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMinHeightPercent(YGNodeRef node, float minHeight);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMaxWidth(YGNodeRef node, float maxWidth);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMaxWidthPercent(YGNodeRef node, float maxWidth);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMaxHeight(YGNodeRef node, float maxHeight);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMaxHeightPercent(YGNodeRef node, float maxHeight);

    // Style setters - Flex
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexDirection(YGNodeRef node, YGFlexDirection flexDirection);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexWrap(YGNodeRef node, YGWrap wrap);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexGrow(YGNodeRef node, float flexGrow);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexShrink(YGNodeRef node, float flexShrink);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexBasis(YGNodeRef node, float flexBasis);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexBasisPercent(YGNodeRef node, float flexBasis);
    [DllImport(DllName)] public static extern void YGNodeStyleSetFlexBasisAuto(YGNodeRef node);

    // Style setters - Alignment
    [DllImport(DllName)] public static extern void YGNodeStyleSetJustifyContent(YGNodeRef node, YGJustify justifyContent);
    [DllImport(DllName)] public static extern void YGNodeStyleSetAlignItems(YGNodeRef node, YGAlign alignItems);
    [DllImport(DllName)] public static extern void YGNodeStyleSetAlignSelf(YGNodeRef node, YGAlign alignSelf);
    [DllImport(DllName)] public static extern void YGNodeStyleSetAlignContent(YGNodeRef node, YGAlign alignContent);

    // Style setters - Position
    [DllImport(DllName)] public static extern void YGNodeStyleSetPositionType(YGNodeRef node, YGPositionType positionType);
    [DllImport(DllName)] public static extern void YGNodeStyleSetPosition(YGNodeRef node, YGEdge edge, float position);
    [DllImport(DllName)] public static extern void YGNodeStyleSetPositionPercent(YGNodeRef node, YGEdge edge, float position);

    // Style setters - Margin
    [DllImport(DllName)] public static extern void YGNodeStyleSetMargin(YGNodeRef node, YGEdge edge, float margin);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMarginPercent(YGNodeRef node, YGEdge edge, float margin);
    [DllImport(DllName)] public static extern void YGNodeStyleSetMarginAuto(YGNodeRef node, YGEdge edge);

    // Style setters - Padding
    [DllImport(DllName)] public static extern void YGNodeStyleSetPadding(YGNodeRef node, YGEdge edge, float padding);
    [DllImport(DllName)] public static extern void YGNodeStyleSetPaddingPercent(YGNodeRef node, YGEdge edge, float padding);

    // Style setters - Border
    [DllImport(DllName)] public static extern void YGNodeStyleSetBorder(YGNodeRef node, YGEdge edge, float border);

    // Style setters - Other
    [DllImport(DllName)] public static extern void YGNodeStyleSetDisplay(YGNodeRef node, YGDisplay display);
    [DllImport(DllName)] public static extern void YGNodeStyleSetOverflow(YGNodeRef node, YGOverflow overflow);
    [DllImport(DllName)] public static extern void YGNodeStyleSetAspectRatio(YGNodeRef node, float aspectRatio);
    [DllImport(DllName)] public static extern void YGNodeStyleSetGap(YGNodeRef node, YGGutter gutter, float gapLength);
}

// Yoga types
public struct YGNodeRef
{
    public IntPtr Ptr;
    public bool IsNull => Ptr == IntPtr.Zero;
    public static YGNodeRef Null => new() { Ptr = IntPtr.Zero };
}

public struct YGConfigRef
{
    public IntPtr Ptr;
}

// Yoga enums
public enum YGDirection { Inherit, LTR, RTL }
public enum YGFlexDirection { Column, ColumnReverse, Row, RowReverse }
public enum YGJustify { FlexStart, Center, FlexEnd, SpaceBetween, SpaceAround, SpaceEvenly }
public enum YGAlign { Auto, FlexStart, Center, FlexEnd, Stretch, Baseline, SpaceBetween, SpaceAround }
public enum YGPositionType { Static, Relative, Absolute }
public enum YGWrap { NoWrap, Wrap, WrapReverse }
public enum YGOverflow { Visible, Hidden, Scroll }
public enum YGDisplay { Flex, None }
public enum YGEdge { Left, Top, Right, Bottom, Start, End, Horizontal, Vertical, All }
public enum YGGutter { Column, Row, All }
public enum YGMeasureMode { Undefined, Exactly, AtMost }

// Measure function delegate
public delegate Vector2 YGMeasureFunc(YGNodeRef node, float width, YGMeasureMode widthMode, float height, YGMeasureMode heightMode);

#pragma warning restore CS8981
