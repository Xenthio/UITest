namespace Sandbox.UI;

/// <summary>
/// A single tab within a TabControl.
/// Based on S&box UI patterns.
/// </summary>
[Library("tab")]
public class Tab : Panel
{
    private string _tabName = "";
    private string _tabText = "";

    /// <summary>
    /// The unique identifier for this tab
    /// </summary>
    public string TabName
    {
        get => _tabName;
        set => _tabName = value;
    }

    /// <summary>
    /// The text displayed on the tab button
    /// </summary>
    public string TabText
    {
        get => _tabText;
        set => _tabText = value;
    }

    /// <summary>
    /// The button in the tab bar representing this tab
    /// </summary>
    public Button? TabButton { get; set; }

    public Tab()
    {
        AddClass("tab");
        ElementName = "tab";
    }

    public override void SetProperty(string name, string value)
    {
        switch (name)
        {
            case "tabname":
                TabName = value;
                return;

            case "tabtext":
                TabText = value;
                if (TabButton != null)
                    TabButton.Text = value;
                return;

            case "slot":
                // Ignore slot attribute - it's used for positioning in markup
                return;
        }

        base.SetProperty(name, value);
    }
}
