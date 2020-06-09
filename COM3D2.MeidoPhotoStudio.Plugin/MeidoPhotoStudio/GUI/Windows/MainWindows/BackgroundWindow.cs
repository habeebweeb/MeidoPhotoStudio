using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class BackgroundWindow : BaseMainWindow
    {
        private EnvironmentManager environmentManager;
        private BackgroundSelectorPane backgroundSelectorPane;
        private PropsPane propsPane;

        public BackgroundWindow(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
            this.backgroundSelectorPane = new BackgroundSelectorPane(this.environmentManager);
            this.propsPane = new PropsPane(this.environmentManager);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            this.backgroundSelectorPane.Draw();
            this.propsPane.Draw();
        }
    }
}
