using System.IO;

using MeidoPhotoStudio.Plugin.Core.Schema;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public interface ISceneSerializer
{
    void SerializeScene(Stream stream, SceneSchema sceneSchema);

    SceneSchema DeserializeScene(Stream stream);
}
