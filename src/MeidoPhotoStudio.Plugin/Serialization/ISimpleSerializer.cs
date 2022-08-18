using System.IO;

namespace MeidoPhotoStudio.Plugin;

public interface ISimpleSerializer
{
    void Serialize(object obj, BinaryWriter writer);

    object Deserialize(BinaryReader reader, SceneMetadata metadata);
}
