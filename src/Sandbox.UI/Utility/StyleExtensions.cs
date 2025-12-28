using System.Text.RegularExpressions;

namespace Sandbox.UI;

/// <summary>
/// Extension methods for common operations used in the UI framework.
/// </summary>
internal static class StyleExtensions
{
	/// <summary>
	/// Puts a filename into the format /path/filename.ext (from path\FileName.EXT)
	/// </summary>
	public static string NormalizeFilename( this string str, bool enforceInitialSlash = true ) => NormalizeFilename( str, enforceInitialSlash, true );

	/// <summary>
	/// Puts a filename into the format /path/filename.ext (from path\FileName.EXT)
	/// </summary>
	public static string NormalizeFilename( this string str, bool enforceInitialSlash, bool enforceLowerCase )
	{
		if ( enforceLowerCase )
		{
			str = str.ToLowerInvariant();
		}

		str = str.Replace( '\\', '/' );

		if ( enforceInitialSlash && !str.StartsWith( '/' ) )
			str = str.Insert( 0, "/" );

		return str;
	}

	/// <summary>
	/// Match a string against a wildcard pattern (supports * wildcard)
	/// </summary>
	public static bool WildcardMatch( this string str, string wildcard )
	{
		if ( str == null ) return false;
		if ( wildcard == null ) return false;

		if ( wildcard.Contains( '*' ) )
		{
			wildcard = Regex.Escape( wildcard ).Replace( "\\*", ".*" );
			wildcard = $"^{wildcard}$";
			return Regex.IsMatch( str, wildcard, RegexOptions.IgnoreCase );
		}

		return string.Equals( str, wildcard, StringComparison.OrdinalIgnoreCase );
	}

	/// <summary>
	/// Count the number of bits set in an integer
	/// </summary>
	public static int BitsSet( this int i )
	{
		i = i - ((i >> 1) & 0x55555555);        // add pairs of bits
		i = (i & 0x33333333) + ((i >> 2) & 0x33333333);  // quads
		i = (i + (i >> 4)) & 0x0F0F0F0F;        // groups of 8
		return (i * 0x01010101) >> 24;          // horizontal sum of bytes
	}

	/// <summary>
	/// Check if a PseudoClass contains a specific flag
	/// </summary>
	public static bool Contains( this PseudoClass pseudoClass, PseudoClass flag )
	{
		return (pseudoClass & flag) == flag;
	}
}
