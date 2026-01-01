
namespace Sandbox.Utility;

/// <summary>
/// Easing functions used for transitions. See <a href="https://easings.net/">https://easings.net/</a> for examples.
/// </summary>
public static class Easing
{
	/// <summary>
	/// An easing function that transforms the linear input into non linear output.
	/// </summary>
	/// <param name="delta">A linear input value from 0 to 1</param>
	/// <returns>The resulting non linear output value, from 0 to 1</returns>
	public delegate float Function( float delta );

	private static readonly Dictionary<string, Function> _functions = new Dictionary<string, Function>
	{
		{ "linear",         Linear },

		{ "ease",           QuadraticInOut },
		{ "ease-in-out",    ExpoInOut },
		{ "ease-out",       QuadraticOut },
		{ "ease-in",        QuadraticIn },

		{ "bounce-in",      BounceIn },
		{ "bounce-out",     BounceOut },
		{ "bounce-in-out",  BounceInOut },

		{ "sin-ease-in",     SineEaseIn },
		{ "sin-ease-out",    SineEaseOut },
		{ "sin-ease-in-out", SineEaseInOut },
	};

	/// <inheritdoc cref="ExpoInOut"/>
	public static float EaseInOut( float f ) => ExpoInOut( f );
	/// <inheritdoc cref="QuadraticIn"/>
	public static float EaseIn( float f ) => QuadraticIn( f );
	/// <inheritdoc cref="QuadraticOut"/>
	public static float EaseOut( float f ) => QuadraticOut( f );

	/// <summary>
	/// Linear easing function, x=y.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float Linear( float f ) => f;

	/// <summary>
	/// Quadratic ease in.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float QuadraticIn( float f ) => f * f;

	/// <summary>
	/// Quadratic ease out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float QuadraticOut( float f ) => f * (2.0f - f);

	/// <summary>
	/// Quadratic ease in and out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float QuadraticInOut( float f ) => (f *= 2.0f) < 1.0f ? 0.5f * f * f : -0.5f * ((f -= 1f) * (f - 2f) - 1f);


	/// <summary>
	/// Exponential ease in.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float ExpoIn( float f ) => f == 0f ? 0f : MathF.Pow( 1024f, f - 1f );

	/// <summary>
	/// Exponential ease out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float ExpoOut( float f ) => f == 1f ? 1f : 1f - MathF.Pow( 2f, -10f * f );

	/// <summary>
	/// Exponential ease in and out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float ExpoInOut( float f ) => f < 0.5 ? ExpoIn( f * 2.0f ) * 0.5f : ExpoOut( (f - 0.5f) * 2.0f ) * 0.5f + 0.5f;


	/// <summary>
	/// Bouncy ease in.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float BounceIn( float f ) => 1f - BounceOut( 1f - f );

	/// <summary>
	/// Bouncy ease out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float BounceOut( float f ) => f < (1f / 2.75f) ? 7.5625f * f * f : f < (2f / 2.75f) ? 7.5625f * (f -= (1.5f / 2.75f)) * f + 0.75f : f < (2.5f / 2.75f) ? 7.5625f * (f -= (2.25f / 2.75f)) * f + 0.9375f : 7.5625f * (f -= (2.625f / 2.75f)) * f + 0.984375f;

	/// <summary>
	/// Bouncy ease in and out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float BounceInOut( float f ) => f < 0.5 ? BounceIn( f * 2.0f ) * 0.5f : BounceOut( (f - 0.5f) * 2.0f ) * 0.5f + 0.5f;


	/// <summary>
	/// Sine ease in.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float SineEaseIn( float f ) => 1.0f - MathF.Cos( (f * MathF.PI) * 0.5f );

	/// <summary>
	/// Sine ease out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float SineEaseOut( float f ) => MathF.Sin( (f * MathF.PI) * 0.5f );

	/// <summary>
	/// Sine ease in and out.
	/// </summary>
	/// <param name="f">Input in range of 0 to 1.</param>
	/// <returns>Output in range 0 to 1.</returns>
	public static float SineEaseInOut( float f ) => -(MathF.Cos( MathF.PI * f ) - 1.0f) * 0.5f;


	/// <summary>
	/// Add an easing function.
	/// If the function already exists we silently return.
	/// </summary>
	internal static void AddFunction( string name, Function func )
	{
		if ( _functions.ContainsKey( name ) )
			return;

		_functions[name] = func;
	}

	/// <summary>
	/// Get an easing function by name (ie, "ease-in").
	/// If the function doesn't exist we return QuadraticInOut
	/// </summary>
	public static Function GetFunction( string name )
	{
		if ( _functions.TryGetValue( name, out var f ) )
			return f;

		return QuadraticInOut;
	}

	/// <summary>
	/// Get an easing function by name (ie, "ease-in").
	/// If the function exists we return true, otherwise return false.
	/// </summary>
	public static bool TryGetFunction( string name, out Function function )
	{
		return _functions.TryGetValue( name, out function );
	}
}
