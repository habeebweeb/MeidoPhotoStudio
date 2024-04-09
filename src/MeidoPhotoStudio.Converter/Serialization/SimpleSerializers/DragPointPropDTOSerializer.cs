using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Converter.Serialization;

public class DragPointPropDTOSerializer : SimpleSerializer<DragPointPropDTO>
{
    private const short Version = 2;

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

        // NOTE: V2 data
        {
            writer.Write(dragPointDto.DragHandleEnabled);
            writer.Write(dragPointDto.GizmoEnabled);
            writer.Write((int)dragPointDto.GizmoMode);
            writer.Write(dragPointDto.Visible);
        }
    }

    public override DragPointPropDTO Deserialize(BinaryReader reader, SceneMetadata metadata)
    {
        var version = reader.ReadVersion();

        var dto = new DragPointPropDTO()
        {
            PropInfo = PropInfoSerializer.Deserialize(reader, metadata),
            TransformDTO = TransformSerializer.Deserialize(reader, metadata),
            AttachPointInfo = AttachPointSerializer.Deserialize(reader, metadata),
            ShadowCasting = reader.ReadBoolean(),
        };

        if (version >= 2)
        {
            dto.DragHandleEnabled = reader.ReadBoolean();
            dto.GizmoEnabled = reader.ReadBoolean();
            dto.GizmoMode = (CustomGizmo.GizmoMode)reader.ReadInt32();
            dto.Visible = reader.ReadBoolean();
        }

        return dto;
    }
}
