using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class TabsPane : BasePane
{
    private static readonly Constants.Window[] Tabs =
    [
        Constants.Window.Call,
        Constants.Window.Pose,
        Constants.Window.BG,
        Constants.Window.BG2,
    ];

    private readonly SelectionGrid tabs;

    private Constants.Window selectedTab;

    public TabsPane()
    {
        tabs = new(Translation.GetArray("tabs", Tabs.Select(tab => tab.ToString())));
        tabs.ControlEvent += (_, _) =>
            OnChangeTab();
    }

    public event EventHandler TabChange;

    public Constants.Window SelectedTab
    {
        get => selectedTab;
        set
        {
            var newTab = value;

            if (value is Constants.Window.Face)
                newTab = Constants.Window.Pose;

            var tabIndex = Array.IndexOf(Tabs, newTab);

            if (tabIndex < 0)
                return;

            tabs.SelectedItemIndex = tabIndex;
        }
    }

    public override void Draw()
    {
        tabs.Draw();
        MpsGui.BlackLine();
    }

    protected override void ReloadTranslation() =>
        tabs.SetItemsWithoutNotify(
            Translation.GetArray("tabs", Tabs.Select(tab => tab.ToLower())), tabs.SelectedItemIndex);

    private void OnChangeTab()
    {
        selectedTab = Tabs[tabs.SelectedItemIndex];
        TabChange?.Invoke(null, EventArgs.Empty);
    }
}
