using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointLimb : DragPointChain
{
    private int foot = 1;
    private bool isLower;
    private bool isMiddle;
    private bool isUpper;

    public override bool IsBone
    {
        set
        {
            base.IsBone = value;

            BaseScale = isBone ? BoneScale : OriginalScale;
        }
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        var name = myObject.name;

        foot = name.EndsWith("Foot") ? -1 : 1;
        isLower = name.EndsWith("Hand") || foot is -1;
        isMiddle = name.EndsWith("Calf") || name.EndsWith("Forearm");
        isUpper = !isMiddle && !isLower;

        if (isLower)
            ikChain[0] = ikChain[0].parent;
    }

    protected override void ApplyDragType()
    {
        var current = CurrentDragType;
        var isBone = IsBone;

        if (CurrentDragType is LegacyDragHandleMode.Ignore)
        {
            ApplyProperties();
        }
        else if (current is LegacyDragHandleMode.RotateBody)
        {
            if (isLower)
                ApplyProperties(!isBone, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is LegacyDragHandleMode.RotateBodyAlternate)
        {
            if (isLower || isMiddle)
                ApplyProperties(!isBone, false, false);
            else if (isUpper)
                ApplyProperties(false, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is LegacyDragHandleMode.DragMiddleBone)
        {
            if (isMiddle)
                ApplyProperties(false, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is LegacyDragHandleMode.DragLowerLimb)
        {
            if (isLower)
                ApplyProperties(true, isBone, false);
            else
                ApplyProperties();
        }
        else
        {
            ApplyProperties(true, isBone, false);
        }
    }

    protected override void Drag()
    {
        if (isPlaying)
            meido.Stop = true;

        var altRotation = CurrentDragType is LegacyDragHandleMode.DragLowerLimb or LegacyDragHandleMode.DragMiddleBone;

        if (CurrentDragType is LegacyDragHandleMode.None || altRotation)
        {
            var upperJoint = altRotation ? JointMiddle : JointUpper;

            Porc(IK, ikCtrlData, ikChain[upperJoint], ikChain[JointMiddle], ikChain[JointLower]);

            InitializeRotation();
        }

        var mouseDelta = MouseDelta();

        if (CurrentDragType is LegacyDragHandleMode.RotateBodyAlternate)
        {
            var joint = isMiddle ? JointUpper : JointLower;

            ikChain[joint].localRotation = jointRotation[joint];
            ikChain[joint].Rotate(Vector3.right * (-mouseDelta.x / 1.5f));
        }

        if (CurrentDragType is LegacyDragHandleMode.RotateBody)
        {
            ikChain[JointLower].localRotation = jointRotation[JointLower];
            ikChain[JointLower].Rotate(Vector3.up * (foot * mouseDelta.x / 1.5f));
            ikChain[JointLower].Rotate(Vector3.forward * (foot * mouseDelta.y / 1.5f));
        }
    }
}
