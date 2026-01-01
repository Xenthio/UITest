namespace Sandbox.UI;

/// <summary>
/// Stub class for CSS transitions support.
/// Full implementation would track transition properties, durations, easing functions, etc.
/// </summary>
public class TransitionList
{
	public List<TransitionDescriptor> List { get; set; } = new List<TransitionDescriptor>();
}

/// <summary>
/// Describes a single CSS transition property.
/// </summary>
public class TransitionDescriptor
{
	/// <summary>
	/// The CSS property name being transitioned
	/// </summary>
	public string Property { get; set; } = "";
	
	/// <summary>
	/// Duration in milliseconds
	/// </summary>
	public float? Duration { get; set; }
	
	/// <summary>
	/// Delay in milliseconds
	/// </summary>
	public float? Delay { get; set; }
	
	/// <summary>
	/// Timing function name (ease, linear, ease-in, ease-out, etc.)
	/// </summary>
	public string? TimingFunction { get; set; }
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
