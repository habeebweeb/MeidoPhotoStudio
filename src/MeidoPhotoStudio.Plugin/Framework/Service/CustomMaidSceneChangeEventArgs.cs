namespace MeidoPhotoStudio.Plugin.Framework.Service;

public class CustomMaidSceneChangeEventArgs(
    CustomMaidSceneService.CustomMaidScene current,
    CustomMaidSceneService.CustomMaidScene next)
    : EventArgs
{
    public CustomMaidSceneService.CustomMaidScene CurrentScene { get; } = current;

    public CustomMaidSceneService.CustomMaidScene NextScene { get; } = next;
}
