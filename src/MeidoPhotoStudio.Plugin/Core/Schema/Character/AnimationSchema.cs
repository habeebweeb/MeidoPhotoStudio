namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class AnimationSchema(short version = AnimationSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public IAnimationModelSchema Animation { get; init; }

    public float Time { get; init; }

    public bool Playing { get; init; }
}
