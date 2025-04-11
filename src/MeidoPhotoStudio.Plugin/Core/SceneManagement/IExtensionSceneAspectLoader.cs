using MeidoPhotoStudio.Plugin.Core.Extension;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public interface IExtensionSceneAspectLoader
{
    void Load(IExtensionData extensionData, ILoadOptions loadOptions);
}
