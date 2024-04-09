namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class HeadSchema(short version = HeadSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public bool MMConverted { get; init; }

    public Quaternion LeftEyeRotationDelta { get; init; }

    public Quaternion RightEyeRotationDelta { get; init; }

    public bool FreeLook { get; init; }

    public Vector2 OffsetLookTarget { get; init; }

    public Vector3 HeadLookRotation { get; init; }

    public bool HeadToCamera { get; init; }

    public bool EyeToCamera { get; init; }
}
