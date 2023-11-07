using System.IO;

namespace MeidoPhotoStudio.Converter.Serialization;

public class TransformDTOSerializer : SimpleSerializer<TransformDTO>
{
    private const short Version = 1;

    public override void Serialize(TransformDTO transform, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        writer.Write(transform.Position);
        writer.Write(transform.Rotation);
        writer.Write(transform.LocalPosition);
        writer.Write(transform.LocalRotation);
        writer.Write(transform.LocalScale);
    }

    public override TransformDTO Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new TransformDTO
        {
            Position = reader.ReadVector3(),
            Rotation = reader.ReadQuaternion(),
            LocalPosition = reader.ReadVector3(),
            LocalRotation = reader.ReadQuaternion(),
            LocalScale = reader.ReadVector3(),
        };
    }
}
