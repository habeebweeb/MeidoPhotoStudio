using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public interface ISimpleSerializer
    {
        void Serialize(object obj, BinaryWriter writer);
        object Deserialize(BinaryReader reader, SceneMetadata metadata);
    }
}
