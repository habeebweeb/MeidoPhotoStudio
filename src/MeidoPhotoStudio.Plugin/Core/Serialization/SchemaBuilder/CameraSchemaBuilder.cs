using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class CameraSchemaBuilder(
    CameraSaveSlotController cameraSaveSlotController,
    ISchemaBuilder<CameraInfoSchema, CameraInfo> cameraInfoSchemaBuilder)
    : ISceneSchemaAspectBuilder<CameraSchema>
{
    private readonly CameraSaveSlotController cameraSaveSlotController = cameraSaveSlotController
        ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));

    private readonly ISchemaBuilder<CameraInfoSchema, CameraInfo> cameraInfoSchemaBuilder = cameraInfoSchemaBuilder
        ?? throw new ArgumentNullException(nameof(cameraInfoSchemaBuilder));

    public CameraSchema Build() =>
        new()
        {
            CurrentCameraSlot = cameraSaveSlotController.CurrentCameraSlot,
            CameraInfo = cameraSaveSlotController.Select(cameraInfoSchemaBuilder.Build).ToList(),
        };
}
