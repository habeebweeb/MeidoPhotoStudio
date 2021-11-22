using System.IO;

namespace MeidoPhotoStudio.Plugin
{
    public class DragPointPropDTOSerializer : SimpleSerializer<DragPointPropDTO>
    {
        private const short version = 1;

        private static SimpleSerializer<PropInfo> PropInfoSerializer => Serialization.GetSimple<PropInfo>();
        private static SimpleSerializer<TransformDTO> TransformSerializer => Serialization.GetSimple<TransformDTO>();

        private static SimpleSerializer<AttachPointInfo> AttachPointSerializer
            => Serialization.GetSimple<AttachPointInfo>();

        public override void Serialize(DragPointPropDTO dragPointDto, BinaryWriter writer)
        {
            writer.WriteVersion(version);

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
                ShadowCasting = reader.ReadBoolean()
            };
        }
    }

    public class DragPointPropDTO
    {
        public TransformDTO TransformDTO { get; set; }
        public AttachPointInfo AttachPointInfo { get; set; }
        public PropInfo PropInfo { get; set; }
        public bool ShadowCasting { get; set; }

        public DragPointPropDTO() { }

        public DragPointPropDTO(DragPointProp dragPoint)
        {
            TransformDTO = new TransformDTO(dragPoint.MyObject.transform);
            ShadowCasting = dragPoint.ShadowCasting;
            AttachPointInfo = dragPoint.AttachPointInfo;
            PropInfo = dragPoint.Info;
        }

        public void Deconstruct(out TransformDTO transform, out AttachPointInfo attachPointInfo, out bool shadowCasting)
        {
            transform = TransformDTO;
            attachPointInfo = AttachPointInfo;
            shadowCasting = ShadowCasting;
        }
    }
}
