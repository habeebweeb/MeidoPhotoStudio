using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BGWindowPane : BaseWindowPane
    {
        private BackgroundSelectorPane backgroundSelectorPane;
        private LightsPane lightsPane;
        private EffectsPane effectsPane;
        private DragPointPane dragPointPane;
        private Button sceneManagerButton;

        public BGWindowPane(
            EnvironmentManager environmentManager, LightManager lightManager, EffectManager effectManager,
            SceneWindow sceneWindow
        )
        {
            this.sceneManagerButton = new Button("Manage Scenes");
            this.sceneManagerButton.ControlEvent += (s, a) => sceneWindow.Visible = !sceneWindow.Visible;

            this.backgroundSelectorPane = AddPane(new BackgroundSelectorPane(environmentManager));
            this.dragPointPane = AddPane(new DragPointPane());
            this.lightsPane = AddPane(new LightsPane(lightManager));

            this.effectsPane = AddPane(new EffectsPane()
            {
                ["bloom"] = new BloomPane(effectManager),
                ["dof"] = new DepthOfFieldPane(effectManager),
                ["vignette"] = new VignettePane(effectManager),
                ["fog"] = new FogPane(effectManager)
            });
        }

        public override void Draw()
        {
            this.sceneManagerButton.Draw();
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
                base.UpdatePanes();
            }
        }
    }
}
