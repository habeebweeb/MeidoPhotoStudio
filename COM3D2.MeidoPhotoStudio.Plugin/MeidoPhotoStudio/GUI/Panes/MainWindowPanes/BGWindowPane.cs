using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BGWindowPane : BaseWindowPane
    {
        private BackgroundSelectorPane backgroundSelectorPane;
        private LightsPane lightsPane;
        private EffectsPane effectsPane;
        private DragPointPane dragPointPane;

        public BGWindowPane(EnvironmentManager environmentManager)
        {
            this.backgroundSelectorPane = new BackgroundSelectorPane(environmentManager);
            this.dragPointPane = new DragPointPane();
            this.lightsPane = new LightsPane(environmentManager);

            EffectManager effectManager = environmentManager.EffectManager;

            this.effectsPane = new EffectsPane()
            {
                ["bloom"] = new BloomPane(effectManager),
                ["dof"] = new DepthOfFieldPane(effectManager),
                ["vignette"] = new VignettePane(effectManager),
                ["fog"] = new FogPane(effectManager)
            };
        }

        public override void Draw()
        {
            this.backgroundSelectorPane.Draw();
            this.dragPointPane.Draw();
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            this.lightsPane.Draw();
            this.effectsPane.Draw();
            GUILayout.EndScrollView();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                this.lightsPane.UpdatePane();
                this.effectsPane.UpdatePane();
            }
        }
    }
}
