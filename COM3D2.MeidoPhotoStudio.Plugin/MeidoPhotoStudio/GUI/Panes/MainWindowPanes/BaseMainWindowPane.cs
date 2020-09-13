namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BaseMainWindowPane : BaseWindowPane
    {
        protected TabsPane tabsPane;
        public void SetTabsPane(TabsPane tabsPane) => this.tabsPane = tabsPane;
    }
}
