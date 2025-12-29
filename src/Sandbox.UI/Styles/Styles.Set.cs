namespace Sandbox.UI
{
	public partial class Styles
	{
		/// <summary>
		/// Override to handle CSS property aliases and shorthand properties
		/// Based on s&box's Styles.Set.cs
		/// </summary>
		public override bool Set( string property, string value )
		{
			// CSS standard: "color" is an alias for "font-color"
			if ( property == "color" )
				property = "font-color";

			// Handle border shorthand properties
			switch ( property )
			{
				case "border":
					return SetBorder( value, w => BorderWidth = w, c => BorderColor = c );

				case "border-left":
					return SetBorder( value, w => BorderLeftWidth = w, c => BorderLeftColor = c );

				case "border-right":
					return SetBorder( value, w => BorderRightWidth = w, c => BorderRightColor = c );

				case "border-top":
					return SetBorder( value, w => BorderTopWidth = w, c => BorderTopColor = c );

				case "border-bottom":
					return SetBorder( value, w => BorderBottomWidth = w, c => BorderBottomColor = c );

				case "border-color":
					{
						var borderColor = Color.Parse( value );
						BorderColor = borderColor;
						return borderColor.HasValue;
					}

				case "border-width":
					{
						var borderWidth = Length.Parse( value );
						BorderWidth = borderWidth;
						return borderWidth.HasValue;
					}
			}

			return base.Set( property, value );
		}

		/// <summary>
		/// Parse border shorthand syntax: "1px solid #808080" or "2px dashed red"
		/// Based on s&box's SetBorder implementation
		/// </summary>
		private bool SetBorder( string value, System.Action<Length?> setWidth, System.Action<Color?> setColor )
		{
			var parts = value.Split( new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries );

			foreach ( var part in parts )
			{
				// Try to parse as length (width)
				var lengthValue = Length.Parse( part );
				if ( lengthValue.HasValue )
				{
					setWidth( lengthValue );
					continue;
				}

				// Try to parse as color
				var colorValue = Color.Parse( part );
				if ( colorValue.HasValue )
				{
					setColor( colorValue );
					continue;
				}

				// Skip line style keywords (solid, dashed, dotted, etc.)
				// We don't support different border styles yet, so just ignore them
				if ( part.Equals( "solid", System.StringComparison.OrdinalIgnoreCase ) ||
				     part.Equals( "dashed", System.StringComparison.OrdinalIgnoreCase ) ||
				     part.Equals( "dotted", System.StringComparison.OrdinalIgnoreCase ) ||
				     part.Equals( "none", System.StringComparison.OrdinalIgnoreCase ) )
				{
					if ( part.Equals( "none", System.StringComparison.OrdinalIgnoreCase ) )
					{
						setWidth( Length.Pixels( 0 ) );
						return true;
					}
					continue;
				}

				// Unknown part, return false
				return false;
			}

			return true;
		}
	}
}
