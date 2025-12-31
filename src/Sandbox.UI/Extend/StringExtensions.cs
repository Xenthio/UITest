using System.Text.RegularExpressions;

namespace Sandbox.UI;

/// <summary>
/// String extension methods for white-space processing.
/// Based on s&box's StringExtensions from engine/Sandbox.System/Extend/StringExtensions.cs
/// </summary>
public static partial class StringExtensions
{
	[GeneratedRegex("\\s+")]
	private static partial Regex CollapseWhiteSpaceRegex();

	/// <summary>
	/// Collapse sequences of whitespace into a single whitespace.
	/// Used for WhiteSpace.Normal and WhiteSpace.NoWrap.
	/// </summary>
	public static string CollapseWhiteSpace(this string str)
	{
		if (string.IsNullOrEmpty(str)) return str;

		str = CollapseWhiteSpaceRegex().Replace(str, " ");
		str = str.Trim();

		return str;
	}

	[GeneratedRegex("[ \\t]+")]
	private static partial Regex CollapseSpacesAndTabsRegex();
	
	[GeneratedRegex("(?<=\\n|\\u2029)[ \\t]+|[ \\t]+(?=\\n|\\u2029)")]
	private static partial Regex RemoveSpacesAroundLineBreaksRegex();

	/// <summary>
	/// Collapse sequences of spaces and tabs into a single space, preserving newlines.
	/// Used for WhiteSpace.PreLine.
	/// </summary>
	public static string CollapseSpacesAndPreserveLines(this string str)
	{
		if (string.IsNullOrEmpty(str)) return str;

		str = CollapseSpacesAndTabsRegex().Replace(str, " ");
		str = RemoveSpacesAroundLineBreaksRegex().Replace(str, "");

		return str.Trim();
	}
}
