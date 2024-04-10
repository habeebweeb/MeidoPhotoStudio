namespace MeidoPhotoStudio.Plugin.Core.Schema;

public class TransformSchema(short version = TransformSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Vector3 Position { get; init; }

    public Quaternion Rotation { get; init; } = Quaternion.identity;

    public Vector3 LocalPosition { get; init; }

    public Quaternion LocalRotation { get; init; } = Quaternion.identity;

    public Vector3 LocalScale { get; init; } = Vector3.one;
}
