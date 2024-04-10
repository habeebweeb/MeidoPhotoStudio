namespace MeidoPhotoStudio.Plugin.Core.Schema.Camera;

public class CameraSchema(short version = CameraSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public int CurrentCameraSlot { get; init; }

    public List<CameraInfoSchema> CameraInfo { get; init; }
}
