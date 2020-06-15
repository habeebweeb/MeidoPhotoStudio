using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class BGWindowPane : BaseWindowPane
    {
        private BackgroundSelectorPane backgroundSelectorPane;
        private PropsPane propsPane;
        // private LightsPane lightsPane;

        public BGWindowPane(EnvironmentManager environmentManager)
        {
            this.backgroundSelectorPane = new BackgroundSelectorPane(environmentManager);
            this.propsPane = new PropsPane(environmentManager.PropManager);
            // this.lightsPane = new LightsPane(environmentManager.LightManager);
        }
        public override void Draw()
        {
            this.backgroundSelectorPane.Draw();
            this.propsPane.Draw();
            // this.lightsPane.Draw();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                this.propsPane.UpdatePane();
                // this.lightsPane.UpdatePane();
            }
        }
    }
}
