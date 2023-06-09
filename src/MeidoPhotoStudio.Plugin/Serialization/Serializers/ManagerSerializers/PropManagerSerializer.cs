using System.Collections.Generic;
using System.IO;

namespace MeidoPhotoStudio.Plugin;

public class PropManagerSerializer : Serializer<PropManager>
{
    private const short Version = 1;

    private static SimpleSerializer<DragPointPropDTO> DragPointDtoSerializer =>
        Serialization.GetSimple<DragPointPropDTO>();

    public override void Serialize(PropManager manager, BinaryWriter writer)
    {
        writer.Write(PropManager.Header);
        writer.WriteVersion(Version);

        var propList = GetPropList(manager);

        writer.Write(propList.Count);

        foreach (var prop in propList)
            DragPointDtoSerializer.Serialize(new DragPointPropDTO(prop), writer);
    }

    public override void Deserialize(PropManager manager, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        manager.DeleteAllProps();

        var propList = GetPropList(manager);
        var propCount = reader.ReadInt32();
        var propIndex = 0;

        for (var i = 0; i < propCount; i++)
        {
            var dragPointPropDto = DragPointDtoSerializer.Deserialize(reader, metadata);

            if (!manager.AddFromPropInfo(dragPointPropDto.PropInfo))
                continue;

            Apply(manager, propList[propIndex], dragPointPropDto);

            propIndex++;
        }
    }

    private static List<DragPointProp> GetPropList(PropManager manager) =>
        Utility.GetFieldValue<PropManager, List<DragPointProp>>(manager, "propList");

    private static void Apply(PropManager manager, DragPointProp prop, DragPointPropDTO dto)
    {
        prop.ShadowCasting = dto.ShadowCasting;
        prop.DragPointEnabled = dto.DragHandleEnabled;
        prop.GizmoEnabled = dto.GizmoEnabled;
        prop.Gizmo.Mode = dto.GizmoMode;
        prop.Visible = dto.Visible;

        var transform = prop.MyObject;

        var attachPointInfo = dto.AttachPointInfo;
        var transformDto = dto.TransformDTO;

        if (dto.AttachPointInfo.AttachPoint is not AttachPoint.None)
        {
            manager.AttachProp(prop, attachPointInfo.AttachPoint, attachPointInfo.MaidIndex);
            transform.localPosition = transformDto.LocalPosition;
            transform.localRotation = transformDto.LocalRotation;
        }

        transform.SetPositionAndRotation(transformDto.Position, transformDto.Rotation);
        transform.localScale = transformDto.LocalScale;
    }
}
