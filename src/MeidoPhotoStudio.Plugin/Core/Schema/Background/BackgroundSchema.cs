namespace MeidoPhotoStudio.Plugin.Core.Schema.Background;

public class BackgroundSchema
{
    public const short SchemaVersion = 2;

    public BackgroundSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public string BackgroundName { get; init; }

    public BackgroundModelSchema Background { get; init; }

    public TransformSchema Transform { get; init; }

    public Color Colour { get; init; }
}
