using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

public class DragPointFinger : DragPointMeido
{
    private static readonly Color DragpointColour = new(0.1f, 0.4f, 0.95f, DefaultAlpha);

    private readonly TBody.IKCMO ik = new();
    private readonly Quaternion[] jointRotation = new Quaternion[2];

    private IKCtrlData ikCtrlData;
    private Transform[] ikChain;
    private bool baseFinger;

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        var parentName = myObject.parent.name.Split(' ')[2];

        // Base finger names have the form 'FingerN' or 'ToeN' where N is a natural number
        baseFinger = parentName.Length is 7 or 4;
        ikChain = new Transform[2] { myObject.parent, myObject };

        ikCtrlData = IkCtrlData;
    }

    protected override void ApplyDragType()
    {
        if (baseFinger && CurrentDragType is DragType.RotLocalY)
            ApplyProperties(true, true, false);
        else if (CurrentDragType is DragType.MoveXZ)
            ApplyProperties(true, true, false);
        else
            ApplyProperties(false, false, false);

        ApplyColour(DragpointColour);
    }

    protected override void UpdateDragType() =>
        CurrentDragType = !Input.GetKey(MpsKey.DragFinger)
            ? DragType.None
            : Input.Shift
                ? DragType.RotLocalY
                : DragType.MoveXZ;

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        jointRotation[JointUpper] = ikChain[JointUpper].localRotation;
        jointRotation[JointMiddle] = ikChain[JointMiddle].localRotation;

        InitializeIK(ik, ikChain[JointUpper], ikChain[JointUpper], ikChain[JointMiddle]);
    }

    protected override void Drag()
    {
        if (isPlaying)
            meido.Stop = true;

        if (CurrentDragType is DragType.MoveXZ)
        {
            Porc(ik, ikCtrlData, ikChain[JointUpper], ikChain[JointUpper], ikChain[JointMiddle]);

            if (!baseFinger)
            {
                SetRotation(JointUpper);
                SetRotation(JointMiddle);
            }
            else
            {
                jointRotation[JointUpper] = ikChain[JointUpper].localRotation;
                jointRotation[JointMiddle] = ikChain[JointMiddle].localRotation;
            }
        }
        else if (CurrentDragType is DragType.RotLocalY)
        {
            var mouseDelta = MouseDelta();

            ikChain[JointUpper].localRotation = jointRotation[JointUpper];
            ikChain[JointUpper].Rotate(Vector3.right * (mouseDelta.x / 1.5f));
        }
    }

    private void SetRotation(int joint)
    {
        var rotation = jointRotation[joint].eulerAngles;

        rotation.z = ikChain[joint].localEulerAngles.z;
        ikChain[joint].localRotation = Quaternion.Euler(rotation);
    }
}
