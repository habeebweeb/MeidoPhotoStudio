using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class CameraSchemaBuilder : ISceneSchemaAspectBuilder<CameraSchema>
{
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly ISchemaBuilder<CameraInfoSchema, CameraInfo> cameraInfoSchemaBuilder;

    public CameraSchemaBuilder(CameraSaveSlotController cameraSaveSlotController, ISchemaBuilder<CameraInfoSchema, CameraInfo> cameraInfoSchemaBuilder)
    {
        this.cameraSaveSlotController = cameraSaveSlotController ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));
        this.cameraInfoSchemaBuilder = cameraInfoSchemaBuilder ?? throw new ArgumentNullException(nameof(cameraInfoSchemaBuilder));
    }

    public CameraSchema Build() =>
        new()
        {
            CurrentCameraSlot = cameraSaveSlotController.CurrentCameraSlot,
            CameraInfo = cameraSaveSlotController.Select(cameraInfoSchemaBuilder.Build).ToList(),
        };
}
