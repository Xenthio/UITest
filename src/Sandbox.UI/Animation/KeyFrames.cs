namespace Sandbox.UI;

/// <summary>
/// Represents a CSS <c>@keyframes</c> rule.
/// </summary>
public partial class KeyFrames
{
	/// <summary>
	/// Name of the <c>@keyframes</c> rule.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// A keyframe within the animation.
	/// </summary>
	public class Block
	{
		/// <summary>
		/// The position of the keyframe within the overall animation. 0 to 1, where 0 is the start, and 1 is the end of the animation.
		/// </summary>
		public float Interval { get; set; }

		/// <summary>
		/// The styles that should be applied at this position in the animation.
		/// </summary>
		public Styles Styles { get; set; }
	}

	/// <summary>
	/// List of keyframes with in the <c>@keyframes</c> rule.
	/// </summary>
	public List<Block> Blocks = new List<Block>();

	internal void FillStyle( float delta, Styles animStyle )
	{
		var startBlock = Blocks.First();
		var endBlock = startBlock;

		animStyle.From( startBlock.Styles );

		// Work out previous and next blocks
		foreach ( var block in Blocks )
		{
			endBlock = block;
			if ( block.Interval > delta ) break;
			startBlock = block;
		}

		// If startBlock & endBlock intervals are same, difference becomes zero
		// which results in division by zero (NaN)
		float t;
		if ( startBlock.Interval == endBlock.Interval )
			t = 0f;
		else
			t = LerpInverse( delta, startBlock.Interval, endBlock.Interval );

		animStyle.FromLerp( startBlock.Styles, endBlock.Styles, t );
	}

	/// <summary>
	/// Inverse lerp - finds the interpolation factor between two values
	/// </summary>
	private static float LerpInverse( float value, float from, float to )
	{
		if ( from == to ) return 0;
		return Math.Clamp( (value - from) / (to - from), 0, 1 );
	}
}
