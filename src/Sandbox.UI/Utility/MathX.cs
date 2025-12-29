using System;
using System.Numerics;

namespace Sandbox.UI;

/// <summary>
/// Extended math utilities
/// </summary>
public static class MathX
{
	/// <summary>
	/// Linear interpolation between two values
	/// </summary>
	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * Math.Clamp(t, 0f, 1f);
	}
	
	/// <summary>
	/// Linear interpolation extension method
	/// </summary>
	public static float LerpTo(this float from, float to, float t, bool clamp = true)
	{
		if (clamp)
			t = Math.Clamp(t, 0f, 1f);
		return from + (to - from) * t;
	}
	
	/// <summary>
	/// Linear interpolation between two Vector3 values
	/// </summary>
	public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		return Vector3.Lerp(a, b, Math.Clamp(t, 0f, 1f));
	}
	
	/// <summary>
	/// Clamp a value between min and max
	/// </summary>
	public static float Clamp(float value, float min, float max)
	{
		return Math.Clamp(value, min, max);
	}
	
	/// <summary>
	/// Convert degrees to radians
	/// </summary>
	public static float DegToRad(float degrees)
	{
		return degrees * (MathF.PI / 180f);
	}
	
	/// <summary>
	/// Convert radians to degrees
	/// </summary>
	public static float RadToDeg(float radians)
	{
		return radians * (180f / MathF.PI);
	}
}
