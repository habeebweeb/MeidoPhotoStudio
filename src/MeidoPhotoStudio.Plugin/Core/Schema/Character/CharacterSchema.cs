namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class CharacterSchema(short version = CharacterSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public string ID { get; init; }

    public int Slot { get; init; }

    public TransformSchema Transform { get; init; }

    public HeadSchema Head { get; init; }

    public FaceSchema Face { get; init; }

    public PoseSchema Pose { get; init; }

    public ClothingSchema Clothing { get; init; }
}
