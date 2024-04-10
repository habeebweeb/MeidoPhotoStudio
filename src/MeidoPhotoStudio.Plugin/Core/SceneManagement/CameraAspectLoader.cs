using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class CameraAspectLoader(CameraSaveSlotController cameraSaveSlotController) : ISceneAspectLoader<CameraSchema>
{
    private readonly CameraSaveSlotController cameraSaveSlotController = cameraSaveSlotController
        ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));

    public void Load(CameraSchema cameraSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Camera)
            return;

        if (cameraSchema is null)
            return;

        cameraSaveSlotController.CurrentCameraSlot = cameraSchema.CurrentCameraSlot;

        for (var i = 0; i < cameraSaveSlotController.SaveSlotCount; i++)
        {
            if (i >= cameraSchema.CameraInfo.Count)
                break;

            var cameraInfoSchema = cameraSchema.CameraInfo[i];

            cameraSaveSlotController[i] = new(
                cameraInfoSchema.TargetPosition,
                cameraInfoSchema.Rotation,
                cameraInfoSchema.Distance,
                cameraInfoSchema.FOV);
        }
    }
}
