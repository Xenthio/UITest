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

			// Handle border shorthand properties and other common CSS properties
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

				// Handle padding shorthand
				case "padding":
					return SetBoxModel( value, 
						t => PaddingTop = t,
						r => PaddingRight = r,
						b => PaddingBottom = b,
						l => PaddingLeft = l );

				// Handle margin shorthand
				case "margin":
					return SetBoxModel( value,
						t => MarginTop = t,
						r => MarginRight = r,
						b => MarginBottom = b,
						l => MarginLeft = l );

				// Handle alignment properties
				case "align-content":
					AlignContent = GetAlign( value );
					return AlignContent.HasValue;

				case "align-self":
					AlignSelf = GetAlign( value );
					return AlignSelf.HasValue;

				case "align-items":
					AlignItems = GetAlign( value );
					return AlignItems.HasValue;

				// Handle gap shorthand (row-gap column-gap)
				case "gap":
					return SetGap( value );

				// Handle flex shorthand
				case "flex":
					return SetFlex( value );
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

		/// <summary>
		/// Parse box model shorthand syntax (padding/margin): "5px" or "5px 10px" or "5px 10px 15px 20px"
		/// CSS standard: 1 value = all sides, 2 values = vertical horizontal, 
		///               3 values = top horizontal bottom, 4 values = top right bottom left
		/// </summary>
		private bool SetBoxModel( string value, 
			System.Action<Length?> setTop, 
			System.Action<Length?> setRight,
			System.Action<Length?> setBottom, 
			System.Action<Length?> setLeft )
		{
			var parts = value.Split( new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries );

			if ( parts.Length == 0 )
				return false;

			// Parse all parts as lengths
			var lengths = new System.Collections.Generic.List<Length?>();
			foreach ( var part in parts )
			{
				var lengthValue = Length.Parse( part );
				if ( !lengthValue.HasValue )
					return false; // Invalid value
				lengths.Add( lengthValue );
			}

			// Apply based on count
			switch ( lengths.Count )
			{
				case 1: // All sides
					setTop( lengths[0] );
					setRight( lengths[0] );
					setBottom( lengths[0] );
					setLeft( lengths[0] );
					return true;

				case 2: // Vertical, Horizontal
					setTop( lengths[0] );
					setBottom( lengths[0] );
					setRight( lengths[1] );
					setLeft( lengths[1] );
					return true;

				case 3: // Top, Horizontal, Bottom
					setTop( lengths[0] );
					setRight( lengths[1] );
					setLeft( lengths[1] );
					setBottom( lengths[2] );
					return true;

				case 4: // Top, Right, Bottom, Left
					setTop( lengths[0] );
					setRight( lengths[1] );
					setBottom( lengths[2] );
					setLeft( lengths[3] );
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Parse alignment values for align-items, align-self, align-content
		/// Based on s&box's GetAlign implementation
		/// </summary>
		private Align? GetAlign( string value )
		{
			switch ( value )
			{
				case "auto": return Align.Auto;
				case "flex-end": return Align.FlexEnd;
				case "flex-start": return Align.FlexStart;
				case "center": return Align.Center;
				case "stretch": return Align.Stretch;
				case "space-between": return Align.SpaceBetween;
				case "space-around": return Align.SpaceAround;
				case "space-evenly": return Align.SpaceEvenly;
				case "baseline": return Align.Baseline;
				default:
					return null;
			}
		}

		/// <summary>
		/// Parse gap shorthand: "10px" (both row and column) or "10px 20px" (row column)
		/// Based on s&box's SetGap implementation
		/// </summary>
		private bool SetGap( string value )
		{
			var parts = value.Split( new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries );

			if ( parts.Length == 0 )
				return false;

			var gap = Length.Parse( parts[0] );
			if ( !gap.HasValue )
				return false;

			RowGap = gap;
			ColumnGap = gap;

			if ( parts.Length > 1 )
			{
				var colGap = Length.Parse( parts[1] );
				if ( colGap.HasValue )
					ColumnGap = colGap;
			}

			return true;
		}

		/// <summary>
		/// Parse flex shorthand: "1" (flex-grow) or "1 0" (grow shrink) or "1 0 auto" (grow shrink basis)
		/// Based on s&box's SetFlex implementation
		/// </summary>
		private bool SetFlex( string value )
		{
			var parts = value.Split( new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries );

			if ( parts.Length == 0 )
				return false;

			// First value is flex-grow
			if ( float.TryParse( parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var grow ) )
			{
				FlexGrow = grow;
			}
			else
			{
				return false;
			}

			// Second value is flex-shrink
			if ( parts.Length > 1 )
			{
				if ( float.TryParse( parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var shrink ) )
				{
					FlexShrink = shrink;
				}
			}

			// Third value is flex-basis
			if ( parts.Length > 2 )
			{
				var basis = Length.Parse( parts[2] );
				if ( basis.HasValue )
					FlexBasis = basis;
			}

			return true;
		}
	}
}
