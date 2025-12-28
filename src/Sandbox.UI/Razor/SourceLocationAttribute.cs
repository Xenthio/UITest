namespace Sandbox.UI;

/// <summary>
/// Automatically added to code-generated classes to let them determine their location.
/// This helps when looking for resources relative to them, like style sheets.
/// Based on s&box's SourceLocationAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class SourceLocationAttribute : Attribute
{
    /// <summary>
    /// The file path where the class is defined
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// The line number in the source file
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Create a SourceLocationAttribute with the specified path and line
    /// </summary>
    public SourceLocationAttribute(string filePath, int line = 0)
    {
        FilePath = filePath;
        Line = line;
    }
}
