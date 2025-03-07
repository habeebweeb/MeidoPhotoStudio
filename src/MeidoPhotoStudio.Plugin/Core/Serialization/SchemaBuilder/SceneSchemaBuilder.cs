using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Extension;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Schema.Message;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SceneSchemaBuilder(
    ISceneSchemaAspectBuilder<CharactersSchema> charactersSchemaBuilder,
    ISceneSchemaAspectBuilder<MessageWindowSchema> messageWindowSchemaBuilder,
    ISceneSchemaAspectBuilder<CameraSchema> cameraSchemaBuilder,
    ISceneSchemaAspectBuilder<LightsSchema> lightsSchemaBuilder,
    ISceneSchemaAspectBuilder<EffectsSchema> effectsSchemaBuilder,
    ISceneSchemaAspectBuilder<BackgroundSchema> backgroundSchemaBuilder,
    ISceneSchemaAspectBuilder<PropsSchema> propsSchemaBuilder,
    ISceneSchemaAspectBuilder<ExtensionSchema> extensionSchemaBuilder)
{
    private readonly ISceneSchemaAspectBuilder<CharactersSchema> charactersSchemaBuilder = charactersSchemaBuilder
        ?? throw new ArgumentNullException(nameof(charactersSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<MessageWindowSchema> messageWindowSchemaBuilder = messageWindowSchemaBuilder
        ?? throw new ArgumentNullException(nameof(messageWindowSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<CameraSchema> cameraSchemaBuilder = cameraSchemaBuilder
        ?? throw new ArgumentNullException(nameof(cameraSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<LightsSchema> lightsSchemaBuilder = lightsSchemaBuilder
        ?? throw new ArgumentNullException(nameof(lightsSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<EffectsSchema> effectsSchemaBuilder = effectsSchemaBuilder
        ?? throw new ArgumentNullException(nameof(effectsSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<BackgroundSchema> backgroundSchemaBuilder = backgroundSchemaBuilder
        ?? throw new ArgumentNullException(nameof(backgroundSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<PropsSchema> propsSchemaBuilder = propsSchemaBuilder
        ?? throw new ArgumentNullException(nameof(propsSchemaBuilder));

    private readonly ISceneSchemaAspectBuilder<ExtensionSchema> extensionSchemaBuilder = extensionSchemaBuilder
        ?? throw new ArgumentNullException(nameof(extensionSchemaBuilder));

    public SceneSchema Build() =>
        new()
        {
            Character = charactersSchemaBuilder.Build(),
            MessageWindow = messageWindowSchemaBuilder.Build(),
            Camera = cameraSchemaBuilder.Build(),
            Lights = lightsSchemaBuilder.Build(),
            Effects = effectsSchemaBuilder.Build(),
            Background = backgroundSchemaBuilder.Build(),
            Props = propsSchemaBuilder.Build(),
            Extension = extensionSchemaBuilder.Build(),
        };
}
