using System;

namespace Microsoft.AspNetCore.Components
{

	namespace Rendering
	{
		public readonly struct RenderHandle
		{
			public void Render( Action<object> renderFragment ) { }
			public void InvokeAsync( Action workItem ) { }
		}
	}

}
