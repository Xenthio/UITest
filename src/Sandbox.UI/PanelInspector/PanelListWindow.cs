using System.Numerics;

namespace Sandbox.UI.PanelInspector;

/// <summary>
/// Inspector window that shows the panel hierarchy.
/// Based on s&box's PanelList.cs (MIT Licensed)
/// </summary>
public class PanelListWindow : Window
{
	private Panel? contentContainer;
	private TextEntry? filterInput;
	private Button? refreshButton;
	private Button? pickerButton;
	private Panel? treeContainer;
	
	private RootPanel? targetRootPanel;
	private Panel? selectedPanel;
	private Panel? hoveredPanel;
	private bool pickerActive;

	public event Action<Panel?>? PanelSelected;
	public event Action<Panel?>? PanelHovered;

	public PanelListWindow() : base()
	{
		Title = "Panel Inspector";
		WindowWidth = 400;
		WindowHeight = 600;
		
		BuildUI();
	}

	private void BuildUI()
	{
		// Create toolbar
		var toolbar = new Panel(this);
		toolbar.AddClass("toolbar");
		toolbar.Style.FlexDirection = FlexDirection.Row;
		toolbar.Style.PaddingLeft = 4;
		toolbar.Style.PaddingRight = 4;
		toolbar.Style.PaddingTop = 4;
		toolbar.Style.PaddingBottom = 4;

		filterInput = new TextEntry();
		filterInput.Parent = toolbar;
		filterInput.Placeholder = "Filter panels...";
		filterInput.Style.FlexGrow = 1;

		pickerButton = new Button();
		pickerButton.Parent = toolbar;
		pickerButton.Text = "Pick";
		pickerButton.AddClass("picker-button");
		pickerButton.OnClick += TogglePicker;

		refreshButton = new Button();
		refreshButton.Parent = toolbar;
		refreshButton.Text = "Refresh";
		refreshButton.OnClick += Rebuild;

		// Create tree container
		treeContainer = new Panel(this);
		treeContainer.AddClass("tree-container");
		treeContainer.Style.FlexGrow = 1;
		treeContainer.Style.Overflow = OverflowMode.Scroll;
	}

	public void SetTargetRootPanel(RootPanel? root)
	{
		targetRootPanel = root;
		Rebuild();
	}

	private void Rebuild()
	{
		if (treeContainer == null) return;
		
		treeContainer.DeleteChildren();

		if (targetRootPanel == null || !targetRootPanel.IsValid())
			return;

		BuildPanelTree(targetRootPanel, treeContainer, 0);
	}

	private void BuildPanelTree(Panel panel, Panel container, int depth)
	{
		if (panel == null || !panel.IsValid())
			return;

		var nodeContainer = new Panel(container);
		nodeContainer.AddClass("node-row");
		nodeContainer.Style.PaddingLeft = depth * 20;
		nodeContainer.Style.PaddingTop = 4;
		nodeContainer.Style.PaddingRight = 4;
		nodeContainer.Style.PaddingBottom = 4;

		var node = new PanelTreeNode(panel, nodeContainer);
		
		// Make it clickable
		nodeContainer.AddEventListener("onclick", (PanelEvent e) =>
		{
			SelectPanel(panel);
		});

		// Build children
		if (panel.ChildrenCount > 0)
		{
			foreach (var child in panel.Children)
			{
				BuildPanelTree(child, container, depth + 1);
			}
		}
	}

	private void SelectPanel(Panel? panel)
	{
		selectedPanel = panel;
		PanelSelected?.Invoke(panel);
	}

	private void TogglePicker()
	{
		pickerActive = !pickerActive;
		pickerButton?.SetClass("active", pickerActive);
	}

	public override void Tick()
	{
		base.Tick();

		// Handle picker mode
		if (pickerActive && targetRootPanel != null)
		{
			var mousePos = targetRootPanel.MousePos;
			var panelAtMouse = targetRootPanel.GetPanelAt(mousePos, true, false);
			
			if (panelAtMouse != hoveredPanel)
			{
				hoveredPanel = panelAtMouse;
				PanelHovered?.Invoke(hoveredPanel);
			}

			// Click to select in picker mode
			// This is simplified - in a real implementation you'd handle mouse input properly
		}

		// Auto-refresh tree periodically (simplified approach)
		// In production you'd use dirty flags or events
	}

	public override void OnDeleted()
	{
		base.OnDeleted();
		pickerActive = false;
	}
}
