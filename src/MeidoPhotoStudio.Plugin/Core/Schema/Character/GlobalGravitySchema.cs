namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class GlobalGravitySchema(short version = GlobalGravitySchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Enabled { get; init; }

    public Vector3 HairGravityPosition { get; init; }

    public Vector3 ClothingGravityPosition { get; init; }
}
