using MeidoPhotoStudio.Plugin.Core.Camera;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BGWindowPane : BaseMainWindowPane
{
    private readonly BackgroundSelectorPane backgroundSelectorPane;
    private readonly CameraPane cameraPane;
    private readonly LightsPane lightsPane;
    private readonly EffectsPane effectsPane;
    private readonly DragPointPane dragPointPane;
    private readonly OtherEffectsPane otherEffectsPane;
    private readonly Button sceneManagerButton;

    public BGWindowPane(
        EnvironmentManager environmentManager,
        LightManager lightManager,
        EffectManager effectManager,
        SceneWindow sceneWindow,
        CameraController cameraManager,
        CameraSaveSlotController cameraSaveSlotController)
    {
        sceneManagerButton = new(Translation.Get("backgroundWindow", "manageScenesButton"));
        sceneManagerButton.ControlEvent += (_, _) =>
            sceneWindow.Visible = !sceneWindow.Visible;

        backgroundSelectorPane = AddPane(new BackgroundSelectorPane(environmentManager));
        cameraPane = AddPane(new CameraPane(cameraManager, cameraSaveSlotController));
        dragPointPane = AddPane(new DragPointPane());
        lightsPane = AddPane(new LightsPane(lightManager));
        effectsPane = AddPane(new EffectsPane()
        {
            ["bloom"] = new BloomPane(effectManager),
            ["dof"] = new DepthOfFieldPane(effectManager),
            ["vignette"] = new VignettePane(effectManager),
            ["fog"] = new FogPane(effectManager),
        });

        otherEffectsPane = AddPane(new OtherEffectsPane(effectManager));
    }

    public override void Draw()
    {
        tabsPane.Draw();
        sceneManagerButton.Draw();
        backgroundSelectorPane.Draw();
        dragPointPane.Draw();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        cameraPane.Draw();
        lightsPane.Draw();
        effectsPane.Draw();
        otherEffectsPane.Draw();

        GUILayout.EndScrollView();
    }

    public override void UpdatePanes()
    {
        if (ActiveWindow)
            base.UpdatePanes();
    }

    protected override void ReloadTranslation() =>
        sceneManagerButton.Label = Translation.Get("backgroundWindow", "manageScenesButton");
}
