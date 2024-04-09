namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class PoseSchema(short version = PoseSchema.SchemaVersion)
{
    public const short SchemaVersion = 3;

    public short Version { get; } = version;

    public byte[] AnimationFrameBinary { get; init; }

    public MMPoseSchema MMPose { get; init; }

    public AnimationSchema Animation { get; init; }

    public Quaternion MuneSubL { get; init; }

    public Quaternion MuneSubR { get; init; }

    public bool LimbsLimited { get; init; }

    public bool DigitsLimited { get; init; }
}
