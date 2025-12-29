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
