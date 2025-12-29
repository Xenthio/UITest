using System.Globalization;

namespace Sandbox.UI
{
	public abstract partial class BaseStyles
	{
		static float? ParseFloat( string value )
		{
			if ( float.TryParse( value, CultureInfo.InvariantCulture, out var result ) )
				return result;

			return null;
		}

		static int? ParseInt( string value )
		{
			if ( int.TryParse( value, CultureInfo.InvariantCulture, out var result ) )
				return result;

			return null;
		}

		static float? ParseSeconds( string value )
		{
			if ( value.EndsWith( 's' ) )
			{
				return ParseFloat( value.Substring( 0, value.Length - 1 ) );
			}

			return default;
		}

		static float? ParseAspectRatio( string value )
		{
			var vals = value.Split( new[] { ' ', ':', '/' }, StringSplitOptions.RemoveEmptyEntries );
			if ( vals.Length == 1 )
			{
				return ParseFloat( value );
			}
			return ParseFloat( vals[0] ) / ParseFloat( vals[1] );
		}

		/// <summary>
		/// Whether there is an active CSS animation.
		/// </summary>
		public bool HasAnimation
		{
			get
			{
				if ( _animationname is null ) return false;
				if ( _animationname.Length == 0 ) return false;
				if ( _animationname == "none" ) return false;
				if ( string.IsNullOrWhiteSpace( _animationname ) ) return false;

				return true;
			}
		}

		protected void Lerp( ref float? o, in float? a, in float? b, in float? defaultValue, float delta )
		{
			if ( !a.HasValue && !b.HasValue )
				return;

			float from = a ?? defaultValue.Value;
			float to = b ?? defaultValue.Value;

			if ( from == to )
			{
				o = from;
				return;
			}

			o = from.LerpTo( to, delta );
		}

		protected void Lerp( ref PanelTransform? o, in PanelTransform? a, in PanelTransform? b, in PanelTransform? defaultValue, float delta )
		{
			if ( !a.HasValue && !b.HasValue )
				return;

			var from = a ?? defaultValue.Value;
			var to = b ?? defaultValue.Value;

			if ( from == to )
			{
				o = from;
				return;
			}

			o = PanelTransform.Lerp( from, to, delta );
		}

		protected void Lerp( ref Color? o, in Color? a, in Color? b, in Color? defaultValue, float delta )
		{
			if ( !a.HasValue && !b.HasValue )
				return;

			var from = a ?? defaultValue ?? default;
			var to = b ?? defaultValue ?? default;

			if ( from == to )
			{
				o = from;
				return;
			}

			o = Color.Lerp( from, to, delta );
		}

		protected void Lerp( ref Length? o, in Length? a, in Length? b, in Length? defaultValue, float delta )
		{
			if ( !a.HasValue && !b.HasValue )
				return;

			var from = a ?? defaultValue.Value;
			var to = b ?? defaultValue.Value;

			if ( from == to )
			{
				o = from;
				return;
			}

			o = Length.Lerp( from, to, delta );
		}

		protected void Lerp( ref int? o, in int? a, in int? b, in int? defaultValue, float delta )
		{
			if ( !a.HasValue && !b.HasValue )
				return;

			var from = a ?? defaultValue.Value;
			var to = b ?? defaultValue.Value;

			if ( from == to )
			{
				o = from;
				return;
			}

			o = (int)MathX.Lerp( from, to, delta );
		}
	}
}
