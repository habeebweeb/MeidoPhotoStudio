using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class CameraInfoSchemaBuilder : ISchemaBuilder<CameraInfoSchema, CameraInfo>
{
    public CameraInfoSchema Build(CameraInfo cameraInfo) =>
        new()
        {
            TargetPosition = cameraInfo.TargetPos,
            Rotation = cameraInfo.Angle,
            Distance = cameraInfo.Distance,
            FOV = cameraInfo.FOV,
        };
}
