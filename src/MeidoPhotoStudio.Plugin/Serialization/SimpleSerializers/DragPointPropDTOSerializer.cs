using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPropDTOSerializer : SimpleSerializer<DragPointPropDTO>
{
    private const short Version = 1;

    private static SimpleSerializer<PropInfo> PropInfoSerializer =>
        Serialization.GetSimple<PropInfo>();

    private static SimpleSerializer<TransformDTO> TransformSerializer =>
        Serialization.GetSimple<TransformDTO>();

    private static SimpleSerializer<AttachPointInfo> AttachPointSerializer =>
        Serialization.GetSimple<AttachPointInfo>();

    public override void Serialize(DragPointPropDTO dragPointDto, BinaryWriter writer)
    {
        writer.WriteVersion(Version);

        PropInfoSerializer.Serialize(dragPointDto.PropInfo, writer);

        TransformSerializer.Serialize(dragPointDto.TransformDTO, writer);

        AttachPointSerializer.Serialize(dragPointDto.AttachPointInfo, writer);

        writer.Write(dragPointDto.ShadowCasting);
    }

    public override DragPointPropDTO Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        return new DragPointPropDTO
        {
            PropInfo = PropInfoSerializer.Deserialize(reader, metadata),
            TransformDTO = TransformSerializer.Deserialize(reader, metadata),
            AttachPointInfo = AttachPointSerializer.Deserialize(reader, metadata),
            ShadowCasting = reader.ReadBoolean(),
        };
    }
}
