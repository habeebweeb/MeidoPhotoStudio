using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BG2WindowPane : BaseWindowPane
    {
        private EnvironmentManager environmentManager;
        private MeidoManager meidoManager;
        private PropsPane propsPane;
        private AttachPropPane attachPropPane;
        private MyRoomPropsPane myRoomPropsPane;
        private SelectionGrid propTabs;
        private BasePane currentPropsPane;

        public BG2WindowPane(MeidoManager meidoManager, EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
            this.meidoManager = meidoManager;

            PropManager propManager = this.environmentManager.PropManager;
            this.propsPane = new PropsPane(propManager);
            this.myRoomPropsPane = new MyRoomPropsPane(propManager);
            this.attachPropPane = new AttachPropPane(this.meidoManager, propManager);

            this.Panes.Add(propsPane);
            this.Panes.Add(myRoomPropsPane);

            this.propTabs = new SelectionGrid(Translation.GetArray("propTabs", new[] { "Props", "MyRoom", "Mod" }));
            this.propTabs.ControlEvent += (s, a) =>
            {
                currentPropsPane = this.Panes[this.propTabs.SelectedItemIndex];
            };
            this.currentPropsPane = this.Panes[0];
        }

        public override void Draw()
        {
            this.propTabs.Draw();
            MiscGUI.WhiteLine();
            this.currentPropsPane.Draw();
            if (this.propTabs.SelectedItemIndex == 0) this.attachPropPane.Draw();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                this.propsPane.UpdatePane();
                this.attachPropPane.UpdatePane();
            }
        }
    }
}
