namespace MeidoPhotoStudio.Plugin.Core.Extension;

public abstract class Extension
{
    public abstract string GUID { get; }

    public abstract string Name { get; }

    public virtual ExtensionSceneAspectController SceneAspectController { get; } = null;
}
