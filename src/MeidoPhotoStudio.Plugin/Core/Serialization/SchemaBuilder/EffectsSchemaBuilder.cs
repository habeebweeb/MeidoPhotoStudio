using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class EffectsSchemaBuilder(
    BloomController bloomController,
    DepthOfFieldController depthOfFieldController,
    FogController fogController,
    VignetteController vignetteController,
    SepiaToneController sepiaToneController,
    BlurController blurController,
    ISchemaBuilder<BloomSchema, BloomController> bloomSchemaBuilder,
    ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldController> depthOfFieldSchemaBuilder,
    ISchemaBuilder<FogSchema, FogController> fogSchemaBuilder,
    ISchemaBuilder<VignetteSchema, VignetteController> vignetteSchemaBuilder,
    ISchemaBuilder<SepiaToneSchema, SepiaToneController> sepiaToneSchemaBuilder,
    ISchemaBuilder<BlurSchema, BlurController> blurSchemaBuilder)
    : ISceneSchemaAspectBuilder<EffectsSchema>
{
    private readonly BloomController bloomController =
        bloomController ?? throw new ArgumentNullException(nameof(bloomController));

    private readonly DepthOfFieldController depthOfFieldController =
        depthOfFieldController ?? throw new ArgumentNullException(nameof(depthOfFieldController));

    private readonly FogController fogController =
        fogController ?? throw new ArgumentNullException(nameof(fogController));

    private readonly VignetteController vignetteController =
        vignetteController ?? throw new ArgumentNullException(nameof(vignetteController));

    private readonly SepiaToneController sepiaToneController =
        sepiaToneController ?? throw new ArgumentNullException(nameof(sepiaToneController));

    private readonly BlurController blurController =
        blurController ?? throw new ArgumentNullException(nameof(blurController));

    private readonly ISchemaBuilder<BloomSchema, BloomController> bloomSchemaBuilder =
        bloomSchemaBuilder ?? throw new ArgumentNullException(nameof(bloomSchemaBuilder));

    private readonly ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldController> depthOfFieldSchemaBuilder =
        depthOfFieldSchemaBuilder ?? throw new ArgumentNullException(nameof(depthOfFieldSchemaBuilder));

    private readonly ISchemaBuilder<FogSchema, FogController> fogSchemaBuilder =
        fogSchemaBuilder ?? throw new ArgumentNullException(nameof(fogSchemaBuilder));

    private readonly ISchemaBuilder<VignetteSchema, VignetteController> vignetteSchemaBuilder =
        vignetteSchemaBuilder ?? throw new ArgumentNullException(nameof(vignetteSchemaBuilder));

    private readonly ISchemaBuilder<SepiaToneSchema, SepiaToneController> sepiaToneSchemaBuilder =
        sepiaToneSchemaBuilder ?? throw new ArgumentNullException(nameof(sepiaToneSchemaBuilder));

    private readonly ISchemaBuilder<BlurSchema, BlurController> blurSchemaBuilder =
        blurSchemaBuilder ?? throw new ArgumentNullException(nameof(blurSchemaBuilder));

    public EffectsSchema Build() =>
        new()
        {
            Bloom = bloomSchemaBuilder.Build(bloomController),
            DepthOfField = depthOfFieldSchemaBuilder.Build(depthOfFieldController),
            Fog = fogSchemaBuilder.Build(fogController),
            Vignette = vignetteSchemaBuilder.Build(vignetteController),
            SepiaTone = sepiaToneSchemaBuilder.Build(sepiaToneController),
            Blur = blurSchemaBuilder.Build(blurController),
        };
}
