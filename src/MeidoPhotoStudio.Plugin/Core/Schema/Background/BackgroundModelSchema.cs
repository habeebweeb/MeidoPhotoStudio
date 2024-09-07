using MeidoPhotoStudio.Plugin.Core.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Background;

public class BackgroundModelSchema(short version = BackgroundModelSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public string ID { get; init; }

    public BackgroundCategory Category { get; init; }

    public string AssetName { get; init; }

    public string Name { get; init; }
}
