namespace MeidoPhotoStudio.Plugin;

public readonly struct PoseInfo
{
    private static readonly PoseInfo DefaultPoseValue =
        new(Constants.PoseGroupList[0], Constants.PoseDict[Constants.PoseGroupList[0]][0]);

    public PoseInfo(string poseGroup, string pose, bool customPose = false)
    {
        PoseGroup = poseGroup;
        Pose = pose;
        CustomPose = customPose;
    }

    public static ref readonly PoseInfo DefaultPose =>
        ref DefaultPoseValue;

    public string PoseGroup { get; }

    public string Pose { get; }

    public bool CustomPose { get; }
}
