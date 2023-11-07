using System;

using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Schema;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class SceneSchemaBuilder
{
    private readonly MessageWindowManager messageWindowManager;
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly LightRepository lightRepository;
    private readonly EffectManager effectManager;
    private readonly BackgroundService backgroundService;

    public SceneSchemaBuilder(
        MessageWindowManager messageWindowManager,
        CameraSaveSlotController cameraSaveSlotController,
        LightRepository lightRepository,
        EffectManager effectManager,
        BackgroundService backgroundService)
    {
        this.messageWindowManager = messageWindowManager ?? throw new ArgumentNullException(nameof(messageWindowManager));
        this.cameraSaveSlotController = cameraSaveSlotController ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.effectManager = effectManager ?? throw new ArgumentNullException(nameof(effectManager));
        this.backgroundService = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));
    }

    public SceneSchema Build()
    {
        var transformSchemaBuilder = new TransformSchemaBuilder();

        var sceneSchema = new SceneSchema()
        {
            MessageWindow = new MessageWindowSchemaBuilder(messageWindowManager)
                .Build(),
            Camera = new CameraSchemaBuilder(cameraSaveSlotController, new CameraInfoSchemaBuilder())
                .Build(),
            Lights = new LightRepositorySchemaBuilder(
                lightRepository,
                new LightSchemaBuilder(new LightPropertiesSchemaBuilder()))
                .Build(),
            Effects = new EffectsSchemaBuilder(
                effectManager,
                new BloomSchemaBuilder(),
                new DepthOfFieldSchemaBuilder(),
                new FogSchemaBuilder(),
                new VignetteSchemaBuilder(),
                new SepiaToneSchemaBuilder(),
                new BlurSchemaBuilder())
                .Build(),
            Background = new BackgroundSchemaBuilder(
                backgroundService,
                new BackgroundModelSchemaBuilder(),
                transformSchemaBuilder)
                .Build(),
        };

        return sceneSchema;
    }
}
