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
				case "transition":
				case "transition-delay":
				case "transition-duration":
				case "transition-property":
				case "transition-timing-function":
					Transitions = TransitionDesc.ParseProperty( property, value, Transitions );
					return true;

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

				// Handle font-smooth with never/always aliases
				case "font-smooth":
					return SetFontSmooth( value );

				// S&box-specific cases that need custom method handlers
				case "flex-direction":
					return SetFlexDirection( value );

				case "border-radius":
					return SetBorderRadius( value );

				case "justify-content":
					return SetJustifyContent( value );

				case "flex-wrap":
					return SetFlexWrap( value );

				case "display":
					return SetDisplay( value );

				case "pointer-events":
					return SetPointerEvents( value );

				case "position":
					return SetPosition( value );

				case "text-align":
					return SetTextAlign( value );

				case "text-overflow":
					return SetTextOverflow( value );

				case "word-break":
					return SetWordBreak( value );

				case "white-space":
					return SetWhiteSpace( value );

				case "object-fit":
					return SetObjectFit( value );
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
			var lengths = parts.Select( part => Length.Parse( part ) ).ToList();

			if ( lengths.Any( lengthValue => !lengthValue.HasValue ) )
				return false; // Invalid value

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
			if ( parts.Length > 1 && float.TryParse( parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var shrink ) )
			{
				FlexShrink = shrink;
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

		/// <summary>
		/// Parse font-smooth values including never/always aliases
		/// Based on s&box's font-smooth handling
		/// </summary>
		private bool SetFontSmooth( string value )
		{
			switch ( value )
			{
				case "never":
				case "none":
					FontSmooth = UI.FontSmooth.None;
					return true;

				case "always":
				case "antialiased":
					FontSmooth = UI.FontSmooth.Antialiased;
					return true;

				case "subpixel-antialiased":
					FontSmooth = UI.FontSmooth.SubpixelAntialiased;
					return true;

				case "auto":
					FontSmooth = UI.FontSmooth.Auto;
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Parse flex-direction values
		/// Based on s&box's SetFlexDirection implementation
		/// </summary>
		private bool SetFlexDirection( string value )
		{
			switch ( value )
			{
				case "column":
					FlexDirection = UI.FlexDirection.Column;
					return true;
				case "column-reverse":
					FlexDirection = UI.FlexDirection.ColumnReverse;
					return true;
				case "row":
					FlexDirection = UI.FlexDirection.Row;
					return true;
				case "row-reverse":
					FlexDirection = UI.FlexDirection.RowReverse;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse border-radius shorthand
		/// Based on s&box's SetBorderRadius implementation
		/// </summary>
		private bool SetBorderRadius( string value )
		{
			var parts = value.Split( new[] { ' ', ',' }, System.StringSplitOptions.RemoveEmptyEntries );

			if ( parts.Length == 0 )
				return false;

			var lengths = parts.Select( part => Length.Parse( part ) ).ToList();

			if ( lengths.Any( l => !l.HasValue ) )
				return false;

			switch ( lengths.Count )
			{
				case 1: // All corners
					BorderTopLeftRadius = lengths[0];
					BorderTopRightRadius = lengths[0];
					BorderBottomRightRadius = lengths[0];
					BorderBottomLeftRadius = lengths[0];
					return true;

				case 2: // Top-left/bottom-right, top-right/bottom-left
					BorderTopLeftRadius = lengths[0];
					BorderTopRightRadius = lengths[1];
					BorderBottomRightRadius = lengths[0];
					BorderBottomLeftRadius = lengths[1];
					return true;

				case 3: // Top-left, top-right/bottom-left, bottom-right
					BorderTopLeftRadius = lengths[0];
					BorderTopRightRadius = lengths[1];
					BorderBottomRightRadius = lengths[2];
					BorderBottomLeftRadius = lengths[1];
					return true;

				case 4: // Top-left, top-right, bottom-right, bottom-left
					BorderTopLeftRadius = lengths[0];
					BorderTopRightRadius = lengths[1];
					BorderBottomRightRadius = lengths[2];
					BorderBottomLeftRadius = lengths[3];
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Parse justify-content values
		/// Based on s&box's SetJustifyContent implementation
		/// </summary>
		private bool SetJustifyContent( string value )
		{
			switch ( value )
			{
				case "flex-start":
					JustifyContent = UI.Justify.FlexStart;
					return true;
				case "center":
					JustifyContent = UI.Justify.Center;
					return true;
				case "flex-end":
					JustifyContent = UI.Justify.FlexEnd;
					return true;
				case "space-between":
					JustifyContent = UI.Justify.SpaceBetween;
					return true;
				case "space-around":
					JustifyContent = UI.Justify.SpaceAround;
					return true;
				case "space-evenly":
					JustifyContent = UI.Justify.SpaceEvenly;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse flex-wrap values
		/// Based on s&box's SetFlexWrap implementation
		/// </summary>
		private bool SetFlexWrap( string value )
		{
			switch ( value )
			{
				case "nowrap":
					FlexWrap = Wrap.NoWrap;
					return true;
				case "wrap":
					FlexWrap = Wrap.Wrap;
					return true;
				case "wrap-reverse":
					FlexWrap = Wrap.WrapReverse;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse display mode values
		/// Based on s&box's SetDisplay implementation
		/// </summary>
		private bool SetDisplay( string value )
		{
			switch ( value )
			{
				case "none":
					Display = DisplayMode.None;
					return true;
				case "flex":
					Display = DisplayMode.Flex;
					return true;
				case "contents":
					Display = DisplayMode.Contents;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse pointer-events values
		/// Based on s&box's SetPointerEvents implementation
		/// </summary>
		private bool SetPointerEvents( string value )
		{
			switch ( value )
			{
				case "auto":
					PointerEvents = null;
					return true;
				case "none":
					PointerEvents = UI.PointerEvents.None;
					return true;
				case "all":
					PointerEvents = UI.PointerEvents.All;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse position mode values
		/// Based on s&box's SetPosition implementation
		/// </summary>
		private bool SetPosition( string value )
		{
			switch ( value )
			{
				case "static":
					Position = PositionMode.Static;
					return true;
				case "absolute":
					Position = PositionMode.Absolute;
					return true;
				case "relative":
					Position = PositionMode.Relative;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse text-align values
		/// Based on s&box's SetTextAlign implementation
		/// </summary>
		private bool SetTextAlign( string value )
		{
			switch ( value )
			{
				case "center":
					TextAlign = UI.TextAlign.Center;
					return true;
				case "left":
					TextAlign = UI.TextAlign.Left;
					return true;
				case "right":
					TextAlign = UI.TextAlign.Right;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse text-overflow values
		/// Based on s&box's SetTextOverflow implementation
		/// </summary>
		private bool SetTextOverflow( string value )
		{
			switch ( value )
			{
				case "ellipsis":
					TextOverflow = UI.TextOverflow.Ellipsis;
					return true;
				case "clip":
					TextOverflow = UI.TextOverflow.Clip;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse word-break values
		/// Based on s&box's SetWordBreak implementation
		/// </summary>
		private bool SetWordBreak( string value )
		{
			switch ( value )
			{
				case "normal":
					WordBreak = UI.WordBreak.Normal;
					return true;
				case "break-all":
					WordBreak = UI.WordBreak.BreakAll;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse white-space values
		/// Based on s&box's SetWhiteSpace implementation
		/// </summary>
		private bool SetWhiteSpace( string value )
		{
			switch ( value )
			{
				case "normal":
					WhiteSpace = UI.WhiteSpace.Normal;
					return true;
				case "nowrap":
					WhiteSpace = UI.WhiteSpace.NoWrap;
					return true;
				case "pre-line":
					WhiteSpace = UI.WhiteSpace.PreLine;
					return true;
				case "pre":
					WhiteSpace = UI.WhiteSpace.Pre;
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Parse object-fit values
		/// Based on s&box's SetObjectFit implementation
		/// </summary>
		private bool SetObjectFit( string value )
		{
			value = value.Trim();

			if ( System.Enum.TryParse<ObjectFit>( value, true, out var objectFit ) )
			{
				ObjectFit = objectFit;
				return true;
			}

			return false;
		}
	}
}
