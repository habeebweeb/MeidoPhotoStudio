namespace MeidoPhotoStudio.Plugin.Core.Database.Scenes;

public class SceneRepositoryChangedEventArgs : EventArgs
{
    public SceneModel Scene { get; init; }

    public string Category { get; init; }
}
