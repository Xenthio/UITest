namespace Sandbox.UI;

/// <summary>
/// Stub class for CSS transitions support.
/// Full implementation would track transition properties, durations, easing functions, etc.
/// </summary>
public class TransitionList
{
	public List<object> List { get; set; } = new List<object>();
}

/// <summary>
/// Stub class for transition descriptors
/// </summary>
public class TransitionDesc
{
	public static TransitionList? ParseProperty(string property, string value, TransitionList? existing)
	{
		// Stub implementation - transitions not fully supported yet
		return existing ?? new TransitionList();
	}
}
