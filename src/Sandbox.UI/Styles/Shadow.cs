
namespace Sandbox.UI;

/// <summary>
/// Shadow style settings
/// </summary>
[SkipHotload]
public struct Shadow
{
	/// <summary>
	/// Shadow offset on the X axis.
	/// </summary>
	public float OffsetX;

	/// <summary>
	/// Shadow offset on the Y axis.
	/// </summary>
	public float OffsetY;

	/// <summary>
	/// Amount of blurring for the shadow.
	/// </summary>
	public float Blur;

	/// <summary>
	/// Increases the box size by this much before starting shadow blur.
	/// Box shadows only.
	/// </summary>
	public float Spread;

	/// <summary>
	/// Whether or not this shadow is inset.
	/// Box shadows only.
	/// </summary>
	public bool Inset;

	/// <summary>
	/// Color of the shadow.
	/// </summary>
	public Color Color;

	// garry: we shouldn't be storing this stuff here!
	//		  maybe store them on the panel with a lookup by shadow index?
	//public Texture ShadowTexture { get; set; }

	// TODO - render mode would be cool

	/// <summary>
	/// Scale all variables by given scalar.
	/// </summary>
	/// <param name="f">How much to scale the shadow parameters by. 1 is no change, 2 is double the sizes, etc.</param>
	/// <returns>The scaled shadow.</returns>
	public Shadow Scale( float f )
	{
		var s = this;
		s.OffsetX *= f;
		s.OffsetY *= f;
		s.Blur *= f;
		s.Spread *= f;
		return s;
	}

	/// <summary>
	/// Perform linear interpolation between 2 shadows.
	/// </summary>
	/// <param name="shadow">The target shadow to morph into.</param>
	/// <param name="delta">Progress of the transformation. 0 = original shadow, 1 = fully target shadow.</param>
	/// <returns>The interpolated shadow.</returns>
	public Shadow LerpTo( Shadow shadow, float delta )
	{
		var s = new Shadow
		{
			OffsetX = OffsetX.LerpTo( shadow.OffsetX, delta, false ),
			OffsetY = OffsetY.LerpTo( shadow.OffsetY, delta, false ),
			Blur = Blur.LerpTo( shadow.Blur, delta, false ),
			Spread = Spread.LerpTo( shadow.Spread, delta, false ),
			Color = Color.Lerp( Color, shadow.Color, delta ),
		};

		return s;
	}

	public readonly override int GetHashCode()
	{
		return HashCode.Combine( OffsetX, OffsetY, Blur, Spread, Color );
	}
}

/// <summary>
/// A list of shadows 
/// </summary>
[SkipHotload]
public sealed class ShadowList : List<Shadow>
{
	/// <summary>
	/// Whether there are no shadows at all.
	/// </summary>
	public bool IsNone;

	/// <summary>
	/// Copy shadows from another list of shadows.
	/// </summary>
	public void AddFrom( ShadowList other )
	{
		if ( !other.IsNone && other.Count == 0 )
			return;

		Clear();

		if ( other.IsNone )
			return;

		AddRange( other );
	}

	/// <summary>
	/// Given 2 lists of shadows, perform linear interpolation on both lists and store the result in this list.
	/// Will work with mismatched shadow counts.
	/// </summary>
	/// <param name="a">The first list of shadows.</param>
	/// <param name="b">The second list of shadows.</param>
	/// <param name="frac">Fraction for the linear interpolation, in range of [0,1]</param>
	/// <exception cref="System.ArgumentException">Thrown when both inputs are equal.</exception>
	public void SetFromLerp( ShadowList a, ShadowList b, float frac )
	{
		if ( a == b ) throw new System.ArgumentException( "Input lists cannot be the same list.", nameof( b ) );

		var incomingCount = Math.Max( a.Count, b.Count );

		if ( Count != incomingCount )
		{
			Clear();

			for ( int i = 0; i < incomingCount; i++ )
				Add( default );
		}

		for ( int i = 0; i < incomingCount; i++ )
		{
			var shadow_a = a.Get( i );
			var shadow_b = b.Get( i );

			this[i] = shadow_a.LerpTo( shadow_b, frac );
		}

	}

	private Shadow Get( int i )
	{
		if ( i >= Count ) return default;
		return this[i];
	}

	internal ShadowList MakeCopy()
	{
		// fucking hell mate
		var copy = new ShadowList();
		copy.IsNone = IsNone;
		copy.AddRange( this );
		return copy;
	}

	public override int GetHashCode()
	{
		var code = 0;

		foreach ( var e in this )
		{
			code = HashCode.Combine( code, e );
		}

		return code;
	}
}
