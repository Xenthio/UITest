namespace Sandbox.UI;

/// <summary>
/// A panel that gets deleted automatically when clicked away from.
/// Ported from s&box's Sandbox.Engine/Systems/UI/Controls/BasePopup.cs
/// </summary>
public abstract class BasePopup : Panel
{
    static List<BasePopup> AllPopups = new();

    /// <summary>
    /// Stay open, even when CloseAll popups is called
    /// </summary>
    public bool StayOpen { get; set; }

    public static void CloseAll(Panel? exceptThisOne = null)
    {
        if (AllPopups.Count == 0)
            return;

        AllPopups.RemoveAll(x => !x.IsValid());

        BasePopup? floater = null;

        if (exceptThisOne is Panel flt)
        {
            floater = flt.AncestorsAndSelf.OfType<BasePopup>().FirstOrDefault();
        }

        foreach (var panel in AllPopups.ToArray())
        {
            if (panel == floater) continue;
            if (panel.StayOpen && panel.Parent.IsValid()) continue;

            try
            {
                AllPopups.Remove(panel);
                panel.Delete();
            }
            catch
            {
                // ignored
            }
        }
    }

    public BasePopup()
    {
        AllPopups.Add(this);
    }

    public override void OnDeleted()
    {
        base.OnDeleted();

        AllPopups.Remove(this);
    }
}
