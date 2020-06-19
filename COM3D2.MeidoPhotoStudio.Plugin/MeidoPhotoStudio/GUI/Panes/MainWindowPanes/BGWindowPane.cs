using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BGWindowPane : BaseWindowPane
    {
        private BackgroundSelectorPane backgroundSelectorPane;
        private PropsPane propsPane;
        private LightsPane lightsPane;
        private EffectsPane effectsPane;

        public BGWindowPane(EnvironmentManager environmentManager)
        {
            this.backgroundSelectorPane = new BackgroundSelectorPane(environmentManager);
            this.propsPane = new PropsPane(environmentManager.PropManager);
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
            this.propsPane.Draw();
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            this.lightsPane.Draw();
            this.effectsPane.Draw();
            GUILayout.EndScrollView();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow)
            {
                this.propsPane.UpdatePane();
                this.lightsPane.UpdatePane();
                this.effectsPane.UpdatePane();
            }
        }
    }
}
