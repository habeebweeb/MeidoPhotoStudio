namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class AttachPointSchema
{
    public const short SchemaVersion = 2;

    public AttachPointSchema(short version = SchemaVersion) =>
        Version = version;

    public short Version { get; }

    public AttachPoint AttachPoint { get; init; }

    public int CharacterIndex { get; init; }

    public string CharacterID { get; init; }
}
