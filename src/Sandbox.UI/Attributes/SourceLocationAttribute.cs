namespace Sandbox.UI;

/// <summary>
/// Add source location to Razor elements for debugging.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SourceLocationAttribute : Attribute
{
    public string SourceFile { get; set; }
    public int SourceLine { get; set; }
    
    public SourceLocationAttribute(string sourceFile = "", int sourceLine = 0)
    {
        SourceFile = sourceFile;
        SourceLine = sourceLine;
    }
}
