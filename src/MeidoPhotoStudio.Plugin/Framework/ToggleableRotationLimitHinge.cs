using RootMotion.FinalIK;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Framework;

public class ToggleableRotationLimitHinge : RotationLimitHinge
{
    public bool Limited { get; set; }

    public override Quaternion LimitRotation(Quaternion rotation) =>
        Limited ? base.LimitRotation(rotation) : rotation;
}
