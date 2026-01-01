namespace Sandbox.UI;

/// <summary>
/// IPanel interface implementation for Panel.
/// This allows panels to be used polymorphically through the IPanel interface.
/// Ported from s&box engine/Sandbox.Engine/Systems/UI/Panel/Panel.IPanel.cs
/// Note: GetPanelAt is already in Panel.Input.cs
/// </summary>
public partial class Panel
{
	/// <summary>
	/// Get the depth of this panel in the tree (how many ancestors it has)
	/// </summary>
	int Depth => 1 + (Parent?.Depth ?? 0);
}
