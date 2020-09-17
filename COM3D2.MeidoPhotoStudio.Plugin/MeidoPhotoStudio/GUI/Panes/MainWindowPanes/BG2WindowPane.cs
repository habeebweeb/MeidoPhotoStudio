namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BG2WindowPane : BaseMainWindowPane
    {
        private static readonly string[] tabNames = { "props", "myRoom", "mod" };
        private readonly MeidoManager meidoManager;
        private readonly PropManager propManager;
        private readonly AttachPropPane attachPropPane;
        private readonly PropManagerPane propManagerPane;
        private readonly SelectionGrid propTabs;
        private BasePane currentPropsPane;

        public BG2WindowPane(MeidoManager meidoManager, PropManager propManager)
        {
            this.meidoManager = meidoManager;
            this.propManager = propManager;
            this.propManager.DoguSelectChange += (s, a) => propTabs.SelectedItemIndex = 0;

            // should be added in this order
            AddPane(new PropsPane(propManager));
            AddPane(new MyRoomPropsPane(propManager));
            AddPane(new ModPropsPane(propManager));

            attachPropPane = AddPane(new AttachPropPane(this.meidoManager, propManager));
            propManagerPane = AddPane(new PropManagerPane(propManager));

            propTabs = new SelectionGrid(Translation.GetArray("propsPaneTabs", tabNames));
            propTabs.ControlEvent += (s, a) => currentPropsPane = Panes[propTabs.SelectedItemIndex];
            currentPropsPane = Panes[0];
        }

        protected override void ReloadTranslation()
        {
            propTabs.SetItems(Translation.GetArray("propsPaneTabs", tabNames));
        }

        public override void Draw()
        {
            tabsPane.Draw();
            propTabs.Draw();
            MiscGUI.WhiteLine();
            currentPropsPane.Draw();
            if (propTabs.SelectedItemIndex == 0)
            {
                propManagerPane.Draw();
                attachPropPane.Draw();
            }
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow) base.UpdatePanes();
        }
    }
}
