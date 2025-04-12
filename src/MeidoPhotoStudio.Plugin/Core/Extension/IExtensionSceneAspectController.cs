using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Plugin.Core.Extension;

public interface IExtensionSceneAspectController : IExtensionDataBuilder, IExtensionSceneAspectLoader
{
    Type DataType { get; }

    IEnumerable<LoadOptionSpec> SubLoadOptions { get; }
}
