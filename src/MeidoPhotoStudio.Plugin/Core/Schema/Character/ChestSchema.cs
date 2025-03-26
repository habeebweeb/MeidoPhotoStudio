namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class ChestSchema(short version = ChestSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public Vector3 MunePositionDelta { get; init; }

    public Vector3 MuneSubPositionDelta { get; init; }

    public Quaternion MuneSubRotation { get; init; }
}
