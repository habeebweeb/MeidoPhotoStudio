using MeidoPhotoStudio.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Background;

public class BackgroundModelSchema
{
    public const short SchemaVersion = 1;

    public BackgroundModelSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public string ID { get; init; }

    public BackgroundCategory Category { get; init; }

    public string AssetName { get; init; }

    public string Name { get; init; }
}
