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

				// Image properties
				case "background-image":
					return SetImage( value, SetBackgroundImageFromTexture );

				case "border-image":
					return SetBorderImage( value );
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

		/// <summary>
		/// Parse image properties (background-image, mask-image, border-image source)
		/// Based on s&box's SetImage implementation
		/// </summary>
		private bool SetImage( string value, System.Func<System.Lazy<Texture>, bool> setImage )
		{
			var p = new Parse( value );
			p = p.SkipWhitespaceAndNewlines();

			if ( p.Is( "none", 0, true ) )
			{
				setImage( new System.Lazy<Texture>( Texture.Invalid ) );
				return true;
			}

			if ( GetTokenValueUnderParenthesis( p, "url", out string url ) )
			{
				url = url.Trim( ' ', '"', '\'' );
				setImage( new System.Lazy<Texture>( () =>
				{
					return Texture.Load( url ) ?? Texture.Invalid;
				} ) );
				return true;
			}

			// Linear gradients, radial gradients etc. would go here
			// For now we only support url() and none

			return false;
		}

		/// <summary>
		/// Helper to extract value inside parentheses for functions like url(), linear-gradient(), etc.
		/// Based on s&box's GetTokenValueUnderParenthesis implementation
		/// </summary>
		private bool GetTokenValueUnderParenthesis( Parse p, string tokenName, out string result )
		{
			if ( p.Is( tokenName, 0, true ) )
			{
				p.Pointer += tokenName.Length;
				p = p.SkipWhitespaceAndNewlines();

				if ( p.Current != '(' )
				{
					result = "";
					return false;
				}

				p.Pointer++;

				int stack = 1;
				var wordStart = p;

				while ( !p.IsEnd && stack > 0 )
				{
					p.Pointer++;
					if ( p.Current == '(' ) stack++;
					if ( p.Current == ')' ) stack--;
				}

				if ( p.IsEnd )
				{
					result = "";
					return false;
				}

				result = wordStart.Read( p.Pointer - wordStart.Pointer );
				return true;
			}
			result = "";
			return false;
		}

		/// <summary>
		/// Parse border-image shorthand property
		/// Based on s&box's SetBorderImage implementation
		/// Syntax: border-image: source slice / width repeat fill
		/// </summary>
		private bool SetBorderImage( string value )
		{
			var p = new Parse( value );

			p = p.SkipWhitespaceAndNewlines();

			// Parse the image source
			if ( !SetImage( p.Text, SetBorderTexture ) )
			{
				return false;
			}

			// Skip past the url(...) part
			p.Pointer += p.ReadUntilOrEnd( ")" ).Length + 1;

			var borderSliceList = new System.Collections.Generic.List<Length>();
			var borderWidthList = new System.Collections.Generic.List<Length>();

			// 0 = parsing slice, 1 = parsing width
			int parseType = 0;

			while ( !p.IsEnd )
			{
				p = p.SkipWhitespaceAndNewlines();
				
				if ( p.Is( "stretch", 0, true ) )
				{
					p.Pointer += "stretch".Length;
					BorderImageRepeat = UI.BorderImageRepeat.Stretch;
				}
				else if ( p.Is( "round", 0, true ) )
				{
					p.Pointer += "round".Length;
					BorderImageRepeat = UI.BorderImageRepeat.Round;
				}
				else if ( p.Is( "fill", 0, true ) )
				{
					p.Pointer += "fill".Length;
					BorderImageFill = UI.BorderImageFill.Filled;
				}
				else if ( p.Is( "/", 0, true ) )
				{
					p.Pointer++;

					// Needs to have at least one element before we do it
					if ( borderSliceList.Count == 0 )
					{
						return false;
					}

					// We don't support anything else
					if ( parseType == 1 )
					{
						return false;
					}

					parseType = 1;
				}
				else if ( p.TryReadLength( out Length lengthValue ) )
				{
					if ( parseType == 0 )
					{
						borderSliceList.Add( lengthValue );
					}
					else
					{
						borderWidthList.Add( lengthValue );
					}
				}

				if ( p.IsEnd )
					break;

				p.Pointer++;
				p = p.SkipWhitespaceAndNewlines();
			}

			// Parse our border slice pixel sizes
			switch ( borderSliceList.Count )
			{
				// 33.3% of texture size
				case 0:
					if ( BorderImageSource != null )
					{
						BorderImageWidthLeft = BorderImageWidthRight = BorderImageWidthTop = BorderImageWidthBottom = BorderImageSource.Width / 3.0f;
					}
					break;

				// Uniform
				case 1:
					BorderImageWidthLeft = BorderImageWidthRight = BorderImageWidthTop = BorderImageWidthBottom = borderSliceList[0];
					break;

				// Top-Bottom and Left-Right
				case 2:
					BorderImageWidthTop = BorderImageWidthBottom = borderSliceList[0];
					BorderImageWidthLeft = BorderImageWidthRight = borderSliceList[1];
					break;

				// Top, Left-Right and Bottom
				case 3:
					BorderImageWidthTop = borderSliceList[0];
					BorderImageWidthLeft = BorderImageWidthRight = borderSliceList[1];
					BorderImageWidthBottom = borderSliceList[2];
					break;

				// Top, Right, Bottom, Left
				case 4:
					BorderImageWidthTop = borderSliceList[0];
					BorderImageWidthRight = borderSliceList[1];
					BorderImageWidthBottom = borderSliceList[2];
					BorderImageWidthLeft = borderSliceList[3];
					break;
			}

			// Parse our border width pixel sizes, we re use BorderWidth so we don't need to pass another uniform to the shader
			switch ( borderWidthList.Count )
			{
				// Just copy whatever is on slice if nothing is set
				case 0:
					BorderLeftWidth = BorderImageWidthLeft;
					BorderRightWidth = BorderImageWidthRight;
					BorderTopWidth = BorderImageWidthTop;
					BorderBottomWidth = BorderImageWidthBottom;
					break;

				// Uniform
				case 1:
					BorderLeftWidth = BorderRightWidth = BorderTopWidth = BorderBottomWidth = borderWidthList[0];
					break;

				// Top-Bottom and Left-Right
				case 2:
					BorderTopWidth = BorderBottomWidth = borderWidthList[0];
					BorderLeftWidth = BorderRightWidth = borderWidthList[1];
					break;

				// Top, Left-Right and Bottom
				case 3:
					BorderTopWidth = borderWidthList[0];
					BorderLeftWidth = BorderRightWidth = borderWidthList[1];
					BorderBottomWidth = borderWidthList[2];
					break;

				// Top, Right, Bottom, Left
				case 4:
					BorderTopWidth = borderWidthList[0];
					BorderRightWidth = borderWidthList[1];
					BorderBottomWidth = borderWidthList[2];
					BorderLeftWidth = borderWidthList[3];
					break;
			}

			return true;
		}

		/// <summary>
		/// Set border texture from parsed image
		/// Based on s&box's SetBorderTexture implementation
		/// </summary>
		private bool SetBorderTexture( System.Lazy<Texture> t )
		{
			_borderImageSource = t;
			return true;
		}

		/// <summary>
		/// Set background image from parsed texture
		/// Based on s&box's SetBackgroundImageFromTexture implementation
		/// </summary>
		private bool SetBackgroundImageFromTexture( System.Lazy<Texture> texture )
		{
			if ( texture == null )
				return true;

			_backgroundImage = texture;
			Dirty();

			return true;
		}
	}
}
