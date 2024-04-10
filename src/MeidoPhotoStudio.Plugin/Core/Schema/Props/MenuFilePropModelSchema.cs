namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class MenuFilePropModelSchema(short version = MenuFilePropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.Menu;

    public short Version { get; } = version;

    public string ID { get; init; }

    public string Filename { get; init; }
}
