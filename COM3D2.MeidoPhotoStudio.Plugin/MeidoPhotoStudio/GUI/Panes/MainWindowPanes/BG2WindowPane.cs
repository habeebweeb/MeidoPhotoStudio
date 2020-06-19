using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BG2WindowPane : BaseWindowPane
    {
        EnvironmentManager environmentManager;
        public BG2WindowPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
        }
        public override void Draw()
        {

            GUILayout.Label("bg2");
        }
    }
}
