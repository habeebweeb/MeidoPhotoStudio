using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class EffectsAspectLoader(EffectManager effectManager) : ISceneAspectLoader<EffectsSchema>
{
    private readonly EffectManager effectManager = effectManager
        ?? throw new ArgumentNullException(nameof(effectManager));

    public void Load(EffectsSchema effectsSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Effects.Load)
            return;

        if (loadOptions.Effects.Bloom)
            ApplyBloom(effectsSchema.Bloom);

        if (loadOptions.Effects.DepthOfField)
            ApplyDepthOfField(effectsSchema.DepthOfField);

        if (loadOptions.Effects.Fog)
            ApplyFog(effectsSchema.Fog);

        if (loadOptions.Effects.Vignette)
            ApplyVignette(effectsSchema.Vignette);

        if (loadOptions.Effects.SepiaTone)
            ApplySepiaTone(effectsSchema.SepiaTone);

        if (loadOptions.Effects.Blur)
            ApplyBlur(effectsSchema.Blur);
    }

    private void ApplyBlur(BlurSchema blurSchema)
    {
        var blur = effectManager.Get<BlurEffectManager>();

        blur.SetEffectActive(blurSchema.Active);
        blur.BlurSize = blurSchema.BlurSize;
    }

    private void ApplySepiaTone(SepiaToneSchema sepiaToneSchema)
    {
        var sepiaTone = effectManager.Get<SepiaToneEffectManager>();

        sepiaTone.SetEffectActive(sepiaToneSchema.Active);
    }

    private void ApplyVignette(VignetteSchema vignetteSchema)
    {
        var vignette = effectManager.Get<VignetteEffectManager>();

        vignette.SetEffectActive(vignetteSchema.Active);
        vignette.Intensity = vignetteSchema.Intensity;
        vignette.Blur = vignetteSchema.Blur;
        vignette.BlurSpread = vignetteSchema.BlurSpread;
        vignette.ChromaticAberration = vignetteSchema.ChromaticAberration;
    }

    private void ApplyFog(FogSchema fogSchema)
    {
        var fog = effectManager.Get<FogEffectManager>();

        fog.SetEffectActive(fogSchema.Active);
        fog.Distance = fogSchema.Distance;
        fog.Density = fogSchema.Density;
        fog.HeightScale = fogSchema.HeightScale;
        fog.Height = fogSchema.Height;
        fog.FogColour = fogSchema.FogColour;
    }

    private void ApplyDepthOfField(DepthOfFieldSchema depthOfFieldSchema)
    {
        var dof = effectManager.Get<DepthOfFieldEffectManager>();

        dof.SetEffectActive(depthOfFieldSchema.Active);
        dof.FocalLength = depthOfFieldSchema.FocalLength;
        dof.FocalSize = depthOfFieldSchema.FocalSize;
        dof.Aperture = depthOfFieldSchema.Aperture;
        dof.MaxBlurSize = depthOfFieldSchema.MaxBlurSize;
        dof.VisualizeFocus = depthOfFieldSchema.VisualizeFocus;
    }

    private void ApplyBloom(BloomSchema bloomSchema)
    {
        var bloom = effectManager.Get<BloomEffectManager>();

        bloom.SetEffectActive(bloomSchema.Active);
        bloom.BloomValue = bloomSchema.BloomValue;
        bloom.BlurIterations = bloomSchema.BlurIterations;
        bloom.BloomThresholdColour = bloomSchema.BloomThresholdColour;
        bloom.BloomHDR = bloomSchema.BloomHDR;
    }
}
