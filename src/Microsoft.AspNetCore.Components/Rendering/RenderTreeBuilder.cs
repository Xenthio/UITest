using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.Rendering;

public abstract partial class RenderTreeBuilder
{
	public abstract void AddLocation( string filename, int line, int column );
	public abstract void OpenElement( int sequence, string elementName );
	public abstract void OpenElement( int sequence, string elementName, object key = null );
	public abstract void AddStyleDefinitions( int sequence, string styles );
	public abstract void AddAttribute<T>( int sequence, Action<T> value ) where T : IComponent;
	public abstract void CloseElement();
	public abstract void AddContent<T>( int sequence, T content );
	public abstract void AddReferenceCapture<T>( int sequence, T current, Action<T> value ) where T : IComponent;
	public abstract void SetRenderFragment<T>( Action<T, RenderFragment> setter, RenderFragment builder ) where T : IComponent;
	public abstract void SetRenderFragmentWithContext<T, U>( Func<T, RenderFragment<U>> getter, Action<T, RenderFragment<U>> setter, RenderFragment<U> builder ) where T : IComponent;

	public abstract void AddMarkupContent( int sequence, string markupContent );
	public abstract void OpenElement<T>( int sequence ) where T : IComponent, new();
	public abstract void OpenElement<T>( int sequence, object key ) where T : IComponent, new();
	public abstract void AddBind<T>( int sequence, string propertyName, Func<T> get, Action<T> set );

	// These aren't used by our system, they're only used by intellisense and VS
	public void OpenComponent<T>( int sequence ) where T : IComponent { }
	public void CloseComponent() { }
	public void AddComponentParameter( int sequence, string parameterName, object value ) { }
	public void AddComponentParameter( int sequence, string parameterName, Action value ) { }
	public void AddComponentParameter( int sequence, string parameterName, Func<Task> value ) { }
	public void SetKey( object value ) { }
}
