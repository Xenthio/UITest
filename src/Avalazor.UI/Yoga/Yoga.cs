using System;
using System.Runtime.InteropServices;

namespace Avalazor.UI.Yoga
{
    // Ported from s&box: engine/Sandbox.Engine/Systems/UI/Yoga/
    // Native P/Invoke bindings for Facebook's Yoga layout engine

    #region Native Handles

    [StructLayout(LayoutKind.Sequential)]
    public struct YGNodeRef
    {
        public IntPtr Handle;
        public static YGNodeRef Null => new YGNodeRef { Handle = IntPtr.Zero };
        public bool IsNull => Handle == IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct YGConfigRef
    {
        public IntPtr Handle;
        public static YGConfigRef Null => new YGConfigRef { Handle = IntPtr.Zero };
    }

    #endregion

    #region Enums

    public enum YGAlign
    {
        Auto,
        FlexStart,
        Center,
        FlexEnd,
        Stretch,
        Baseline,
        SpaceBetween,
        SpaceAround
    }

    public enum YGDirection
    {
        Inherit,
        LTR,
        RTL
    }

    public enum YGDisplay
    {
        Flex,
        None
    }

    public enum YGEdge
    {
        Left,
        Top,
        Right,
        Bottom,
        Start,
        End,
        Horizontal,
        Vertical,
        All
    }

    public enum YGFlexDirection
    {
        Column,
        ColumnReverse,
        Row,
        RowReverse
    }

    public enum YGJustify
    {
        FlexStart,
        Center,
        FlexEnd,
        SpaceBetween,
        SpaceAround,
        SpaceEvenly
    }

    public enum YGMeasureMode
    {
        Undefined,
        Exactly,
        AtMost
    }

    public enum YGOverflow
    {
        Visible,
        Hidden,
        Scroll
    }

    public enum YGPositionType
    {
        Relative,
        Absolute
    }

    public enum YGUnit
    {
        Undefined,
        Point,
        Percent,
        Auto
    }

    public enum YGWrap
    {
        NoWrap,
        Wrap,
        WrapReverse
    }

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    public struct YGValue
    {
        public float Value;
        public YGUnit Unit;

        public static YGValue Undefined() => new YGValue { Value = float.NaN, Unit = YGUnit.Undefined };
        public static YGValue Point(float value) => new YGValue { Value = value, Unit = YGUnit.Point };
        public static YGValue Percent(float value) => new YGValue { Value = value, Unit = YGUnit.Percent };
        public static YGValue Auto() => new YGValue { Value = float.NaN, Unit = YGUnit.Auto };
    }

    #endregion

    #region Delegates

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate YGSize YGMeasureFunc(
        YGNodeRef node,
        float width,
        YGMeasureMode widthMode,
        float height,
        YGMeasureMode heightMode);

    [StructLayout(LayoutKind.Sequential)]
    public struct YGSize
    {
        public float Width;
        public float Height;
    }

    #endregion

    #region Native Functions

    public static class YogaNative
    {
        private const string LibName = "yoga";

        // Node lifecycle
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern YGNodeRef YGNodeNew();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern YGNodeRef YGNodeNewWithConfig(YGConfigRef config);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeFree(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeReset(YGNodeRef node);

        // Children
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeInsertChild(YGNodeRef node, YGNodeRef child, uint index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeRemoveChild(YGNodeRef node, YGNodeRef child);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern YGNodeRef YGNodeGetChild(YGNodeRef node, uint index);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint YGNodeGetChildCount(YGNodeRef node);

        // Layout calculation
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeCalculateLayout(
            YGNodeRef node,
            float availableWidth,
            float availableHeight,
            YGDirection ownerDirection);

        // Layout results
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetLeft(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetTop(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetWidth(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetHeight(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetMargin(YGNodeRef node, YGEdge edge);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetPadding(YGNodeRef node, YGEdge edge);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern float YGNodeLayoutGetBorder(YGNodeRef node, YGEdge edge);

        // Style setters - Dimensions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetWidth(YGNodeRef node, float width);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetHeight(YGNodeRef node, float height);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetWidthPercent(YGNodeRef node, float width);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetHeightPercent(YGNodeRef node, float height);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetWidthAuto(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetHeightAuto(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMinWidth(YGNodeRef node, float minWidth);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMinHeight(YGNodeRef node, float minHeight);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMaxWidth(YGNodeRef node, float maxWidth);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMaxHeight(YGNodeRef node, float maxHeight);

        // Style setters - Flex
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexDirection(YGNodeRef node, YGFlexDirection flexDirection);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexGrow(YGNodeRef node, float flexGrow);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexShrink(YGNodeRef node, float flexShrink);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexBasis(YGNodeRef node, float flexBasis);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexBasisPercent(YGNodeRef node, float flexBasis);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexBasisAuto(YGNodeRef node);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetFlexWrap(YGNodeRef node, YGWrap flexWrap);

        // Style setters - Alignment
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetJustifyContent(YGNodeRef node, YGJustify justifyContent);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetAlignItems(YGNodeRef node, YGAlign alignItems);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetAlignSelf(YGNodeRef node, YGAlign alignSelf);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetAlignContent(YGNodeRef node, YGAlign alignContent);

        // Style setters - Position
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetPositionType(YGNodeRef node, YGPositionType positionType);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetPosition(YGNodeRef node, YGEdge edge, float position);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetPositionPercent(YGNodeRef node, YGEdge edge, float position);

        // Style setters - Margin
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMargin(YGNodeRef node, YGEdge edge, float margin);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMarginPercent(YGNodeRef node, YGEdge edge, float margin);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetMarginAuto(YGNodeRef node, YGEdge edge);

        // Style setters - Padding
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetPadding(YGNodeRef node, YGEdge edge, float padding);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetPaddingPercent(YGNodeRef node, YGEdge edge, float padding);

        // Style setters - Border
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetBorder(YGNodeRef node, YGEdge edge, float border);

        // Style setters - Display
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetDisplay(YGNodeRef node, YGDisplay display);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetOverflow(YGNodeRef node, YGOverflow overflow);

        // Style setters - Gap
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetGap(YGNodeRef node, YGEdge gutter, float gapLength);

        // Style setters - Aspect Ratio
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeStyleSetAspectRatio(YGNodeRef node, float aspectRatio);

        // Measure function
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGNodeSetMeasureFunc(YGNodeRef node, YGMeasureFunc measureFunc);

        // Config
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern YGConfigRef YGConfigNew();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGConfigFree(YGConfigRef config);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGConfigSetUseWebDefaults(YGConfigRef config, bool enabled);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void YGConfigSetPointScaleFactor(YGConfigRef config, float pixelsInPoint);
    }

    #endregion
}
