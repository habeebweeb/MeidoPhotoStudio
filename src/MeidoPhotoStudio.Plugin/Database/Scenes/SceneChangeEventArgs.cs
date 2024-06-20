namespace MeidoPhotoStudio.Database.Scenes;

public class SceneChangeEventArgs(SceneModel scene) : EventArgs
{
    public SceneModel Scene { get; } = scene
        ?? throw new ArgumentNullException(nameof(scene));
}
