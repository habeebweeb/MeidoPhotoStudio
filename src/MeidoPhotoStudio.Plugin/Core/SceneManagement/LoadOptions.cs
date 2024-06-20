namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public readonly record struct LoadOptions(
    CharacterLoadOptions Characters, bool Message, bool Camera, bool Lights, EffectLoadOptions Effects, bool Background, bool Props)
{
    public static LoadOptions All =>
        new()
        {
            Characters = new()
            {
                Load = true,
                ByID = false,
            },
            Message = true,
            Camera = true,
            Lights = true,
            Effects = new()
            {
                Bloom = true,
                DepthOfField = true,
                Vignette = true,
                Fog = true,
                SepiaTone = true,
                Blur = true,
            },
            Background = true,
            Props = true,
        };

    public static LoadOptions Environment =>
        new()
        {
            Characters = new()
            {
                Load = false,
                ByID = false,
            },
            Message = false,
            Camera = false,
            Lights = true,
            Effects = new()
            {
                Bloom = true,
                DepthOfField = true,
                Vignette = true,
                Fog = true,
                SepiaTone = true,
                Blur = true,
            },
            Background = true,
            Props = true,
        };
}
