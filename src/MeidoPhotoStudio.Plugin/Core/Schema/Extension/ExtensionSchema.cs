using MeidoPhotoStudio.Plugin.Core.Extension;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Extension;

public class ExtensionSchema(short version = ExtensionSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public List<IExtensionData> ExtensionData { get; init; }
}
