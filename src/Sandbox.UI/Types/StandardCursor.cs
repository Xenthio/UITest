namespace Sandbox.UI;

/// <summary>
/// Standard cursor types for CSS cursor property.
/// Matches s&box's InputStandardCursor_t enum.
/// </summary>
public enum StandardCursor
{
    /// <summary>
    /// Default arrow cursor
    /// </summary>
    Arrow = 0,

    /// <summary>
    /// Text selection I-beam cursor
    /// </summary>
    IBeam,

    /// <summary>
    /// Hourglass/waiting cursor
    /// </summary>
    HourGlass,

    /// <summary>
    /// Crosshair cursor
    /// </summary>
    Crosshair,

    /// <summary>
    /// Arrow with hourglass
    /// </summary>
    WaitArrow,

    /// <summary>
    /// Up arrow cursor
    /// </summary>
    Up,

    /// <summary>
    /// Northwest-southeast resize cursor
    /// </summary>
    SizeNWSE,

    /// <summary>
    /// Northeast-southwest resize cursor
    /// </summary>
    SizeNESW,

    /// <summary>
    /// West-east resize cursor
    /// </summary>
    SizeWE,

    /// <summary>
    /// North-south resize cursor
    /// </summary>
    SizeNS,

    /// <summary>
    /// All directions resize cursor
    /// </summary>
    SizeALL,

    /// <summary>
    /// Not allowed/prohibited cursor
    /// </summary>
    No,

    /// <summary>
    /// Pointing hand cursor
    /// </summary>
    Hand,

    /// <summary>
    /// Closed/grabbing hand cursor
    /// </summary>
    HandClosed,
}
