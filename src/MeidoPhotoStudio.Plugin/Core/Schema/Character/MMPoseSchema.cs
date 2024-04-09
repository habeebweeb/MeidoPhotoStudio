namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class MMPoseSchema
{
    public bool SixtyFourFlag { get; init; }

    public Quaternion[] FingerToeRotations { get; init; }

    public bool ProperClavicle { get; init; }

    public Quaternion[] BoneRotations { get; init; }

    public Vector3 HipPosition { get; init; }
}
