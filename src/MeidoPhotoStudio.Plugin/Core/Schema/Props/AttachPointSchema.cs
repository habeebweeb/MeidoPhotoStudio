namespace MeidoPhotoStudio.Plugin.Core.Schema.Props;

public class AttachPointSchema(short version = AttachPointSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public AttachPoint AttachPoint { get; init; }

    public int CharacterIndex { get; init; }

    public string CharacterID { get; init; }
}
