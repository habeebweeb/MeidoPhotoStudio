using System.IO;

using MeidoPhotoStudio.Plugin.Core.Camera;

namespace MeidoPhotoStudio.Plugin;

public class CameraManagerSerializer : Serializer<CameraSaveSlotController>
{
    public const string Header = "CAMERA";

    private const short Version = 1;

    public override void Serialize(CameraSaveSlotController manager, BinaryWriter writer)
    {
        writer.Write(Header);
        writer.WriteVersion(Version);

        writer.Write(manager.CurrentCameraSlot);
        writer.Write(manager.SaveSlotCount);

        foreach (var cameraInfo in manager)
            Serialization.GetSimple<CameraInfo>().Serialize(cameraInfo, writer);

        CameraUtility.StopAll();
    }

    public override void Deserialize(CameraSaveSlotController manager, BinaryReader reader, SceneMetadata metadata)
    {
        _ = reader.ReadVersion();

        var currentCameraSlot = reader.ReadInt32();
        var saveSlotCount = reader.ReadInt32();
        var slots = new CameraInfo[saveSlotCount];

        for (var i = 0; i < saveSlotCount; i++)
            slots[i] = Serialization.GetSimple<CameraInfo>().Deserialize(reader, metadata);

        manager.CurrentCameraSlot = currentCameraSlot;

        for (var i = 0; i < manager.SaveSlotCount; i++)
        {
            if (i >= slots.Length)
                break;

            manager[i] = slots[i];
        }
    }
}
