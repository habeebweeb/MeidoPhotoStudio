using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class CameraAspectLoader : ISceneAspectLoader<CameraSchema>
{
    private readonly CameraSaveSlotController cameraSaveSlotController;

    public CameraAspectLoader(CameraSaveSlotController cameraSaveSlotController) =>
        this.cameraSaveSlotController = cameraSaveSlotController ?? throw new System.ArgumentNullException(nameof(cameraSaveSlotController));

    public void Load(CameraSchema sceneAspectSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Camera)
            return;

        cameraSaveSlotController.CurrentCameraSlot = sceneAspectSchema.CurrentCameraSlot;

        for (var i = 0; i < cameraSaveSlotController.SaveSlotCount; i++)
        {
            if (i >= sceneAspectSchema.CameraInfo.Count)
                break;

            var cameraInfoSchema = sceneAspectSchema.CameraInfo[i];

            cameraSaveSlotController[i] = new(
                cameraInfoSchema.TargetPosition,
                cameraInfoSchema.Rotation,
                cameraInfoSchema.Distance,
                cameraInfoSchema.FOV);
        }
    }
}
