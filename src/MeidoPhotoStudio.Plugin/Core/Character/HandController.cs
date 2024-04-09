using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class HandController
{
    private readonly IKController ikController;

    public HandController(IKController ikController, HandOrFootType type)
    {
        this.ikController = ikController ?? throw new ArgumentNullException(nameof(ikController));

        if (!Enum.IsDefined(typeof(HandOrFootType), type))
            throw new InvalidEnumArgumentException(nameof(type), (int)type, typeof(HandOrFootType));

        Type = type;
    }

    public HandOrFootType Type { get; }

    public bool IsHand =>
        Type is HandOrFootType.HandLeft or HandOrFootType.HandRight;

    private IEnumerable<Transform> Bones
    {
        get
        {
            var bone = Type switch
            {
                HandOrFootType.HandLeft => "Bip01 L Finger",
                HandOrFootType.HandRight => "Bip01 R Finger",
                HandOrFootType.FootLeft => "Bip01 L Toe",
                HandOrFootType.FootRight => "Bip01 R Toe",
                _ => throw new NotImplementedException(),
            };

            var (baseJointCount, digitCount) = Type switch
            {
                HandOrFootType.HandLeft or HandOrFootType.HandRight => (5, 3),
                HandOrFootType.FootLeft or HandOrFootType.FootRight => (3, 2),
                _ => throw new NotImplementedException(),
            };

            for (var baseJoint = 0; baseJoint < baseJointCount; baseJoint++)
            {
                var boneBase = bone + baseJoint;

                for (var digitJoint = 0; digitJoint < digitCount; digitJoint++)
                {
                    var joint = digitJoint is 0 ? boneBase : boneBase + digitJoint;

                    yield return ikController.GetBone(joint);
                }
            }
        }
    }

    public void ApplyPreset(HandOrFootPreset preset)
    {
        _ = preset ?? throw new ArgumentNullException(nameof(preset));

        if (IsHand != preset.IsHandPreset)
            throw new NotSupportedException(
                $"Preset is not valid for hand or foot type. Type: {Type}, preset type: {preset.Type}");

        var right = Type is HandOrFootType.HandRight or HandOrFootType.FootRight;

        foreach (var (bone, rotation) in Bones.Zip(right ? preset.RightRotations : preset.LeftRotations))
            bone.localRotation = rotation;
    }

    public HandOrFootPreset GetPresetData() =>
        new(Bones.Select(bone => bone.localRotation), Type);
}
