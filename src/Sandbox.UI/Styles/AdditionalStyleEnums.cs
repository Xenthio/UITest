namespace Sandbox.UI;

/// <summary>
/// Possible values for <c>text-overflow</c> CSS property.
/// </summary>
public enum TextOverflow
{
None = 0,
Ellipsis = 1,
Clip = 2
}

/// <summary>
/// Possible values for <c>word-break</c> CSS property.
/// </summary>
public enum WordBreak
{
Normal,
BreakAll
}

/// <summary>
/// Possible values for <c>text-transform</c> CSS property.
/// </summary>
public enum TextTransform
{
None = 0,
Capitalize = 1,
Uppercase = 2,
Lowercase = 3
}

/// <summary>
/// Possible values for <c>text-decoration-skip-ink</c> CSS property.
/// </summary>
public enum TextSkipInk
{
All = 0,
None = 1,
}

/// <summary>
/// Possible values for <c>text-decoration-style</c> CSS property.
/// </summary>
public enum TextDecorationStyle
{
Solid = 0,
Double = 1,
Dotted = 2,
Dashed = 3,
Wavy = 4
}

/// <summary>
/// Possible values for <c>text-decoration</c> CSS property.
/// </summary>
[Flags]
public enum TextDecoration
{
None = 0,
Underline = 1,
Overline = 2,
LineThrough = 4
}

/// <summary>
/// Possible values for <c>font-style</c> CSS property.
/// </summary>
public enum FontStyle
{
None = 0,
Normal = 1,
Italic = 2,
Oblique = 3
}

/// <summary>
/// Possible values for <c>white-space</c> CSS property.
/// </summary>
public enum WhiteSpace
{
Normal = 0,
NoWrap = 1,
Pre = 2,
PreWrap = 3,
PreLine = 4
}

/// <summary>
/// Possible values for <c>mask-mode</c> CSS property.
/// </summary>
public enum MaskMode
{
Alpha = 0,
Luminance = 1,
MatchSource = 2
}

/// <summary>
/// Possible values for <c>mask-scope</c> CSS property.
/// </summary>
public enum MaskScope
{
Default = 0,
Panel = 1,
Parent = 2
}

/// <summary>
/// Possible values for <c>background-repeat</c> and <c>mask-repeat</c> CSS properties.
/// </summary>
public enum BackgroundRepeat
{
Repeat = 0,
RepeatX = 1,
RepeatY = 2,
NoRepeat = 3,
Space = 4,
Round = 5
}

/// <summary>
/// Possible values for <c>border-image-fill</c> CSS property.
/// </summary>
public enum BorderImageFill
{
None = 0,
Fill = 1,
Unfilled = 2
}

/// <summary>
/// Possible values for <c>border-image-repeat</c> CSS property.
/// </summary>
public enum BorderImageRepeat
{
Stretch = 0,
Repeat = 1,
Round = 2,
Space = 3
}

/// <summary>
/// Possible values for <c>image-rendering</c> CSS property.
/// </summary>
public enum ImageRendering
{
Auto = 0,
Smooth = 1,
HighQuality = 2,
CrispEdges = 3,
Pixelated = 4,
Anisotropic = 5
}

/// <summary>
/// Possible values for <c>font-smooth</c> CSS property.
/// </summary>
public enum FontSmooth
{
Auto = 0,
None = 1,
Antialiased = 2,
SubpixelAntialiased = 3
}

/// <summary>
/// Possible values for <c>mix-blend-mode</c> CSS property.
/// </summary>
public enum MixBlendMode
{
Normal = 0,
Multiply = 1,
Screen = 2,
Overlay = 3,
Darken = 4,
Lighten = 5,
ColorDodge = 6,
ColorBurn = 7,
HardLight = 8,
SoftLight = 9,
Difference = 10,
Exclusion = 11,
Hue = 12,
Saturation = 13,
Color = 14,
Luminosity = 15
}

/// <summary>
/// Stub Texture class for compilation - will be replaced with proper implementation
/// </summary>
public class Texture
{
public string Path { get; set; } = string.Empty;
public int Width { get; set; }
public int Height { get; set; }

public static Texture Load(string path)
{
return new Texture { Path = path };
}
}

/// <summary>
/// Filter modes namespace for Rendering.FilterMode
/// </summary>
public static class RenderingFilters
{
/// <summary>
/// Filter modes for texture/text rendering
/// </summary>
public enum FilterMode
{
None = 0,
Bilinear = 1,
Trilinear = 2,
Point = 3
}
}
