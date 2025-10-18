namespace MeidoPhotoStudio.Plugin.Core.Character;

public readonly record struct BoneBackup(Transform Bone, Quaternion LocalRotation, Vector3? LocalPosition = null)
{
    public static BoneBackup Create(Transform bone)
    {
        _ = bone ? bone : throw new ArgumentNullException(nameof(bone));

        return new(bone, bone.localRotation);
    }

    public static BoneBackup CreateWithPosition(Transform bone)
    {
        _ = bone ? bone : throw new ArgumentNullException(nameof(bone));

        return new(bone, bone.localRotation, bone.localPosition);
    }

    public void Apply()
    {
        if (!Bone)
            return;

        Bone.localRotation = LocalRotation;

        if (LocalPosition is Vector3 localPosition)
            Bone.localPosition = localPosition;
    }
}
