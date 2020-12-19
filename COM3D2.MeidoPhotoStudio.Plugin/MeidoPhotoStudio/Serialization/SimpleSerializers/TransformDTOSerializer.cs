using System.IO;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class TransformDTOSerializer : SimpleSerializer<TransformDTO>
    {
        private const short version = 1;

        public override void Serialize(TransformDTO transform, BinaryWriter writer)
        {
            writer.WriteVersion(version);

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
                LocalScale = reader.ReadVector3()
            };
        }
    }

    public class TransformDTO
    {
        public Vector3 Position { get; init; }
        public Vector3 LocalPosition { get; init; }
        public Quaternion Rotation { get; init; }
        public Quaternion LocalRotation { get; init; }
        public Vector3 LocalScale { get; init; }

        public TransformDTO() { }

        public TransformDTO(Transform transform)
        {
            Position = transform.position;
            LocalPosition = transform.localPosition;
            Rotation = transform.rotation;
            LocalRotation = transform.localRotation;
            LocalScale = transform.localScale;
        }
    }
}
