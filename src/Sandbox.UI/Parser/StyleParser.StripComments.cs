using System.Text;

namespace Sandbox.UI;

internal static partial class StyleParser
{
	public static string StripComments( string v )
	{
		if ( string.IsNullOrWhiteSpace( v ) )
			return v;

		StringBuilder builder = new();
		var p = new Parse( v );

		int lastSafe = 0;

		bool commentCanFollow = true;

		while ( !p.IsEnd )
		{
			if ( commentCanFollow && p.Is( '/' ) )
			{
				p.Pointer++;

				if ( p.IsEnd )
					throw new System.Exception( "Parse error (file ends in /)" );

				if ( p.Is( '/' ) )
				{
					p.Pointer++;

					builder.Append( v.Substring( lastSafe, (p.Pointer - 2) - lastSafe ) );

					p = p.JumpToEndOfLine( false );

					lastSafe = Math.Min( p.Pointer, p.Length );
				}

				if ( p.Is( '*' ) )/* */
				{
					p.Pointer++;

					if ( p.IsEnd )
						throw new System.Exception( "Parse error (file ends in *)" );

					builder.Append( v.Substring( lastSafe, (p.Pointer - 2) - lastSafe ) );

					while ( true )
					{
						if ( p.IsEnd )
							throw new System.Exception( "Unterminated Multiline Comment" );

						if ( p.Is( '*' ) && p.Next == '/' )
						{
							p.Pointer += 2;
							lastSafe = Math.Min( p.Pointer, p.Length );
							break;
						}

						p.Pointer++;
					}
				}
			}

			// don't allow comments after :, because it might be a url
			commentCanFollow = !p.Is( ':' );

			p.Pointer++;
		}

		builder.Append( v.Substring( lastSafe, Math.Min( v.Length - lastSafe, p.Pointer - lastSafe ) ) );
		return builder.ToString();
	}
}
