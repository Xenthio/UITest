using System.Collections.Immutable;
using System.Numerics;

namespace Sandbox.UI;

public partial class Styles
{
	/// <summary>
	/// Parse and generate gradients from CSS gradient functions
	/// (linear-gradient, radial-gradient, etc.)
	/// </summary>
	internal static class GradientGenerator
	{
		/// <summary>
		/// Parse a CSS gradient string and create a GradientInfo
		/// </summary>
		public static GradientInfo? ParseGradient(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;

			value = value.Trim();

			// Check for linear-gradient
			if (value.StartsWith("linear-gradient(", StringComparison.OrdinalIgnoreCase))
			{
				return ParseLinearGradient(value);
			}

			// Check for radial-gradient
			if (value.StartsWith("radial-gradient(", StringComparison.OrdinalIgnoreCase))
			{
				return ParseRadialGradient(value);
			}

			// Not a gradient
			return null;
		}

		/// <summary>
		/// Parse linear-gradient CSS function
		/// Format: linear-gradient([angle|direction], color-stop1, color-stop2, ...)
		/// </summary>
		private static GradientInfo? ParseLinearGradient(string value)
		{
			// Extract content between parentheses
			int start = value.IndexOf('(');
			int end = value.LastIndexOf(')');
			if (start < 0 || end < 0 || end <= start)
				return null;

			string content = value.Substring(start + 1, end - start - 1);
			
			var gradient = new GradientInfo
			{
				GradientType = GradientInfo.GradientTypes.Linear,
				Angle = 180f // Default: top to bottom
			};

			// Split by commas (simple parsing, doesn't handle rgba() with commas)
			var parts = SplitGradientParts(content);
			if (parts.Count < 2)
				return null;

			int colorStartIndex = 0;

			// Check if first part is angle or direction
			string firstPart = parts[0].Trim();
			if (TryParseAngle(firstPart, out float angle))
			{
				gradient.Angle = angle;
				colorStartIndex = 1;
			}
			else if (TryParseDirection(firstPart, out angle))
			{
				gradient.Angle = angle;
				colorStartIndex = 1;
			}

			// Parse color stops
			var colorOffsets = new List<GradientInfo.GradientColorOffset>();
			for (int i = colorStartIndex; i < parts.Count; i++)
			{
				if (TryParseColorStop(parts[i], out var colorOffset))
				{
					colorOffsets.Add(colorOffset);
				}
			}

			if (colorOffsets.Count < 2)
				return null;

			gradient.ColorOffsets = colorOffsets.ToImmutableArray();
			return gradient;
		}

		/// <summary>
		/// Parse radial-gradient CSS function
		/// </summary>
		private static GradientInfo? ParseRadialGradient(string value)
		{
			// Extract content between parentheses
			int start = value.IndexOf('(');
			int end = value.LastIndexOf(')');
			if (start < 0 || end < 0 || end <= start)
				return null;

			string content = value.Substring(start + 1, end - start - 1);

			var gradient = new GradientInfo
			{
				GradientType = GradientInfo.GradientTypes.Radial,
				SizeMode = GradientInfo.RadialSizeMode.FarthestCorner // Default
			};

			// Split by commas
			var parts = SplitGradientParts(content);
			if (parts.Count < 2)
				return null;

			// Parse color stops (simplified - skipping shape and size for now)
			var colorOffsets = new List<GradientInfo.GradientColorOffset>();
			foreach (var part in parts)
			{
				if (TryParseColorStop(part, out var colorOffset))
				{
					colorOffsets.Add(colorOffset);
				}
			}

			if (colorOffsets.Count < 2)
				return null;

			gradient.ColorOffsets = colorOffsets.ToImmutableArray();
			return gradient;
		}

		/// <summary>
		/// Split gradient parts by commas, handling nested functions like rgba()
		/// </summary>
		private static List<string> SplitGradientParts(string content)
		{
			var parts = new List<string>();
			int depth = 0;
			int start = 0;

			for (int i = 0; i < content.Length; i++)
			{
				char c = content[i];
				if (c == '(') depth++;
				else if (c == ')') depth--;
				else if (c == ',' && depth == 0)
				{
					parts.Add(content.Substring(start, i - start));
					start = i + 1;
				}
			}

			// Add last part
			if (start < content.Length)
			{
				parts.Add(content.Substring(start));
			}

			return parts;
		}

