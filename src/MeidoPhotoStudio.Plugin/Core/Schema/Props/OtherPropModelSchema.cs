namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class OtherPropModelSchema : IPropModelSchema
{
    public const short SchemaVersion = 1;

    public OtherPropModelSchema(short version = SchemaVersion) =>
        Version = version;

    public PropType Type =>
        PropType.Other;

    public short Version { get; }

    public string AssetName { get; init; }
}
