namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public readonly record struct LoadOptions(
    bool Maids, bool Message, bool Camera, bool Lights, bool Effects, bool Background, bool Props)
{
    public static LoadOptions All =>
        new()
        {
            Maids = true,
            Message = true,
            Camera = true,
            Lights = true,
            Effects = true,
            Background = true,
            Props = true,
        };

    public static LoadOptions Environment =>
        new()
        {
            Maids = false,
            Message = false,
            Camera = false,
            Lights = true,
            Effects = true,
            Background = true,
            Props = true,
        };
}
