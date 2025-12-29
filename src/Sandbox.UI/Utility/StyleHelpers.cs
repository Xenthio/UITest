using System;

namespace Sandbox.UI;

/// <summary>
/// Helper utilities for style parsing and manipulation
/// </summary>
public static class StyleHelpers
{
	/// <summary>
	/// Tries to parse a CSS length value (e.g., "10px", "50%", "2em")
	/// </summary>
	public static bool TryParseLength(string value, out Length result)
	{
		result = default;
		
		if (string.IsNullOrWhiteSpace(value))
			return false;
			
		value = value.Trim();
		
		// Try to parse as Length
		if (Length.TryParse(value, out var length))
		{
			result = length;
			return true;
		}
		
		return false;
	}
	
	/// <summary>
	/// Tries to parse a number from a string
	/// </summary>
	public static bool TryParseFloat(string value, out float result)
	{
		return float.TryParse(value, System.Globalization.NumberStyles.Float, 
			System.Globalization.CultureInfo.InvariantCulture, out result);
	}
	
	/// <summary>
	/// Parse rotation value (handles deg, rad, grad, turn units)
	/// </summary>
	public static float RotationDegrees(string value)
	{
		value = value.Trim().ToLowerInvariant();
		
		if (value.EndsWith("deg"))
		{
			if (float.TryParse(value.Substring(0, value.Length - 3), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out var deg))
				return deg;
		}
		else if (value.EndsWith("rad"))
		{
			if (float.TryParse(value.Substring(0, value.Length - 3), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out var rad))
				return rad * 180f / MathF.PI;
		}
		else if (value.EndsWith("grad"))
		{
			if (float.TryParse(value.Substring(0, value.Length - 4), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out var grad))
				return grad * 0.9f;
		}
		else if (value.EndsWith("turn"))
		{
			if (float.TryParse(value.Substring(0, value.Length - 4), System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out var turn))
				return turn * 360f;
		}
		else if (float.TryParse(value, System.Globalization.NumberStyles.Float,
			System.Globalization.CultureInfo.InvariantCulture, out var num))
		{
			return num;
		}
		
		return 0f;
	}
	
	/// <summary>
	/// Parse rotation value from numeric value and unit string (handles deg, rad, grad, turn units)
	/// </summary>
	public static float RotationDegrees(float value, string unit)
	{
		if (string.IsNullOrWhiteSpace(unit))
			return value;
			
		unit = unit.Trim().ToLowerInvariant();
		
		return unit switch
		{
			"deg" => value,
			"rad" => value * 180f / MathF.PI,
			"grad" => value * 0.9f,
			"turn" => value * 360f,
			_ => value // assume degrees if no unit or unknown unit
		};
	}
	
	/// <summary>
	/// Trim quotes from a string value
	/// </summary>
	public static string TrimQuoted(this string value, bool alsotrim = true)
	{
		if (string.IsNullOrEmpty(value))
			return value;
			
		if (alsotrim)
			value = value.Trim();
		
		if (value.Length >= 2)
		{
			if ((value[0] == '"' && value[^1] == '"') || 
			    (value[0] == '\'' && value[^1] == '\''))
			{
				return value.Substring(1, value.Length - 2);
			}
		}
		
		return value;
	}
}
