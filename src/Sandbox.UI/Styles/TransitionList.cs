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

	internal static TransitionList? ParseProperty(string property, string value, TransitionList? list)
	{
		var p = new Parse(value);

		list ??= new TransitionList();

		if (property == "transition")
		{
			while (!p.IsEnd)
			{
				p = p.SkipWhitespaceAndNewlines();

				var sub = p.ReadUntilOrEnd(",");
				p.Pointer++;

				var transition = Parse(sub);
				list.Add(transition);
			}

			return list;
		}

		Console.WriteLine($"Didn't handle transition style: {property}");
		return null;
	}

	static TransitionDesc Parse(string value)
	{
		var p = new Parse(value);

		var t = new TransitionDesc();
		t.Delay = 0;
		t.TimingFunction = "ease"; // default is ease

		p = p.SkipWhitespaceAndNewlines();
		t.Property = p.ReadWord(null, true).ToLower();
		if (p.IsEnd) return t;
		p = p.SkipWhitespaceAndNewlines();
		if (p.IsEnd) return t;

		//
		// Duration is mandatory
		//
		if (!p.TryReadTime(out var duration))
			throw new System.Exception("Expecting time in transition");

		t.Duration = duration;

		if (p.IsEnd) return t;
		p = p.SkipWhitespaceAndNewlines();
		if (p.IsEnd) return t;

		//
		// Try to read the delay now, since it could be here
		//
		if (p.TryReadTime(out var delay))
		{
			t.Delay = delay;
		}

		if (p.IsEnd) return t;
		p = p.SkipWhitespaceAndNewlines();
		if (p.IsEnd) return t;

		t.TimingFunction = p.ReadWord(null, true);

		if (p.IsEnd) return t;
		p = p.SkipWhitespaceAndNewlines();
		if (p.IsEnd) return t;

		if (p.TryReadTime(out delay))
		{
			t.Delay = delay;
		}

		return t;
	}
}
