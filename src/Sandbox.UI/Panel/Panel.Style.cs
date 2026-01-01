namespace Sandbox.UI;

public partial class Panel
{
	/// <summary>
	/// This is the style that we computed last. If you're looking to see which
	/// styles are set on this panel then this is what you're looking for.
	/// </summary>
	public Styles ComputedStyle { get; internal set; }

	/// <summary>
	/// Allows you to set styles specifically on this panel. Setting the style will
	/// only affect this panel and no others and will override any other styles.
	/// </summary>
	public PanelStyle Style { get; private set; }

	/// <summary>
	/// Try to find <c>@keyframes</c> CSS rule with given name in <see cref="AllStyleSheets"/>.
	/// </summary>
	/// <param name="name">The name to search for.</param>
	/// <param name="keyframes">The keyframes, if any are found, or <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if <c>@keyframes</c> with given name were found.</returns>
	public bool TryFindKeyframe( string name, out KeyFrames keyframes )
	{
		// TODO: optimization - cache found keyframes? Clear on load?

		keyframes = null;

		foreach ( var sheet in AllStyleSheets )
		{
			if ( sheet.KeyFrames.TryGetValue( name, out var keyframe ) )
			{
				keyframes = keyframe;
				return true;
			}
		}

		return false;
	}

}
