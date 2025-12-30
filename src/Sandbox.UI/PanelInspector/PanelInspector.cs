namespace Sandbox.UI.PanelInspector;

/// <summary>
/// Main panel inspector that combines the panel list and style inspector.
/// Provides a complete debugging interface for UI panels.
/// Based on s&box's PanelInspector (MIT Licensed)
/// </summary>
public class PanelInspector
{
	private PanelListWindow? panelListWindow;
	private StyleInspectorWindow? styleInspectorWindow;
	private RootPanel? targetRootPanel;

	public PanelInspector()
	{
	}

	/// <summary>
	/// Open the panel inspector windows.
	/// </summary>
	/// <param name="rootPanel">The root panel to inspect</param>
	public void Open(RootPanel rootPanel)
	{
		targetRootPanel = rootPanel;

		// Create panel list window if it doesn't exist
		if (panelListWindow == null || !panelListWindow.IsValid())
		{
			panelListWindow = new PanelListWindow();
			panelListWindow.SetTargetRootPanel(rootPanel);
			panelListWindow.PanelSelected += OnPanelSelected;
			panelListWindow.PanelHovered += OnPanelHovered;
			
			// Position on left side
			panelListWindow.Style.Left = 20;
			panelListWindow.Style.Top = 20;
		}

		// Create style inspector window if it doesn't exist
		if (styleInspectorWindow == null || !styleInspectorWindow.IsValid())
		{
			styleInspectorWindow = new StyleInspectorWindow();
			
			// Position on right side, next to panel list
			styleInspectorWindow.Style.Left = 440;
			styleInspectorWindow.Style.Top = 20;
		}
	}

	/// <summary>
	/// Close the panel inspector windows.
	/// </summary>
	public void Close()
	{
		panelListWindow?.Delete();
		panelListWindow = null;

		styleInspectorWindow?.Delete();
		styleInspectorWindow = null;
	}

	/// <summary>
	/// Toggle the panel inspector windows.
	/// </summary>
	public void Toggle(RootPanel rootPanel)
	{
		if (IsOpen())
			Close();
		else
			Open(rootPanel);
	}

	/// <summary>
	/// Check if the inspector is currently open.
	/// </summary>
	public bool IsOpen()
	{
		return panelListWindow != null && panelListWindow.IsValid();
	}

	private void OnPanelSelected(Panel? panel)
	{
		styleInspectorWindow?.SetSelectedPanel(panel);
	}

	private void OnPanelHovered(Panel? panel)
	{
		// Could highlight the panel in the UI here
	}

	/// <summary>
	/// Update the inspector (call this every frame if the inspector is open).
	/// </summary>
	public void Update()
	{
		// Windows handle their own updates in Tick()
	}
}
