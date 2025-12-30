using System.Linq;

namespace Sandbox.UI;

/// <summary>
/// A container with tabs, allowing you to switch between different sheets.
/// You can position the tabs by adding the class tabs-bottom, tabs-left, tabs-right (default is tabs-top)
/// Based on XGUI-3 TabContainer.
/// </summary>
[Library("tabcontrol"), Alias("tabcontainer", "tabs")]
public class TabControl : Panel
{
    /// <summary>
    /// A control housing the tabs
    /// </summary>
    public Panel TabsContainer { get; protected set; }

    /// <summary>
    /// A control housing the sheets
    /// </summary>
    public Panel SheetContainer { get; protected set; }

    /// <summary>
    /// Access to the tabs on this control
    /// </summary>
    public List<TabInfo> Tabs = new();

    /// <summary>
    /// If a cookie is set then the selected tab will be saved and restored.
    /// TODO: Implement cookie support when available
    /// </summary>
    public string TabCookie { get; set; } = "";

    /// <summary>
    /// If true we will act as a tab bar and have no body.
    /// </summary>
    public bool NoBody
    {
        get => SheetContainer.Style.Display == DisplayMode.None;
        set
        {
            SheetContainer.Style.Display = value ? DisplayMode.None : DisplayMode.Flex;
        }
    }

    private string _activeTab = "";
    
    /// <summary>
    /// The tab that is active
    /// </summary>
    public string ActiveTab
    {
        get => _activeTab;
        set
        {
            if (_activeTab == value) return;
            _activeTab = value;

            var t = Tabs.FirstOrDefault(x => x.TabName == _activeTab);
            if (t != null)
                SwitchTab(t);
        }
    }

    public TabControl()
    {
        // ElementName is already set to "tabcontrol" by base constructor
        // and automatically added as a CSS class
        ElementName = "tabcontrol";

        TabsContainer = AddChild(new Panel(this, "tabs"));
        TabsContainer.AddClass("tabs");
        
        SheetContainer = AddChild(new Panel(this, "sheets"));
        SheetContainer.AddClass("sheets");
    }

    public override void SetProperty(string name, string value)
    {
        if (name == "cookie")
        {
            TabCookie = value;
            return;
        }

        if (name == "nobody")
        {
            NoBody = value == "true" || value == "1";
            return;
        }

        if (name == "activetab")
        {
            ActiveTab = value;
            return;
        }

        base.SetProperty(name, value);
    }

    /// <summary>
    /// Add a tab to the sheet.
    /// </summary>
    public TabInfo AddTab(Panel panel, string tabName, string title, string icon = null)
    {
        var index = Tabs.Count;

        var tab = new TabInfo(this, title, icon, panel);
        tab.TabName = tabName;

        Tabs.Add(tab);

        // TODO: Cookie support when implemented
        // var cookieIndex = string.IsNullOrWhiteSpace(TabCookie) ? -1 : GetCookie($"dropdown.{TabCookie}", -1);

        panel.Parent = SheetContainer;

        if (index == 0) // || cookieIndex == index
        {
            SwitchTab(tab, false);
        }
        else
        {
            tab.Active = false;
        }

        return tab;
    }

    public override void OnTemplateSlot(Html.Node element, string slotName, Panel panel)
    {
        if (slotName == "tab")
        {
            AddTab(panel, 
                element.GetAttribute("tabname", null), 
                element.GetAttribute("tabtext", null), 
                element.GetAttribute("tabicon", null));
            return;
        }

        base.OnTemplateSlot(element, slotName, panel);
    }

    /// <summary>
    /// Switch to a specific tab.
    /// </summary>
    public void SwitchTab(TabInfo tab, bool setCookie = true)
    {
        if (tab == null) return;
        
        _activeTab = tab.TabName;

        foreach (var page in Tabs)
        {
            page.Active = page == tab;
        }

        // TODO: Cookie support when implemented
        // if (setCookie && !string.IsNullOrEmpty(TabCookie))
        // {
        //     SetCookie($"dropdown.{TabCookie}", Tabs.IndexOf(tab));
        // }
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

            // Register tabs through the slot system
            foreach (var tab in tabs)
            {
                // The tab will be reparented via OnTemplateSlot
                // This happens automatically through the render tree processing
            }
        }
    }

    /// <summary>
    /// Holds a Tab button and a Page for each sheet on the TabControl.
    /// </summary>
    public class TabInfo
    {
        private TabControl Parent;
        public Button Button { get; protected set; }
        public Panel Page { get; protected set; }
        public string TabName { get; set; } = "";

        public TabInfo(TabControl tabControl, string title, string icon, Panel panel)
        {
            Parent = tabControl;
            Page = panel;

            Button = new Button();
            Button.Text = title;
            Button.AddClass("button");
            if (!string.IsNullOrEmpty(icon))
            {
                Button.Icon = icon;
            }
            Button.OnClick += () => Parent?.SwitchTab(this, true);
            Button.Parent = tabControl.TabsContainer;
        }

        private bool active;

        /// <summary>
        /// Change appearance based on active status
        /// </summary>
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                Button.SetClass("active", value);
                Page.SetClass("active", value);
                Page.Style.Display = value ? DisplayMode.Flex : DisplayMode.None;
            }
        }
    }
}
