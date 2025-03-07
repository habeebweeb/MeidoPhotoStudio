using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Extension;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using MeidoPhotoStudio.Plugin.Core.UndoRedo;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class SceneLoader(
    UndoRedoService undoRedoService,
    ISceneAspectLoader<CharactersSchema> characterAspectLoader,
    ISceneAspectLoader<MessageWindowSchema> messageAspectLoader,
    ISceneAspectLoader<CameraSchema> cameraAspectLoader,
    ISceneAspectLoader<LightsSchema> lightingAspectLoader,
    ISceneAspectLoader<EffectsSchema> effectsAspectLoader,
    ISceneAspectLoader<BackgroundSchema> backgroundAspectLoader,
    ISceneAspectLoader<PropsSchema> propsAspectLoader,
    ISceneAspectLoader<ExtensionSchema> extensionAspectLoader)
{
    private readonly UndoRedoService undoRedoService = undoRedoService
        ?? throw new ArgumentNullException(nameof(undoRedoService));

    private readonly ISceneAspectLoader<CharactersSchema> characterAspectLoader = characterAspectLoader
        ?? throw new ArgumentNullException(nameof(characterAspectLoader));

    private readonly ISceneAspectLoader<MessageWindowSchema> messageAspectLoader = messageAspectLoader
        ?? throw new ArgumentNullException(nameof(messageAspectLoader));

    private readonly ISceneAspectLoader<CameraSchema> cameraAspectLoader = cameraAspectLoader
        ?? throw new ArgumentNullException(nameof(cameraAspectLoader));

    private readonly ISceneAspectLoader<LightsSchema> lightingAspectLoader = lightingAspectLoader
        ?? throw new ArgumentNullException(nameof(lightingAspectLoader));

    private readonly ISceneAspectLoader<EffectsSchema> effectsAspectLoader = effectsAspectLoader
        ?? throw new ArgumentNullException(nameof(effectsAspectLoader));

    private readonly ISceneAspectLoader<BackgroundSchema> backgroundAspectLoader = backgroundAspectLoader
        ?? throw new ArgumentNullException(nameof(backgroundAspectLoader));

    private readonly ISceneAspectLoader<PropsSchema> propsAspectLoader = propsAspectLoader
        ?? throw new ArgumentNullException(nameof(propsAspectLoader));

    private readonly ISceneAspectLoader<ExtensionSchema> extensionAspectLoader = extensionAspectLoader
        ?? throw new ArgumentNullException(nameof(extensionAspectLoader));

    public void LoadScene(SceneSchema sceneSchema, LoadOptions loadOptions)
    {
        if (sceneSchema is null)
            throw new ArgumentNullException(nameof(sceneSchema));

        characterAspectLoader.Load(sceneSchema.Character, loadOptions);
        messageAspectLoader.Load(sceneSchema.MessageWindow, loadOptions);
        cameraAspectLoader.Load(sceneSchema.Camera, loadOptions);
        effectsAspectLoader.Load(sceneSchema.Effects, loadOptions);
        backgroundAspectLoader.Load(sceneSchema.Background, loadOptions);
        lightingAspectLoader.Load(sceneSchema.Lights, loadOptions);
        propsAspectLoader.Load(sceneSchema.Props, loadOptions);
        extensionAspectLoader.Load(sceneSchema.Extension, loadOptions);

        undoRedoService.Clear();
    }
}
