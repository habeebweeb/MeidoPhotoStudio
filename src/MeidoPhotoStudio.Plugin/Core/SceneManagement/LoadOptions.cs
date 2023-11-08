namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public readonly struct LoadOptions
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

    public bool Maids { get; init; }

    public bool Message { get; init; }

    public bool Camera { get; init; }

    public bool Lights { get; init; }

    public bool Effects { get; init; }

    public bool Background { get; init; }

    public bool Props { get; init; }
}
