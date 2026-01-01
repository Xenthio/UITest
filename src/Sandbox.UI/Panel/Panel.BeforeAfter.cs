namespace Sandbox.UI;

/// <summary>
/// Support for CSS ::before and ::after pseudo-elements.
/// Ported from s&box engine/Sandbox.Engine/Systems/UI/Panel/Panel.BeforeAfter.cs
/// </summary>
public partial class Panel
{
	/// <summary>
	/// The ::before pseudo-element, if it exists.
	/// </summary>
	Panel? _beforeElement;

	/// <summary>
	/// The ::after pseudo-element, if it exists.
	/// </summary>
	Panel? _afterElement;

	/// <summary>
	/// Called during tick to create or destroy the ::before and ::after elements.
	/// </summary>
	void UpdateBeforeAfterElements()
	{
		// Don't do this if we ARE a ::before or ::after element.
		if (PseudoClass.HasFlag(PseudoClass.Before)) return;
		if (PseudoClass.HasFlag(PseudoClass.After)) return;

		BuildPseudoElement(Style.HasBeforeElement, PseudoClass.Before, ref _beforeElement);
		BuildPseudoElement(Style.HasAfterElement, PseudoClass.After, ref _afterElement);

		// Make sure ::before is always first
		if (_beforeElement != null && _beforeElement.IsValid())
			SetChildIndex(_beforeElement, 0);

		// Make sure ::after is always last
		if (_afterElement != null && _afterElement.IsValid() && _children != null)
			SetChildIndex(_afterElement, _children.Count - 1);
	}

	/// <summary>
	/// Called to update the state of a ::before or ::after element.
	/// Either destroys it or creates it based on whether it should exist.
	/// </summary>
	private void BuildPseudoElement(bool shouldExist, PseudoClass additionalClass, ref Panel? panel)
	{
		// Destroy it if it exists but shouldn't
		if (!shouldExist)
		{
			if (panel != null)
			{
				panel.Delete();
				panel = null;
			}

			return;
		}

		// Create it if it doesn't exist but should
		if (panel == null || !panel.IsValid())
		{
			panel = new Label();
			panel.ElementName = "element";
			panel.RemoveClass("label");
			panel.PseudoClass = additionalClass;
			AddChild(panel);
		}
	}
}
