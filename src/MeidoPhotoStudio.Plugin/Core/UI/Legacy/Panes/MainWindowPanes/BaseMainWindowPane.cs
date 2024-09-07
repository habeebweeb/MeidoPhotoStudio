using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public abstract class BaseMainWindowPane : BaseWindow
{
    protected TabsPane tabsPane;

    public void SetTabsPane(TabsPane tabsPane) =>
        this.tabsPane = tabsPane;
}
