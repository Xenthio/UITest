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
		
		Console.WriteLine("[PanelInspector] Opening inspector windows...");

		// Create panel list window if it doesn't exist
		if (panelListWindow == null || !panelListWindow.IsValid())
		{
			Console.WriteLine("[PanelInspector] Creating PanelListWindow...");
			panelListWindow = new PanelListWindow();
			panelListWindow.Parent = rootPanel; // Add to root panel!
			panelListWindow.SetTargetRootPanel(rootPanel);
			panelListWindow.PanelSelected += OnPanelSelected;
			panelListWindow.PanelHovered += OnPanelHovered;
			
			// Position on left side
			panelListWindow.Style.Left = 20;
			panelListWindow.Style.Top = 20;
			Console.WriteLine($"[PanelInspector] PanelListWindow created, IsValid={panelListWindow.IsValid()}");
		}

		// Create style inspector window if it doesn't exist
		if (styleInspectorWindow == null || !styleInspectorWindow.IsValid())
		{
			Console.WriteLine("[PanelInspector] Creating StyleInspectorWindow...");
			styleInspectorWindow = new StyleInspectorWindow();
			styleInspectorWindow.Parent = rootPanel; // Add to root panel!
			
			// Position on right side, next to panel list
			styleInspectorWindow.Style.Left = 440;
			styleInspectorWindow.Style.Top = 20;
			Console.WriteLine($"[PanelInspector] StyleInspectorWindow created, IsValid={styleInspectorWindow.IsValid()}");
		}
		
		Console.WriteLine("[PanelInspector] Inspector windows opened successfully");
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
