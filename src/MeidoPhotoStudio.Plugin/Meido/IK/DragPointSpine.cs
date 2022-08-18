using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

public class DragPointSpine : DragPointMeido
{
    private Quaternion spineRotation;
    private bool isHip;
    private bool isThigh;
    private bool isHead;

    public override void AddGizmo(float scale = 0.25f, CustomGizmo.GizmoMode mode = CustomGizmo.GizmoMode.Local)
    {
        base.AddGizmo(scale, mode);

        if (isHead)
            Gizmo.GizmoDrag += (_, _) =>
                meido.HeadToCam = false;
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        isHip = myObject.name is "Bip01";
        isThigh = myObject.name.EndsWith("Thigh");
        isHead = myObject.name.EndsWith("Head");
    }

    protected override void ApplyDragType()
    {
        var current = CurrentDragType;

        if (IsBone && current is not DragType.Ignore)
        {
            if (!isHead && current is DragType.RotLocalXZ)
                ApplyProperties(false, false, isThigh);
            else if (!isThigh && current is DragType.MoveY)
                ApplyProperties(isHip, isHip, !isHip);
            else if (!isThigh && !isHead && current is DragType.RotLocalY)
                ApplyProperties(!isHip, !isHip, isHip);
            else
                ApplyProperties(!isThigh, !isThigh, false);
        }
        else
        {
            ApplyProperties(false, false, false);
        }
    }

    protected override void UpdateDragType()
    {
        var shift = Input.Shift;
        var alt = Input.Alt;

        if (OtherDragType())
        {
            CurrentDragType = DragType.Ignore;
        }
        else if (isThigh && !Input.Control && alt && shift)
        {
            // gizmo thigh rotation
            CurrentDragType = DragType.RotLocalXZ;
        }
        else if (alt)
        {
            CurrentDragType = DragType.Ignore;
        }
        else if (shift)
        {
            CurrentDragType = DragType.RotLocalY;
        }
        else if (Input.Control)
        {
            // hip y transform and spine gizmo rotation
            CurrentDragType = DragType.MoveY;
        }
        else
        {
            CurrentDragType = DragType.None;
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        spineRotation = MyObject.rotation;
    }

    protected override void Drag()
    {
        if (isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragType.None)
        {
            if (isHead)
                meido.HeadToCam = false;

            MyObject.rotation = spineRotation;
            MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 4.5f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
        }

        if (CurrentDragType is DragType.RotLocalY)
        {
            if (isHead)
                meido.HeadToCam = false;

            MyObject.rotation = spineRotation;
            MyObject.Rotate(Vector3.right * mouseDelta.x / 4f);
        }

        if (CurrentDragType is DragType.MoveY)
        {
            var cursorPosition = CursorPosition();

            MyObject.position = new(MyObject.position.x, cursorPosition.y, MyObject.position.z);
        }
    }
}
