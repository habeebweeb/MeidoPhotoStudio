using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class Serializer<T> : ISerializer
    {
        void ISerializer.Serialize(object obj, BinaryWriter writer) => Serialize((T) obj, writer);

        void ISerializer.Deserialize(object obj, BinaryReader reader, SceneMetadata metadata)
            => Deserialize((T) obj, reader, metadata);

        public abstract void Serialize(T obj, BinaryWriter writer);
        public abstract void Deserialize(T obj, BinaryReader reader, SceneMetadata metadata);
    }
}
