using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BG2WindowPane : BaseWindowPane
    {
        private PropsPane propsPane;
        EnvironmentManager environmentManager;
        public BG2WindowPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;

            this.propsPane = new PropsPane(this.environmentManager.PropManager);
        }
        public override void Draw()
        {
            this.propsPane.Draw();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                this.propsPane.UpdatePane();
            }
        }
    }
}
