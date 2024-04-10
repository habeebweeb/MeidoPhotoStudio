namespace MeidoPhotoStudio.Plugin.Core.Schema.Camera;

public class CameraInfoSchema(short version = CameraInfoSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Vector3 TargetPosition { get; init; }

    public Quaternion Rotation { get; init; }

    public float Distance { get; init; }

    public float FOV { get; init; }
}
