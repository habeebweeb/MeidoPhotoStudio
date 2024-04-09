namespace MeidoPhotoStudio.Plugin.Core.Schema.Camera;

public class CameraSchema
{
    public const short SchemaVersion = 1;

    public CameraSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public int CurrentCameraSlot { get; init; }

    public List<CameraInfoSchema> CameraInfo { get; init; }
}
