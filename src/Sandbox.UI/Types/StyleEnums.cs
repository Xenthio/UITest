namespace Sandbox.UI;

/// <summary>
/// Pseudo-class flags for styling (hover, active, etc.)
/// Matches s&box's PseudoClass
/// </summary>
[Flags]
public enum PseudoClass
{
    None = 0,
    Hover = 1 << 0,
    Active = 1 << 1,
    Focus = 1 << 2,
    FirstChild = 1 << 3,
    LastChild = 1 << 4,
    Empty = 1 << 5,
    Intro = 1 << 6,
    Outro = 1 << 7,
    Checked = 1 << 8,
    Disabled = 1 << 9,
    OnlyChild = 1 << 10,
    Before = 1 << 11,
    After = 1 << 12,
}

/// <summary>
/// CSS display mode
/// </summary>
public enum DisplayMode
{
    Flex,
    None,
    Contents,
}

/// <summary>
/// CSS flex direction
/// </summary>
public enum FlexDirection
{
    Row,
    Column,
    RowReverse,
    ColumnReverse
}

/// <summary>
/// CSS justify-content
/// </summary>
public enum Justify
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

/// <summary>
/// CSS align-items / align-self / align-content
/// </summary>
public enum Align
{
    Auto,
    FlexStart,
    FlexEnd,
    Center,
    Stretch,
    Baseline,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly,
}

/// <summary>
/// CSS flex-wrap
/// </summary>
public enum Wrap
{
    NoWrap,
    Wrap,
    WrapReverse
}

/// <summary>
/// CSS position type
/// </summary>
public enum PositionMode
{
    Static,
    Relative,
    Absolute
}

/// <summary>
/// CSS overflow
/// </summary>
public enum OverflowMode
{
    Visible,
    Hidden,
    Scroll
}

/// <summary>
/// CSS pointer-events
/// </summary>
public enum PointerEvents
{
    All,
    None
}

/// <summary>
/// CSS text-align
/// </summary>
public enum TextAlign
{
    Left,
    Center,
    Right,
    Justify
}

/// <summary>
/// CSS word-wrap / overflow-wrap
/// </summary>
public enum WordWrap
{
    Normal,
    NoWrap,
    BreakWord
}

/// <summary>
/// CSS object-fit for images
/// </summary>
public enum ObjectFit
{
    None,
    Contain,
    Cover,
    Fill,
    ScaleDown
}
