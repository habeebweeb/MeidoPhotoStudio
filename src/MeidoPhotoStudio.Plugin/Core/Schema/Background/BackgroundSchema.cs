namespace MeidoPhotoStudio.Plugin.Core.Schema.Background;

public class BackgroundSchema(short version = BackgroundSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public string BackgroundName { get; init; }

    public BackgroundModelSchema Background { get; init; }

    public TransformSchema Transform { get; init; }

    public Color Colour { get; init; }

    public bool Visible { get; init; } = true;
}
