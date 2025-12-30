namespace Sandbox.UI;

/// <summary>
/// This is a tree renderer for panels. If we ever use razor on other ui we'll want to make a copy of
/// this class and do the specific things to that.
/// </summary>
public partial class PanelRenderTreeBuilder : Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder
{
	/// <summary>
	/// Handles @onclick=@( () => DoSomething( "boobies" ) )
	/// </summary>
	public void AddAttributeAction( int sequence, string attrName, Action value )
	{
		// There's no real good way to cache this shit. The function is a local function, the captured variables could change at any time.
		// The cost here is removing and adding to a list. We could find a way to make that less impactful. But lets wait until
		// it is impactful.

		//Log.Info( $"AddEventListener {attrName}" );

		CurrentScope.Element.RemoveEventListener( attrName );
		CurrentScope.Element.AddEventListener( attrName, value );
	}

	/// <summary>
	/// Handles @onclick=@( () => await DoSomethingAsync( "boobies" ) )
	/// </summary>
	public void AddAttributeAction( int sequence, string attrName, Func<Task> value )
	{
		var e = CurrentScope.Element;
		AddAttributeAction( sequence, attrName, () => { _ = value(); e?.StateHasChanged(); } );
	}

	/// <summary>
	/// Handles @onclick=@( ( PanelEvent e ) => DoSomething( e.This, "boobies" ) )
	/// </summary>
	internal void AddPanelEventAttribute( int sequence, string attrName, Action<PanelEvent> value )
	{
		// There's no real good way to cache this shit. The function is a local function, the captured variables could change at any time.
		// The cost here is removing and adding to a list. We could find a way to make that less impactful. But lets wait until
		// it is impactful.

		//Log.Info( $"AddEventListener {attrName}" );
		CurrentScope.Element.RemoveEventListener( attrName );
		CurrentScope.Element.AddEventListener( attrName, value );
	}
}
