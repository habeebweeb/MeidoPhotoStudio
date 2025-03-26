namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class PoseSchema(short version = PoseSchema.SchemaVersion)
{
    public const short SchemaVersion = 4;

    public short Version { get; } = version;

    public byte[] AnimationFrameBinary { get; init; }

    public MMPoseSchema MMPose { get; init; }

    public AnimationSchema Animation { get; init; }

    public Quaternion MuneSubL { get; init; } = Quaternion.identity;

    public Quaternion MuneSubR { get; init; } = Quaternion.identity;

    public ChestSchema LeftChest { get; init; }

    public ChestSchema RightChest { get; init; }

    public bool LimbsLimited { get; init; }

    public bool DigitsLimited { get; init; }
}
