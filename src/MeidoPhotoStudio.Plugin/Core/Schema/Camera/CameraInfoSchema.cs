namespace MeidoPhotoStudio.Plugin.Core.Schema.Camera;

public class CameraInfoSchema
{
    public const short SchemaVersion = 1;

    public CameraInfoSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public Vector3 TargetPosition { get; init; }

    public Quaternion Rotation { get; init; }

    public float Distance { get; init; }

    public float FOV { get; init; }
}
