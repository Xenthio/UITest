using System;

namespace Sandbox.UI;

/// <summary>
/// Extension methods for Length
/// </summary>
public static class LengthExtensions
{
	/// <summary>
	/// Check if a length unit is dynamic (needs recalculation)
	/// </summary>
	public static bool IsDynamic(this LengthUnit unit) => unit is
		LengthUnit.ViewWidth or LengthUnit.ViewHeight or
		LengthUnit.ViewMin or LengthUnit.ViewMax or
		LengthUnit.Em or LengthUnit.RootEm or LengthUnit.Expression;
		
	/// <summary>
	/// Linear interpolation extension method
	/// </summary>
	public static Length LerpTo(this Length from, Length to, float t, bool clamp = true)
	{
		if (from.Unit != to.Unit)
			return to;
			
		if (clamp)
			t = Math.Clamp(t, 0f, 1f);
			
		return new Length(from.Value + (to.Value - from.Value) * t, from.Unit);
	}
}
