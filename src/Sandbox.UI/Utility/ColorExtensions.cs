using System;

namespace Sandbox.UI;

/// <summary>
/// Extension methods for Color
/// </summary>
public static class ColorExtensions
{
	/// <summary>
	/// Linear interpolation between two colors
	/// </summary>
	public static Color Lerp(Color a, Color b, float t)
	{
		t = Math.Clamp(t, 0f, 1f);
		return new Color(
			(byte)MathX.Lerp(a.r, b.r, t),
			(byte)MathX.Lerp(a.g, b.g, t),
			(byte)MathX.Lerp(a.b, b.b, t),
			(byte)MathX.Lerp(a.a, b.a, t)
		);
	}
	
	/// <summary>
	/// Lerp extension method for convenience
	/// </summary>
	public static Color LerpTo(this Color from, Color to, float t)
	{
		return Lerp(from, to, t);
	}
}
