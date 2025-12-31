using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Sandbox.UI;

internal class PanelInput
{
	/// <summary>
	/// Panel we're currently hovered over
	/// </summary>
	public Panel? Hovered { get; private set; }

	/// <summary>
	/// Panel we're currently pressing down
	/// </summary>
	public Panel? Active { get; private set; }

	internal MouseButtonState[] MouseStates;
	private Vector2 _lastMousePosition;

	public PanelInput()
	{
		MouseStates = new MouseButtonState[5];

		for (int i = 0; i < 5; i++)
		{
			MouseStates[i] = new MouseButtonState(this, GetButtonName(i));
		}
	}

	private string GetButtonName(int index)
	{
		return index switch
		{
			0 => "mouseleft",
			1 => "mouseright",
			2 => "mousemiddle",
			3 => "mouseback",
			4 => "mouseforward",
			_ => $"mouse{index}"
		};
	}

	internal void Tick(IEnumerable<RootPanel> panels, Vector2 mousePosition, bool mouseIsActive)
	{
		bool hoveredAny = false;
		bool mouseMoved = _lastMousePosition != mousePosition;

		if (mouseIsActive)
		{
			foreach (var panel in panels)
			{
				if (UpdateMouse(panel, mousePosition))
				{
					hoveredAny = true;
					break;
				}
			}
		}

		if (!hoveredAny)
		{
			SetHovered(null);
		}

		// Dispatch onmousemove event if mouse moved and we have an active panel
		if (mouseMoved && Active != null)
		{
			var mouseMoveEvent = new MousePanelEvent("onmousemove", Active, "mouseleft");
			mouseMoveEvent.LocalPosition = mousePosition;
			Active.CreateEvent(mouseMoveEvent);
			Active.ProcessPendingEvents();
		}

		_lastMousePosition = mousePosition;
	}

	public void AddMouseButton(string button, bool down)
	{
		var index = button switch
		{
			"mouseleft" => 0,
			"mouseright" => 1,
			"mousemiddle" => 2,
			"mouseback" => 3,
			"mouseforward" => 4,
			_ => -1
		};

		if (index >= 0 && index < MouseStates.Length)
		{
			MouseStates[index].Update(down, Hovered);
		}
	}

	private bool UpdateMouse(RootPanel root, Vector2 mousePos)
	{
		root.MousePos = mousePos;

		if (!UpdateHovered(root, mousePos))
			return false;

		// Update active state based on mouse button states
		Active = null;
		if (MouseStates[2].Active != null) Active = MouseStates[2].Active;
		if (MouseStates[1].Active != null) Active = MouseStates[1].Active;
		if (MouseStates[0].Active != null) Active = MouseStates[0].Active;

		return true;
	}

	private bool UpdateHovered(Panel panel, Vector2 pos)
	{
		Panel? current = null;

		if (!CheckHover(panel, pos, ref current))
		{
			return false;
		}

		SetHovered(current);

		return true;
	}

	internal void SetHovered(Panel? current)
	{
		if (current != Hovered)
		{
			if (Hovered != null)
			{
				Hovered.Switch(PseudoClass.Hover, false);
			}

			Hovered = current;

			if (Hovered != null)
			{
				if (Active == null || Active == Hovered)
					Hovered.Switch(PseudoClass.Hover, true);
			}
		}
	}

	private bool CheckHover(Panel panel, Vector2 pos, ref Panel? current)
	{
		bool found = false;

		if (!panel.IsVisible)
			return false;

		if (panel.ComputedStyle == null)
			return false;

		// Transform using this panel's local matrix (like S&box does)
		pos = panel.GetTransformPosition(pos);

		var inside = panel.IsInside(pos);

		// Check pointer-events (matches S&box - defaults to None via property getter)
		if (inside && panel.ComputedStyle.PointerEvents != PointerEvents.None)
		{
			// Debug: Log what panel is being set as current
			// System.Console.WriteLine($"CheckHover: Setting current to {panel.GetType().Name} (was {current?.GetType().Name})");
			current = panel;
			found = true;
		}

		// If we're outside and this panel has overflow hidden we can avoid testing against the children
		if (!inside && (panel.ComputedStyle?.Overflow ?? OverflowMode.Visible) != OverflowMode.Visible)
		{
			return found;
		}

		// No children
		if (panel.ChildrenCount == 0)
		{
			return found;
		}

		// Check children with proper z-ordering (using render order index like S&box)
		int topIndex = -10000;
		
		foreach (var child in panel.Children)
		{
			var index = child.GetRenderOrderIndex();
			if (index < topIndex) continue;

			if (CheckHover(child, pos, ref current))
			{
				topIndex = index;
				found = true;
			}
		}

		return found;
	}

	internal class MouseButtonState
	{
		public PanelInput Input { get; init; }
		public string ButtonName { get; init; }

		public bool Pressed;
		public Panel? Active;

		public MouseButtonState(PanelInput input, string buttonName)
		{
			Input = input;
			ButtonName = buttonName;
		}

		public void Update(bool down, Panel? hovered)
		{
			if (Pressed == down) return;
			Pressed = down;

			if (down) OnPressed(hovered);
			else OnReleased(hovered);
		}

		private void OnPressed(Panel? hovered)
		{
			Active = hovered;

			//Console.WriteLine($"OnPressed: {ButtonName}, hovered={hovered?.GetType().Name}");

			if (Active == null)
				return;

			Active.Switch(PseudoClass.Active, true);

			// Always call Focus() - it will walk up to find a focusable parent (matches S&box)
			Active.Focus();

			// Create and dispatch onmousedown event
			var mouseDownEvent = new MousePanelEvent("onmousedown", Active, ButtonName);
			Active.CreateEvent(mouseDownEvent);
			Active.ProcessPendingEvents();

			Active.OnButtonEvent(new ButtonEvent(ButtonName, true));
		}

		private void OnReleased(Panel? hovered)
		{
			bool canClick = hovered == Active;

			if (Active == null)
				return;

			if (canClick && ButtonName == "mouseleft")
			{
				// Create onclick event for the active panel
				var clickEvent = new MousePanelEvent("onclick", Active, ButtonName);
				Active.CreateEvent(clickEvent);
				Active.ProcessPendingEvents();
			}

			Active.Switch(PseudoClass.Active, false);

			Active.OnButtonEvent(new ButtonEvent(ButtonName, false));
			Active = null;
		}
	}
}
