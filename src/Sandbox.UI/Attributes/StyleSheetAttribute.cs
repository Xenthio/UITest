namespace Sandbox.UI;

/// <summary>
/// Will automatically apply the named stylesheet to the Panel.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class StyleSheetAttribute : Attribute
{
    /// <summary>
    /// File name of the style sheet file.
    /// </summary>
    public string Name;

    public StyleSheetAttribute(string? name = null)
    {
        Name = name ?? "";
    }
}
