using System;

using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class EffectsSchemaBuilder
{
    private readonly EffectManager effectManager;
    private readonly ISchemaBuilder<BloomSchema, BloomEffectManager> bloomSchemaBuilder;
    private readonly ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldEffectManager> depthOfFieldSchemaBuilder;
    private readonly ISchemaBuilder<FogSchema, FogEffectManager> fogSchemaBuilder;
    private readonly ISchemaBuilder<VignetteSchema, VignetteEffectManager> vignetteSchemaBuilder;
    private readonly ISchemaBuilder<SepiaToneSchema, SepiaToneEffectManager> sepiaToneSchemaBuilder;
    private readonly ISchemaBuilder<BlurSchema, BlurEffectManager> blurSchemaBuilder;

    public EffectsSchemaBuilder(
        EffectManager effectManager,
        ISchemaBuilder<BloomSchema, BloomEffectManager> bloomSchemaBuilder,
        ISchemaBuilder<DepthOfFieldSchema, DepthOfFieldEffectManager> depthOfFieldSchemaBuilder,
        ISchemaBuilder<FogSchema, FogEffectManager> fogSchemaBuilder,
        ISchemaBuilder<VignetteSchema, VignetteEffectManager> vignetteSchemaBuilder,
        ISchemaBuilder<SepiaToneSchema, SepiaToneEffectManager> sepiaToneSchemaBuilder,
        ISchemaBuilder<BlurSchema, BlurEffectManager> blurSchemaBuilder)
    {
        this.effectManager = effectManager ?? throw new ArgumentNullException(nameof(effectManager));
        this.bloomSchemaBuilder = bloomSchemaBuilder ?? throw new ArgumentNullException(nameof(bloomSchemaBuilder));
        this.depthOfFieldSchemaBuilder = depthOfFieldSchemaBuilder ?? throw new ArgumentNullException(nameof(depthOfFieldSchemaBuilder));
        this.fogSchemaBuilder = fogSchemaBuilder ?? throw new ArgumentNullException(nameof(fogSchemaBuilder));
        this.vignetteSchemaBuilder = vignetteSchemaBuilder ?? throw new ArgumentNullException(nameof(vignetteSchemaBuilder));
        this.sepiaToneSchemaBuilder = sepiaToneSchemaBuilder ?? throw new ArgumentNullException(nameof(sepiaToneSchemaBuilder));
        this.blurSchemaBuilder = blurSchemaBuilder ?? throw new ArgumentNullException(nameof(blurSchemaBuilder));
    }

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
