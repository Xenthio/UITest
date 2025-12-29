namespace Sandbox;

/// <summary>
/// Provides alternative names (aliases) for a class when used in Razor templates.
/// For example, [Alias("check")] allows both the Library name and "check" to create instances.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute : Attribute
{
	/// <summary>
	/// The aliases for this class.
	/// </summary>
	public string[] Value { get; private set; }

	public AliasAttribute(params string[] aliases)
	{
		Value = aliases;
	}
}
