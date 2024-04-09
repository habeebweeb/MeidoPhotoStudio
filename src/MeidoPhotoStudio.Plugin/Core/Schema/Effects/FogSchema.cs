namespace MeidoPhotoStudio.Plugin.Core.Schema.Effects;

public class FogSchema
{
    public const short SchemaVersion = 1;

    public FogSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public bool Active { get; init; }

    public float Distance { get; init; }

    public float Density { get; init; }

    public float HeightScale { get; init; }

    public float Height { get; init; }

    public Color FogColour { get; init; }
}
