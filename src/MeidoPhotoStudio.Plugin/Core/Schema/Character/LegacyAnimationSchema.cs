namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class LegacyAnimationSchema(short version = LegacyAnimationSchema.SchemaVersion) : IAnimationModelSchema
{
    public const short SchemaVersion = 1;

    public short Version { get; } = version;

    public bool Custom =>
        CustomPose;

    public string PoseGroup { get; init; }

    public string Pose { get; init; }

    public bool CustomPose { get; init; }
}
