namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class CustomAnimationSchema(short version = CustomAnimationSchema.SchemaVersion) : IAnimationModelSchema
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public long ID { get; init; }

    public string Path { get; init; }

    public bool Custom =>
        true;
}
