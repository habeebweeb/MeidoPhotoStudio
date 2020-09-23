using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class BGWindowPane : BaseMainWindowPane
    {
        private readonly BackgroundSelectorPane backgroundSelectorPane;
        private readonly LightsPane lightsPane;
        private readonly EffectsPane effectsPane;
        private readonly DragPointPane dragPointPane;
        private readonly OtherEffectsPane otherEffectsPane;
        private readonly Button sceneManagerButton;

        public BGWindowPane(
            EnvironmentManager environmentManager, LightManager lightManager, EffectManager effectManager,
            SceneWindow sceneWindow
        )
        {
            sceneManagerButton = new Button(Translation.Get("backgroundWindow", "manageScenesButton"));
            sceneManagerButton.ControlEvent += (s, a) => sceneWindow.Visible = !sceneWindow.Visible;

            backgroundSelectorPane = AddPane(new BackgroundSelectorPane(environmentManager));
            dragPointPane = AddPane(new DragPointPane());
            lightsPane = AddPane(new LightsPane(lightManager));

            effectsPane = AddPane(new EffectsPane()
            {
                ["bloom"] = new BloomPane(effectManager),
                ["dof"] = new DepthOfFieldPane(effectManager),
                ["vignette"] = new VignettePane(effectManager),
                ["fog"] = new FogPane(effectManager)
            });

            otherEffectsPane = AddPane(new OtherEffectsPane(effectManager));
        }

        protected override void ReloadTranslation()
        {
            sceneManagerButton.Label = Translation.Get("backgroundWindow", "manageScenesButton");
        }

        public override void Draw()
        {
            tabsPane.Draw();
            sceneManagerButton.Draw();
            backgroundSelectorPane.Draw();
            dragPointPane.Draw();
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            lightsPane.Draw();
            effectsPane.Draw();
            otherEffectsPane.Draw();
            GUILayout.EndScrollView();
        }

        public override void UpdatePanes()
        {
            if (ActiveWindow) base.UpdatePanes();
        }
    }
}
