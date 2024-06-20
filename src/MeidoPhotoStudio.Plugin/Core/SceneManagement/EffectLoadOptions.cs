namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public readonly record struct EffectLoadOptions(
    bool Load = true,
    bool Bloom = true,
    bool DepthOfField = true,
    bool Vignette = true,
    bool Fog = true,
    bool SepiaTone = true,
    bool Blur = true);
