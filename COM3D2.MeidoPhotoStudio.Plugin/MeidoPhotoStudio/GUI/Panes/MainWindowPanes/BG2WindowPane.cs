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

        public BG2WindowPane(MeidoManager meidoManager, EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
            this.meidoManager = meidoManager;

            this.propsPane = new PropsPane(this.environmentManager.PropManager);
            this.attachPropPane = new AttachPropPane(this.meidoManager, this.environmentManager.PropManager);
        }

        public override void Draw()
        {
            this.propsPane.Draw();
            this.attachPropPane.Draw();
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
