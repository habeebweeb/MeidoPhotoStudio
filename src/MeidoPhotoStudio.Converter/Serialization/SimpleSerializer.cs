namespace MeidoPhotoStudio.Converter.Serialization;

public abstract class SimpleSerializer<T> : ISimpleSerializer
    where T : notnull
{
    void ISimpleSerializer.Serialize(object obj, BinaryWriter writer) =>
        Serialize((T)obj, writer);

    object ISimpleSerializer.Deserialize(BinaryReader reader, SceneMetadata metadata) =>
        Deserialize(reader, metadata);

    public abstract void Serialize(T obj, BinaryWriter writer);

    public abstract T Deserialize(BinaryReader reader, SceneMetadata metadata);
}
