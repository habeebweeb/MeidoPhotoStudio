using MeidoPhotoStudio.Plugin.Core.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core.Extension;

public abstract class ExtensionSceneAspectController : IExtensionSceneAspectController
{
    public abstract Type DataType { get; }

    public abstract IExtensionData Build();

    public abstract void Load(IExtensionData extensionData, ILoadOptions loadOptions);
}
