using System;

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Represents a segment of UI content, implemented as a delegate that
/// writes the content to a builder
/// </summary>
public delegate void RenderFragment( Rendering.RenderTreeBuilder builder );

/// <summary>
/// Represents a segment of UI content for an object of type <typeparamref name="TValue"/>, implemented as
/// a function that returns a <see cref="RenderFragment"/>.
/// </summary>
public delegate RenderFragment RenderFragment<TValue>( TValue value );

/// <summary>
/// A component type
/// </summary>
public interface IComponent { }

/// <summary>
/// Signifies a parameter attribute
/// </summary>
public class ParameterAttribute : Attribute
{
}

/// <summary>
/// A base component
/// </summary>
public class ComponentBase : IComponent
{
}

[System.AttributeUsage( System.AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
public sealed class EventHandlerAttribute : System.Attribute
{
	public EventHandlerAttribute( string attributeName, System.Type eventArgsType, bool enableStopPropagation, bool enablePreventDefault )
	{
		ArgumentNullException.ThrowIfNull( attributeName );
		ArgumentNullException.ThrowIfNull( eventArgsType );

		AttributeName = attributeName;
		EventArgsType = eventArgsType;
		EnableStopPropagation = enableStopPropagation;
		EnablePreventDefault = enablePreventDefault;
	}

	/// <summary>
	/// Gets the attribute name.
	/// </summary>
	public string AttributeName { get; }

	/// <summary>
	/// Gets the event argument type.
	/// </summary>
	public Type EventArgsType { get; }

	/// <summary>
	/// Gets the event's ability to stop propagation.
	/// </summary>
	public bool EnableStopPropagation { get; }

	/// <summary>
	/// Gets the event's ability to prevent default event flow.
	/// </summary>
	public bool EnablePreventDefault { get; }
}

internal interface IEventCallback
{
	bool HasDelegate { get; }

	object UnpackForRenderTree();
}

public interface IHandleEvent
{
}

public readonly struct EventCallback : IEventCallback
{
	public static readonly EventCallbackFactory Factory;
	public object UnpackForRenderTree() => null;
	public bool HasDelegate => false;
}

public readonly struct EventCallback<TValue> : IEventCallback
{
	public object UnpackForRenderTree() => null;
	public bool HasDelegate => false;
}

public sealed class EventCallbackFactory
{
	public EventCallback<TValue> Create<TValue>( object receiver, System.Action callback ) => throw null;
	public object UnpackForRenderTree() => null;
	public bool HasDelegate => false;
}



/// <summary>
/// Specifies that the component parameter is required to be provided by the user when authoring it in the editor.
/// <para>
/// If a value for this parameter is not provided, editors or build tools may provide warnings indicating the user to
/// specify a value. This attribute is only valid on properties marked with <see cref="ParameterAttribute"/>.
/// </para>
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
public sealed class EditorRequiredAttribute : Attribute
{
}
