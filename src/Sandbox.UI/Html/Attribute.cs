using System;
using System.Diagnostics;
using System.Web;

namespace Sandbox.Html
{
	/// <summary>
	/// Represents an HTML attribute.
	/// </summary>
	[DebuggerDisplay( "Name: {Name}, Value: {Value}" )]
	public class Attribute
	{
		internal string _name;
		internal int _namelength;
		internal int _namestartindex;
		internal string _value;
		internal int _valuelength;
		internal int _valuestartindex;

		private string _text;

		internal Attribute( Document ownerdocument )
		{
			_text = ownerdocument.Text;
		}

		/// <summary>
		/// Gets the qualified name of the attribute.
		/// </summary>
		public string Name => _name ?? (_name = _text.Substring( _namestartindex, _namelength ).ToLowerInvariant());

		/// <summary>
		/// Gets or sets the value of the attribute.
		/// </summary>
		public string Value => _value ??= GetValue();


		private string GetValue()
		{
			if ( _valuestartindex <= 0 ) return string.Empty;
			if ( _valuelength <= 0 ) return string.Empty;

			return HttpUtility.HtmlDecode( _text.Substring( _valuestartindex, _valuelength ) );
		}
	}
}
