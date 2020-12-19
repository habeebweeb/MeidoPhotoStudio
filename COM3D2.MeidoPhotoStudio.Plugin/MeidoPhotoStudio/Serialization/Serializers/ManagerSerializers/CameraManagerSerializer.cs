using System.IO;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class CameraManagerSerializer : Serializer<CameraManager>
    {
        private const short version = 1;
        private static Serializer<CameraInfo> InfoSerializer => Serialization.Get<CameraInfo>();
        private static readonly CameraInfo dummyInfo = new();

        public override void Serialize(CameraManager manager, BinaryWriter writer)
        {
            writer.Write(CameraManager.header);
            writer.WriteVersion(version);

            CameraInfo[] cameraInfos = GetCameraInfos(manager);
            cameraInfos[manager.CurrentCameraIndex].UpdateInfo(CameraUtility.MainCamera);

            writer.Write(manager.CurrentCameraIndex);
            writer.Write(manager.CameraCount);
            foreach (var info in cameraInfos) InfoSerializer.Serialize(info, writer);
        }

        public override void Deserialize(CameraManager manager, BinaryReader reader, SceneMetadata metadata)
        {
            _ = reader.ReadVersion();

            var camera = CameraUtility.MainCamera;

            manager.CurrentCameraIndex = reader.ReadInt32();

            var cameraCount = reader.ReadInt32();

            CameraInfo[] cameraInfos = GetCameraInfos(manager);
            for (var i = 0; i < cameraCount; i++)
                InfoSerializer.Deserialize(i >= manager.CameraCount ? dummyInfo : cameraInfos[i], reader, metadata);

            if (metadata.Environment) return;

            cameraInfos[manager.CurrentCameraIndex].Apply(camera);
        }

        private static CameraInfo[] GetCameraInfos(CameraManager manager)
            => Utility.GetFieldValue<CameraManager, CameraInfo[]>(manager, "cameraInfos");
    }
}
