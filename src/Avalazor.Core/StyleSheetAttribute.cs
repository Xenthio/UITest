using System;

namespace Avalazor.UI;

/// <summary>
/// Attribute to automatically apply a stylesheet to a component.
/// Based on s&box StyleSheetAttribute (MIT licensed)
/// Source: https://github.com/Facepunch/sbox-public/blob/master/engine/Sandbox.System/UI/StyleSheetAttribute.cs
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class StyleSheetAttribute : Attribute
{
    /// <summary>
    /// File name of the style sheet file (can be .css or .scss)
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Create a StyleSheet attribute with a specific file name
    /// </summary>
    /// <param name="name">Path to the stylesheet file</param>
    public StyleSheetAttribute(string? name = null)
    {
        Name = name;
    }
}
