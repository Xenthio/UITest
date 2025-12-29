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

			// TODO: Add more shorthand property handlers as needed
			// (padding, margin, border, etc.) - see s&box's Styles.Set.cs for reference

			return base.Set( property, value );
		}
	}
}
