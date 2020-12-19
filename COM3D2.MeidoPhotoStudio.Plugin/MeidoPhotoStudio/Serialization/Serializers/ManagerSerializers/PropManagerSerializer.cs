using System.Collections.Generic;
using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class PropManagerSerializer : Serializer<PropManager>
    {
        private const short version = 1;

        private static SimpleSerializer<DragPointPropDTO> DragPointDtoSerializer
            => Serialization.GetSimple<DragPointPropDTO>();

        public override void Serialize(PropManager manager, BinaryWriter writer)
        {
            writer.Write(PropManager.header);
            writer.WriteVersion(version);

            List<DragPointProp> propList = GetPropList(manager);

            writer.Write(propList.Count);
            foreach (var prop in propList) DragPointDtoSerializer.Serialize(new DragPointPropDTO(prop), writer);
        }

        public override void Deserialize(PropManager manager, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            manager.DeleteAllProps();

            List<DragPointProp> propList = GetPropList(manager);

            var propCount = reader.ReadInt32();
            var propIndex = 0;
            for (var i = 0; i < propCount; i++)
            {
                var dragPointPropDto = DragPointDtoSerializer.Deserialize(reader, metadata);

                if (!manager.AddFromPropInfo(dragPointPropDto.PropInfo)) continue;

                Apply(manager, propList[propIndex], dragPointPropDto);

                propIndex++;
            }
        }

        private static void Apply(PropManager manager, DragPointProp prop, DragPointPropDTO dto)
        {
            var (transformDto, attachPointInfo, shadowCasting) = dto;

            prop.ShadowCasting = shadowCasting;

            var transform = prop.MyObject;

            if (attachPointInfo.AttachPoint != AttachPoint.None)
            {
                manager.AttachProp(prop, attachPointInfo.AttachPoint, attachPointInfo.MaidIndex);
                transform.localPosition = transformDto.LocalPosition;
                transform.localRotation = transformDto.LocalRotation;
            }

            transform.position = transformDto.Position;
            transform.rotation = transformDto.Rotation;
            transform.localScale = transformDto.LocalScale;
        }

        private static List<DragPointProp> GetPropList(PropManager manager)
            => Utility.GetFieldValue<PropManager, List<DragPointProp>>(manager, "propList");
    }
}
