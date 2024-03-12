using System;

using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class SceneLoader
{
    private readonly ISceneAspectLoader<MessageWindowSchema> messageAspectLoader;
    private readonly ISceneAspectLoader<CameraSchema> cameraAspectLoader;
    private readonly ISceneAspectLoader<LightRepositorySchema> lightingAspectLoader;
    private readonly ISceneAspectLoader<EffectsSchema> effectsAspectLoader;
    private readonly ISceneAspectLoader<BackgroundSchema> backgroundAspectLoader;
    private readonly ISceneAspectLoader<PropsSchema> propsAspectLoader;

    public SceneLoader(
        ISceneAspectLoader<MessageWindowSchema> messageAspectLoader,
        ISceneAspectLoader<CameraSchema> cameraAspectLoader,
        ISceneAspectLoader<LightRepositorySchema> lightingAspectLoader,
        ISceneAspectLoader<EffectsSchema> effectsAspectLoader,
        ISceneAspectLoader<BackgroundSchema> backgroundAspectLoader,
        ISceneAspectLoader<PropsSchema> propsAspectLoader)
    {
        this.messageAspectLoader = messageAspectLoader ?? throw new ArgumentNullException(nameof(messageAspectLoader));
        this.cameraAspectLoader = cameraAspectLoader ?? throw new ArgumentNullException(nameof(cameraAspectLoader));
        this.lightingAspectLoader = lightingAspectLoader ?? throw new ArgumentNullException(nameof(lightingAspectLoader));
        this.effectsAspectLoader = effectsAspectLoader ?? throw new ArgumentNullException(nameof(effectsAspectLoader));
        this.backgroundAspectLoader = backgroundAspectLoader ?? throw new ArgumentNullException(nameof(backgroundAspectLoader));
        this.propsAspectLoader = propsAspectLoader ?? throw new ArgumentNullException(nameof(propsAspectLoader));
    }

    public void LoadScene(SceneSchema sceneSchema, LoadOptions loadOptions)
    {
        if (sceneSchema is null)
            throw new ArgumentNullException(nameof(sceneSchema));

        messageAspectLoader.Load(sceneSchema.MessageWindow, loadOptions);
        cameraAspectLoader.Load(sceneSchema.Camera, loadOptions);
        lightingAspectLoader.Load(sceneSchema.Lights, loadOptions);
        effectsAspectLoader.Load(sceneSchema.Effects, loadOptions);
        backgroundAspectLoader.Load(sceneSchema.Background, loadOptions);
        propsAspectLoader.Load(sceneSchema.Props, loadOptions);
    }
}
