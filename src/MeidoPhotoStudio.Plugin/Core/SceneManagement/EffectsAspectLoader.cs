using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Core.Schema.Effects;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class EffectsAspectLoader(
    BloomController bloomController,
    DepthOfFieldController depthOfFieldController,
    VignetteController vignetteController,
    FogController fogController,
    BlurController blurController,
    SepiaToneController sepiaToneController)
    : ISceneAspectLoader<EffectsSchema>
{
    private readonly BloomController bloomController = bloomController
        ?? throw new ArgumentException(nameof(bloomController));

    private readonly DepthOfFieldController depthOfFieldController = depthOfFieldController
        ?? throw new ArgumentException(nameof(depthOfFieldController));

    private readonly VignetteController vignetteController = vignetteController
        ?? throw new ArgumentException(nameof(vignetteController));

    private readonly FogController fogController = fogController
        ?? throw new ArgumentException(nameof(fogController));

    private readonly BlurController blurController = blurController
        ?? throw new ArgumentException(nameof(blurController));

    private readonly SepiaToneController sepiaToneController = sepiaToneController
        ?? throw new ArgumentException(nameof(sepiaToneController));

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
        var (blurSize, blurIterations, downsample) =
            (blurSchema.BlurSize, blurSchema.BlurIterations, blurSchema.Downsample);

        if (blurSchema.Version is 1)
        {
            var realBlurSize = blurSize / 10f;

            (blurSize, blurIterations, downsample) = realBlurSize >= 3f
                ? (realBlurSize - 0.3f, 1, 1)
                : (realBlurSize, 0, 0);
        }

        blurController.Active = blurSchema.Active;
        blurController.BlurSize = blurSize;
        blurController.BlurIterations = blurIterations;
        blurController.Downsample = downsample;
    }

    private void ApplySepiaTone(SepiaToneSchema sepiaToneSchema) =>
        sepiaToneController.Active = sepiaToneSchema.Active;

    private void ApplyVignette(VignetteSchema vignetteSchema)
    {
        vignetteController.Active = vignetteSchema.Active;
        vignetteController.Intensity = vignetteSchema.Intensity;
        vignetteController.Blur = vignetteSchema.Blur;
        vignetteController.BlurSpread = vignetteSchema.BlurSpread;
        vignetteController.ChromaticAberration = vignetteSchema.ChromaticAberration;
    }

    private void ApplyFog(FogSchema fogSchema)
    {
        fogController.Active = fogSchema.Active;
        fogController.Distance = fogSchema.Distance;
        fogController.Density = fogSchema.Density;
        fogController.HeightScale = fogSchema.HeightScale;
        fogController.Height = fogSchema.Height;
        fogController.FogColour = fogSchema.FogColour;
    }

    private void ApplyDepthOfField(DepthOfFieldSchema depthOfFieldSchema)
    {
        depthOfFieldController.Active = depthOfFieldSchema.Active;
        depthOfFieldController.FocalLength = depthOfFieldSchema.FocalLength;
        depthOfFieldController.FocalSize = depthOfFieldSchema.FocalSize;
        depthOfFieldController.Aperture = depthOfFieldSchema.Aperture;
        depthOfFieldController.MaxBlurSize = depthOfFieldSchema.MaxBlurSize;
        depthOfFieldController.VisualizeFocus = depthOfFieldSchema.VisualizeFocus;
    }

    private void ApplyBloom(BloomSchema bloomSchema)
    {
        bloomController.Active = bloomSchema.Active;
        bloomController.BloomValue = (int)bloomSchema.BloomValue;
        bloomController.BlurIterations = bloomSchema.BlurIterations;
        bloomController.BloomThresholdColour = bloomSchema.BloomThresholdColour;
        bloomController.HDR = bloomSchema.BloomHDR;
    }
}
