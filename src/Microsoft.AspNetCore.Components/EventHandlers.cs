using System;

namespace Microsoft.AspNetCore.Components.Web
{
	[EventHandler( "onfocus", typeof( EventArgs ), true, true )]
	[EventHandler( "onblur", typeof( EventArgs ), true, true )]
	[EventHandler( "onfocusin", typeof( EventArgs ), true, true )]
	[EventHandler( "onfocusout", typeof( EventArgs ), true, true )]
	[EventHandler( "onmouseover", typeof( EventArgs ), true, true )]
	[EventHandler( "onmouseout", typeof( EventArgs ), true, true )]
	[EventHandler( "onmouseleave", typeof( EventArgs ), true, true )]
	[EventHandler( "onmouseenter", typeof( EventArgs ), true, true )]
	[EventHandler( "onmousemove", typeof( EventArgs ), true, true )]
	[EventHandler( "onmousedown", typeof( EventArgs ), true, true )]
	[EventHandler( "onmouseup", typeof( EventArgs ), true, true )]
	[EventHandler( "onclick", typeof( EventArgs ), true, true )]
	[EventHandler( "ondblclick", typeof( EventArgs ), true, true )]
	public static class EventHandlers
	{
	}
}
