using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;

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

        if (CurrentDragType is DragType.Ignore)
        {
            ApplyProperties();
        }
        else if (current is DragType.RotLocalXZ)
        {
            if (isLower)
                ApplyProperties(!isBone, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is DragType.RotLocalY)
        {
            if (isLower || isMiddle)
                ApplyProperties(!isBone, false, false);
            else if (isUpper)
                ApplyProperties(false, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is DragType.RotY)
        {
            if (isMiddle)
                ApplyProperties(false, false, isBone);
            else
                ApplyProperties();
        }
        else if (current is DragType.MoveXZ)
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

    protected override void UpdateDragType()
    {
        var control = Input.Control;
        var alt = Input.Alt;

        // Check for DragMove so that hand dragpoint is not in the way
        if (OtherDragType())
        {
            CurrentDragType = DragType.Ignore;
        }
        else if (control && !Input.GetKey(MpsKey.DragMove))
        {
            if (alt)
                CurrentDragType = DragType.RotY;
            else
                CurrentDragType = DragType.MoveXZ;
        }
        else if (alt)
        {
            // TODO: Rethink this formatting
            CurrentDragType = Input.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ;
        }
        else
        {
            CurrentDragType = Input.Shift
                ? DragType.Ignore
                : DragType.None;
        }
    }

    protected override void Drag()
    {
        if (isPlaying)
            meido.Stop = true;

        var altRotation = CurrentDragType is DragType.MoveXZ or DragType.RotY;

        if (CurrentDragType is DragType.None || altRotation)
        {
            var upperJoint = altRotation ? JointMiddle : JointUpper;

            Porc(IK, ikCtrlData, ikChain[upperJoint], ikChain[JointMiddle], ikChain[JointLower]);

            InitializeRotation();
        }

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragType.RotLocalY)
        {
            var joint = isMiddle ? JointUpper : JointLower;

            ikChain[joint].localRotation = jointRotation[joint];
            ikChain[joint].Rotate(Vector3.right * (-mouseDelta.x / 1.5f));
        }

        if (CurrentDragType is DragType.RotLocalXZ)
        {
            ikChain[JointLower].localRotation = jointRotation[JointLower];
            ikChain[JointLower].Rotate(Vector3.up * (foot * mouseDelta.x / 1.5f));
            ikChain[JointLower].Rotate(Vector3.forward * (foot * mouseDelta.y / 1.5f));
        }
    }
}
