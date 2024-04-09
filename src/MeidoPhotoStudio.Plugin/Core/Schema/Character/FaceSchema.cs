namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class FaceSchema(short version = FaceSchema.SchemaVersion)
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Blink { get; init; }

    public IBlendSetModelSchema BlendSet { get; init; }

    public Dictionary<string, float> FacialExpressionSet { get; init; }
}
