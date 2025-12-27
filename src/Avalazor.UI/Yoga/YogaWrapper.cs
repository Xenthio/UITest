using System;
using System.Collections.Generic;

namespace Avalazor.UI.Yoga
{
    // Adapted from s&box: engine/Sandbox.Engine/Systems/UI/Yoga/YogaWrapper.cs
    // Managed wrapper around native Yoga node

    public class YogaWrapper : IDisposable
    {
        private YGNodeRef _node;
        private static YGConfigRef _defaultConfig;
        private List<YogaWrapper> _children = new List<YogaWrapper>();

        static YogaWrapper()
        {
            // Initialize default config with web defaults
            _defaultConfig = YogaNative.YGConfigNew();
            YogaNative.YGConfigSetUseWebDefaults(_defaultConfig, true);
            YogaNative.YGConfigSetPointScaleFactor(_defaultConfig, 0); // Manual pixel snapping
        }

        public YogaWrapper()
        {
            _node = YogaNative.YGNodeNewWithConfig(_defaultConfig);
        }

        public YGNodeRef NodeRef => _node;

        #region Layout Results

        public float LayoutX => YogaNative.YGNodeLayoutGetLeft(_node);
        public float LayoutY => YogaNative.YGNodeLayoutGetTop(_node);
        public float LayoutWidth => YogaNative.YGNodeLayoutGetWidth(_node);
        public float LayoutHeight => YogaNative.YGNodeLayoutGetHeight(_node);

        public float GetLayoutMargin(YGEdge edge) => YogaNative.YGNodeLayoutGetMargin(_node, edge);
        public float GetLayoutPadding(YGEdge edge) => YogaNative.YGNodeLayoutGetPadding(_node, edge);
        public float GetLayoutBorder(YGEdge edge) => YogaNative.YGNodeLayoutGetBorder(_node, edge);

        #endregion

        #region Children

        public void AddChild(YogaWrapper child, int index = -1)
        {
            if (index < 0 || index >= _children.Count)
            {
                _children.Add(child);
                YogaNative.YGNodeInsertChild(_node, child._node, (uint)_children.Count - 1);
            }
            else
            {
                _children.Insert(index, child);
                YogaNative.YGNodeInsertChild(_node, child._node, (uint)index);
            }
        }

        public void RemoveChild(YogaWrapper child)
        {
            _children.Remove(child);
            YogaNative.YGNodeRemoveChild(_node, child._node);
        }

        #endregion

        #region Layout Calculation

        public void CalculateLayout(float availableWidth = float.NaN, float availableHeight = float.NaN)
        {
            YogaNative.YGNodeCalculateLayout(_node, availableWidth, availableHeight, YGDirection.LTR);
        }

        #endregion

        #region Style Properties - Dimensions

        public void SetWidth(YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetWidth(_node, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetWidthPercent(_node, value.Value);
            else if (value.Unit == YGUnit.Auto)
                YogaNative.YGNodeStyleSetWidthAuto(_node);
        }

        public void SetHeight(YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetHeight(_node, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetHeightPercent(_node, value.Value);
            else if (value.Unit == YGUnit.Auto)
                YogaNative.YGNodeStyleSetHeightAuto(_node);
        }

        public void SetMinWidth(float value) => YogaNative.YGNodeStyleSetMinWidth(_node, value);
        public void SetMinHeight(float value) => YogaNative.YGNodeStyleSetMinHeight(_node, value);
        public void SetMaxWidth(float value) => YogaNative.YGNodeStyleSetMaxWidth(_node, value);
        public void SetMaxHeight(float value) => YogaNative.YGNodeStyleSetMaxHeight(_node, value);

        #endregion

        #region Style Properties - Flex

        public YGFlexDirection FlexDirection
        {
            set => YogaNative.YGNodeStyleSetFlexDirection(_node, value);
        }

        public float FlexGrow
        {
            set => YogaNative.YGNodeStyleSetFlexGrow(_node, value);
        }

        public float FlexShrink
        {
            set => YogaNative.YGNodeStyleSetFlexShrink(_node, value);
        }

        public void SetFlexBasis(YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetFlexBasis(_node, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetFlexBasisPercent(_node, value.Value);
            else if (value.Unit == YGUnit.Auto)
                YogaNative.YGNodeStyleSetFlexBasisAuto(_node);
        }

        public YGWrap FlexWrap
        {
            set => YogaNative.YGNodeStyleSetFlexWrap(_node, value);
        }

        #endregion

        #region Style Properties - Alignment

        public YGJustify JustifyContent
        {
            set => YogaNative.YGNodeStyleSetJustifyContent(_node, value);
        }

        public YGAlign AlignItems
        {
            set => YogaNative.YGNodeStyleSetAlignItems(_node, value);
        }

        public YGAlign AlignSelf
        {
            set => YogaNative.YGNodeStyleSetAlignSelf(_node, value);
        }

        public YGAlign AlignContent
        {
            set => YogaNative.YGNodeStyleSetAlignContent(_node, value);
        }

        #endregion

        #region Style Properties - Position

        public YGPositionType PositionType
        {
            set => YogaNative.YGNodeStyleSetPositionType(_node, value);
        }

        public void SetPosition(YGEdge edge, YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetPosition(_node, edge, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetPositionPercent(_node, edge, value.Value);
        }

        #endregion

        #region Style Properties - Spacing

        public void SetMargin(YGEdge edge, YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetMargin(_node, edge, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetMarginPercent(_node, edge, value.Value);
            else if (value.Unit == YGUnit.Auto)
                YogaNative.YGNodeStyleSetMarginAuto(_node, edge);
        }

        public void SetPadding(YGEdge edge, YGValue value)
        {
            if (value.Unit == YGUnit.Point)
                YogaNative.YGNodeStyleSetPadding(_node, edge, value.Value);
            else if (value.Unit == YGUnit.Percent)
                YogaNative.YGNodeStyleSetPaddingPercent(_node, edge, value.Value);
        }

        public void SetBorder(YGEdge edge, float value)
        {
            YogaNative.YGNodeStyleSetBorder(_node, edge, value);
        }

        #endregion

        #region Style Properties - Display

        public YGDisplay Display
        {
            set => YogaNative.YGNodeStyleSetDisplay(_node, value);
        }

        public YGOverflow Overflow
        {
            set => YogaNative.YGNodeStyleSetOverflow(_node, value);
        }

        #endregion

        #region Style Properties - Other

        public void SetGap(YGEdge gutter, float value)
        {
            YogaNative.YGNodeStyleSetGap(_node, gutter, value);
        }

        public float AspectRatio
        {
            set => YogaNative.YGNodeStyleSetAspectRatio(_node, value);
        }

        #endregion

        #region Measure Function

        public void SetMeasureFunc(YGMeasureFunc measureFunc)
        {
            YogaNative.YGNodeSetMeasureFunc(_node, measureFunc);
        }

        #endregion

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                if (!_node.IsNull)
                {
                    YogaNative.YGNodeFree(_node);
                    _node = YGNodeRef.Null;
                }
                _disposed = true;
            }
        }

        ~YogaWrapper()
        {
            Dispose();
        }

        #endregion
    }
}
