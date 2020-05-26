using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class Background2Window : BaseMainWindow
    {
        EnvironmentManager environmentManager;
        public Background2Window(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
        }
        public override void Draw(params GUILayoutOption[] layoutOptions)
        {

            GUILayout.Label("bg2");
        }
    }
}