		/// <summary>
		/// Try to parse angle from string (e.g., "45deg", "0.5turn", "100grad")
		/// </summary>
		private static bool TryParseAngle(string value, out float angle)
		{
			angle = 0f;
			value = value.Trim().ToLowerInvariant();

			if (value.EndsWith("deg"))
			{
				if (float.TryParse(value.Substring(0, value.Length - 3), out angle))
					return true;
			}
			else if (value.EndsWith("grad"))
			{
				if (float.TryParse(value.Substring(0, value.Length - 4), out float grad))
				{
					angle = grad * 0.9f; // Convert gradians to degrees
					return true;
				}
			}
			else if (value.EndsWith("rad"))
			{
				if (float.TryParse(value.Substring(0, value.Length - 3), out float rad))
				{
					angle = rad * (180f / MathF.PI); // Convert radians to degrees
					return true;
				}
			}
			else if (value.EndsWith("turn"))
			{
				if (float.TryParse(value.Substring(0, value.Length - 4), out float turn))
				{
					angle = turn * 360f; // Convert turns to degrees
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Try to parse direction keywords (to top, to right, etc.)
		/// </summary>
		private static bool TryParseDirection(string value, out float angle)
		{
			angle = 0f;
			value = value.Trim().ToLowerInvariant();

			switch (value)
			{
				case "to top":
					angle = 0f;
					return true;
				case "to right":
					angle = 90f;
					return true;
				case "to bottom":
					angle = 180f;
					return true;
				case "to left":
					angle = 270f;
					return true;
				case "to top right":
					angle = 45f;
					return true;
				case "to bottom right":
					angle = 135f;
					return true;
				case "to bottom left":
					angle = 225f;
					return true;
				case "to top left":
					angle = 315f;
					return true;
			}

			return false;
		}

		/// <summary>
		/// Try to parse a color stop (e.g., "#ff0000 20%", "rgba(255,0,0,0.5)", "red 50px")
		/// </summary>
		private static bool TryParseColorStop(string value, out GradientInfo.GradientColorOffset colorOffset)
		{
			colorOffset = default;
			value = value.Trim();

			if (string.IsNullOrEmpty(value))
				return false;

			// Find position (percentage or length at the end)
			float? position = null;
			string colorPart = value;

			// Check for percentage
			int percentIndex = value.LastIndexOf('%');
			if (percentIndex > 0)
			{
				string posStr = value.Substring(0, percentIndex + 1).TrimEnd();
				int spaceIndex = posStr.LastIndexOf(' ');
				if (spaceIndex > 0)
				{
					string percentStr = posStr.Substring(spaceIndex + 1);
					if (percentStr.EndsWith("%") && float.TryParse(percentStr.Substring(0, percentStr.Length - 1), out float percent))
					{
						position = percent / 100f;
						colorPart = value.Substring(0, spaceIndex).Trim();
					}
				}
			}

			// Try to parse color
			if (TryParseColor(colorPart, out var color))
			{
				colorOffset = new GradientInfo.GradientColorOffset
				{
					color = color,
					offset = position
				};
				return true;
			}

			return false;
		}

		/// <summary>
		/// Try to parse a CSS color value
		/// </summary>
		private static bool TryParseColor(string value, out Color color)
		{
			color = Color.White;
			value = value.Trim().ToLowerInvariant();

			// Handle rgba/rgb
			if (value.StartsWith("rgba(") || value.StartsWith("rgb("))
			{
				return TryParseRgba(value, out color);
			}

			// Handle hex colors
			if (value.StartsWith("#"))
			{
				return TryParseHex(value, out color);
			}

			// Handle named colors
			return TryParseNamedColor(value, out color);
		}

		private static bool TryParseRgba(string value, out Color color)
		{
			color = Color.White;

			int start = value.IndexOf('(');
			int end = value.IndexOf(')');
			if (start < 0 || end < 0)
				return false;

			string[] parts = value.Substring(start + 1, end - start - 1).Split(',');
			if (parts.Length < 3)
				return false;

			if (!byte.TryParse(parts[0].Trim(), out byte r))
				return false;
			if (!byte.TryParse(parts[1].Trim(), out byte g))
				return false;
			if (!byte.TryParse(parts[2].Trim(), out byte b))
				return false;

			byte a = 255;
			if (parts.Length > 3)
			{
				if (float.TryParse(parts[3].Trim(), out float alpha))
				{
					a = (byte)(alpha * 255);
				}
			}

			color = new Color(r, g, b, a);
			return true;
		}

		private static bool TryParseHex(string value, out Color color)
		{
			color = Color.White;
			value = value.TrimStart('#');

			if (value.Length == 6) // RGB
			{
				if (byte.TryParse(value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
				    byte.TryParse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
				    byte.TryParse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
				{
					color = new Color(r, g, b);
					return true;
				}
			}
			else if (value.Length == 8) // RGBA
			{
				if (byte.TryParse(value.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out byte r) &&
				    byte.TryParse(value.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out byte g) &&
				    byte.TryParse(value.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b) &&
				    byte.TryParse(value.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out byte a))
				{
					color = new Color(r, g, b, a);
					return true;
				}
			}

			return false;
		}

		private static bool TryParseNamedColor(string value, out Color color)
		{
			color = Color.White;

			// Basic named colors
			switch (value)
			{
				case "red": color = new Color(255, 0, 0); return true;
				case "green": color = new Color(0, 128, 0); return true;
				case "blue": color = new Color(0, 0, 255); return true;
				case "white": color = new Color(255, 255, 255); return true;
				case "black": color = new Color(0, 0, 0); return true;
				case "transparent": color = new Color(0, 0, 0, 0); return true;
				// Add more as needed
			}

			return false;
		}
	}
}
