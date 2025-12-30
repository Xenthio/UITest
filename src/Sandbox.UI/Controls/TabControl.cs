namespace Sandbox.UI;

/// <summary>
/// A tab control for organizing content in multiple panels.
/// Based on S&box UI patterns.
/// </summary>
[Library("tabcontrol")]
public class TabControl : Panel
{
    /// <summary>
    /// The tab bar that contains the tab buttons
    /// </summary>
    public Panel? TabBar { get; protected set; }

    /// <summary>
    /// The content area where tab panels are displayed
    /// </summary>
    public Panel? ContentArea { get; protected set; }

    /// <summary>
    /// The currently active tab
    /// </summary>
    public Tab? ActiveTab { get; protected set; }

    public TabControl()
    {
        AddClass("tabcontrol");
        ElementName = "tabcontrol";

        TabBar = AddChild(new Panel(this, "tabbar"));
        ContentArea = AddChild(new Panel(this, "tabcontent"));
    }

    /// <summary>
    /// Called when a tab is added
    /// </summary>
    public void RegisterTab(Tab tab)
    {
        if (tab == null) return;

        // Create the tab button in the tab bar
        if (TabBar != null)
        {
            var tabButton = TabBar.AddChild(new Button());
            tabButton.AddClass("tab");
            tabButton.Text = tab.TabText ?? "Tab";
            tabButton.OnClick += () => SetActiveTab(tab);
            tab.TabButton = tabButton;
        }

        // If this is the first tab, make it active
        if (ActiveTab == null)
        {
            SetActiveTab(tab);
        }
        else
        {
            // Hide the tab content by default
            tab.Style.Display = DisplayMode.None;
        }
    }

    /// <summary>
    /// Set the active tab
    /// </summary>
    public void SetActiveTab(Tab tab)
    {
        if (tab == null) return;

        // Hide all tabs
        foreach (var child in Children)
        {
            if (child is Tab t)
            {
                t.Style.Display = DisplayMode.None;
                t.TabButton?.RemoveClass("active");
            }
        }

        // Show the selected tab
        tab.Style.Display = DisplayMode.Flex;
        tab.TabButton?.AddClass("active");
        ActiveTab = tab;
    }

    /// <summary>
    /// Set the active tab by name
    /// </summary>
    public void SetActiveTab(string tabName)
    {
        foreach (var child in Children)
        {
            if (child is Tab t && t.TabName == tabName)
            {
                SetActiveTab(t);
                return;
            }
        }
    }

    protected override void OnAfterTreeRender(bool firstTime)
    {
        base.OnAfterTreeRender(firstTime);

        if (firstTime)
        {
            // Find all tab children and register them
            var tabs = new List<Tab>();
            foreach (var child in Children)
            {
                if (child is Tab tab)
                {
                    tabs.Add(tab);
                }
            }

            // Move tabs to content area and register them
            foreach (var tab in tabs)
            {
                // Reparent tab to content area
                tab.Parent = null; // Removes from current parent
                
                // Add to content area
                if (ContentArea != null)
                {
                    ContentArea.AddChild(tab);
                }
                
                RegisterTab(tab);
            }
        }
    }
}
