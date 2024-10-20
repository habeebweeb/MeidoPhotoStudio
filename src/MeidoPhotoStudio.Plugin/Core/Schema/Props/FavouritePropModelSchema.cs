namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class FavouritePropModelSchema(short version = FavouritePropModelSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public IPropModelSchema PropModel { get; init; }

    public string Name { get; init; }

    public DateTime DateAdded { get; init; }
}
