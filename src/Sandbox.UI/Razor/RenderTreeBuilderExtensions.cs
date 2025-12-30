using Microsoft.AspNetCore.Components.Rendering;
using Sandbox.UI;

namespace Microsoft.AspNetCore.Components;

public static class RazorExtensions
{
	public static void AddAttribute(this RenderTreeBuilder self, int sequence, string attrName, Action<PanelEvent> value)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddPanelEventAttribute(sequence, attrName, value);
	}

	public static void AddAttribute(this RenderTreeBuilder self, int sequence, string attrName, Func<Task> value)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddAttributeAction(sequence, attrName, value);
	}

	public static void AddAttribute(this RenderTreeBuilder self, int sequence, string attrName, Action value)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddAttributeAction(sequence, attrName, value);
	}

	public static void AddAttribute(this RenderTreeBuilder self, int sequence, string attrName, object value)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddAttributeObject(sequence, attrName, value);
	}

	public static void AddAttribute(this RenderTreeBuilder self, int sequence, string attrName, string value)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddAttributeString(sequence, attrName, value);
	}

	public static void AddAttribute<T>(this RenderTreeBuilder self, int sequence, object value, Action<T> setter)
	{
		if (self is not PanelRenderTreeBuilder ptb) return;

		ptb.AddAttributeWithSetter<T>(sequence, value, setter);
	}
}
