namespace MeidoPhotoStudio.Plugin;

public abstract class BaseMainWindowPane : BaseWindow
{
    protected TabsPane tabsPane;

    public void SetTabsPane(TabsPane tabsPane) =>
        this.tabsPane = tabsPane;
}
