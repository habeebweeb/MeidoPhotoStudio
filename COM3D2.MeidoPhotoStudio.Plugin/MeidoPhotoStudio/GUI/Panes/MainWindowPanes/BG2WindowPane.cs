namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BG2WindowPane : BaseMainWindowPane
    {
        private MeidoManager meidoManager;
        private PropManager propManager;
        private PropsPane propsPane;
        private AttachPropPane attachPropPane;
        private MyRoomPropsPane myRoomPropsPane;
        private ModPropsPane modPropsPane;
        private PropManagerPane propManagerPane;
        private SelectionGrid propTabs;
        private BasePane currentPropsPane;
        private static readonly string[] tabNames = { "props", "myRoom", "mod" };

        public BG2WindowPane(MeidoManager meidoManager, PropManager propManager)
        {
            this.meidoManager = meidoManager;
            this.propManager = propManager;
            this.propManager.DoguSelectChange += (s, a) => this.propTabs.SelectedItemIndex = 0;

            this.propsPane = AddPane(new PropsPane(propManager));
            this.myRoomPropsPane = AddPane(new MyRoomPropsPane(propManager));
            this.modPropsPane = AddPane(new ModPropsPane(propManager));
            this.attachPropPane = AddPane(new AttachPropPane(this.meidoManager, propManager));
            this.propManagerPane = AddPane(new PropManagerPane(propManager));

            this.propTabs = new SelectionGrid(Translation.GetArray("propsPaneTabs", tabNames));
            this.propTabs.ControlEvent += (s, a) =>
            {
                currentPropsPane = this.Panes[this.propTabs.SelectedItemIndex];
            };
            this.currentPropsPane = this.Panes[0];
        }

        protected override void ReloadTranslation()
        {
            this.propTabs.SetItems(Translation.GetArray("propsPaneTabs", tabNames));
        }

        public override void Draw()
        {
            this.tabsPane.Draw();
            this.propTabs.Draw();
            MiscGUI.WhiteLine();
            this.currentPropsPane.Draw();
            if (this.propTabs.SelectedItemIndex == 0)
            {
                this.propManagerPane.Draw();
                this.attachPropPane.Draw();
            }
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                base.UpdatePanes();
            }
        }
    }
}
