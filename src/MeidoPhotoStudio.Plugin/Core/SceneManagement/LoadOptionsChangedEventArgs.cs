namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class LoadOptionsChangedEventArgs(LoadOption loadOption) : EventArgs
{
    public LoadOption LoadOption { get; } = loadOption ?? throw new ArgumentNullException(nameof(loadOption));
}
