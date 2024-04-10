namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class OtherPropModelSchema(short version = OtherPropModelSchema.SchemaVersion) : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public PropType Type =>
        PropType.Other;

    public short Version { get; } = version;

    public string AssetName { get; init; }
}
