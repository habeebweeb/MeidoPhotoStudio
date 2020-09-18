namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal abstract class BaseMainWindowPane : BaseWindowPane
    {
        protected TabsPane tabsPane;
        public void SetTabsPane(TabsPane tabsPane) => this.tabsPane = tabsPane;
        /* Main window panes have panes within them while being a pane itself of the main window */
        public override void SetParent(BaseWindow window)
        {
            foreach (BasePane pane in Panes) pane.SetParent(window);
        }
    }
}
