namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class FogSchema(short version = FogSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Active { get; init; }

    public float Distance { get; init; }

    public float Density { get; init; }

    public float HeightScale { get; init; }

    public float Height { get; init; }

    public Color FogColour { get; init; }
}
