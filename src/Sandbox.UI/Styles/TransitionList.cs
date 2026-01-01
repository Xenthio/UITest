namespace Sandbox.UI;

/// <summary>
/// A list of CSS properties that should transition when changed.
///
/// Utility to create a transition by comparing the
/// panel style before and after the scope.
/// </summary>
public class TransitionList
{
	/// <summary>
	/// The actual list of CSS properties that should be transitioned.
	/// </summary>
	public List<TransitionDesc> List = new List<TransitionDesc>();

	internal void AddTransitions( TransitionList transitions )
	{
		foreach ( var t in transitions.List )
		{
			Add( t );
		}
	}

	internal void Add( TransitionDesc t )
	{
		var n = List.FirstOrDefault( x => x.Property == t.Property );

		if ( t.Delay.HasValue ) n.Delay = t.Delay;
		if ( t.TimingFunction != null ) n.TimingFunction = t.TimingFunction;
		if ( t.Property != null ) n.Property = t.Property;
		if ( t.Duration.HasValue ) n.Duration = t.Duration;

		List.RemoveAll( x => x.Property == t.Property );
		List.Add( n );
	}

	/// <summary>
	/// Clear the list of CSS transitions.
	/// </summary>
	public void Clear()
	{
		List.Clear();
	}
}

/// <summary>
/// Describes a single CSS transition property.
/// </summary>
public struct TransitionDesc
{
	/// <summary>
	/// The CSS property name being transitioned
	/// </summary>
	public string? Property { get; set; }
	
	/// <summary>
	/// Duration in seconds
	/// </summary>
	public float? Duration { get; set; }
	
	/// <summary>
	/// Delay in seconds
	/// </summary>
	public float? Delay { get; set; }
	
	/// <summary>
	/// Timing function name (ease, linear, ease-in, ease-out, etc.)
	/// </summary>
	public string? TimingFunction { get; set; }

	public static TransitionList? ParseProperty(string property, string value, TransitionList? existing)
	{
		// Stub implementation - transitions not fully supported yet
		return existing ?? new TransitionList();
	}
}
