using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class EffectsSchemaBuilder(
    EffectManager effectManager,
    ISchemaBuilder<BloomSchema, BloomEffectManager> bloomSchemaBuilder,
    ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldEffectManager> depthOfFieldSchemaBuilder,
    ISchemaBuilder<FogSchema, FogEffectManager> fogSchemaBuilder,
    ISchemaBuilder<VignetteSchema, VignetteEffectManager> vignetteSchemaBuilder,
    ISchemaBuilder<SepiaToneSchema, SepiaToneEffectManager> sepiaToneSchemaBuilder,
    ISchemaBuilder<BlurSchema, BlurEffectManager> blurSchemaBuilder)
    : ISceneSchemaAspectBuilder<EffectsSchema>
{
    private readonly EffectManager effectManager = effectManager
        ?? throw new ArgumentNullException(nameof(effectManager));

    private readonly ISchemaBuilder<BloomSchema, BloomEffectManager> bloomSchemaBuilder = bloomSchemaBuilder
        ?? throw new ArgumentNullException(nameof(bloomSchemaBuilder));

    private readonly ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldEffectManager> depthOfFieldSchemaBuilder = depthOfFieldSchemaBuilder
        ?? throw new ArgumentNullException(nameof(depthOfFieldSchemaBuilder));

    private readonly ISchemaBuilder<FogSchema, FogEffectManager> fogSchemaBuilder = fogSchemaBuilder
        ?? throw new ArgumentNullException(nameof(fogSchemaBuilder));

    private readonly ISchemaBuilder<VignetteSchema, VignetteEffectManager> vignetteSchemaBuilder = vignetteSchemaBuilder
        ?? throw new ArgumentNullException(nameof(vignetteSchemaBuilder));

    private readonly ISchemaBuilder<SepiaToneSchema, SepiaToneEffectManager> sepiaToneSchemaBuilder = sepiaToneSchemaBuilder
        ?? throw new ArgumentNullException(nameof(sepiaToneSchemaBuilder));

    private readonly ISchemaBuilder<BlurSchema, BlurEffectManager> blurSchemaBuilder = blurSchemaBuilder
        ?? throw new ArgumentNullException(nameof(blurSchemaBuilder));

    public EffectsSchema Build() =>
        new()
        {
            Bloom = bloomSchemaBuilder.Build(effectManager.Get<BloomEffectManager>()),
            DepthOfField = depthOfFieldSchemaBuilder.Build(effectManager.Get<DepthOfFieldEffectManager>()),
            Fog = fogSchemaBuilder.Build(effectManager.Get<FogEffectManager>()),
            Vignette = vignetteSchemaBuilder.Build(effectManager.Get<VignetteEffectManager>()),
            SepiaTone = sepiaToneSchemaBuilder.Build(effectManager.Get<SepiaToneEffectManager>()),
            Blur = blurSchemaBuilder.Build(effectManager.Get<BlurEffectManager>()),
        };
}
