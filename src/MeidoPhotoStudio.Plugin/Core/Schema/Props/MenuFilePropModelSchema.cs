namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class MenuFilePropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public MenuFilePropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.Menu;

    public short Version { get; }

    public string ID { get; init; }

    public string Filename { get; init; }
}
