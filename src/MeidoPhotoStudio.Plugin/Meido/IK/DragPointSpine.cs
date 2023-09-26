using UnityEngine;

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

        if (!IsBone || current is DragType.Ignore)
        {
            ApplyProperties(false, false, false);

            return;
        }

        if (isThigh)
            ApplyThighProperties(current);
        else if (isHip)
            ApplyHipProperties(current);
        else
            ApplySpineProperties(current);

        void ApplyThighProperties(DragType current)
        {
            if (current is DragType.RotLocalXZ)
                ApplyProperties(false, false, true);
            else
                ApplyProperties(false, false, false);
        }

        void ApplyHipProperties(DragType current)
        {
            if (current is DragType.None)
                ApplyProperties(true, true, false);
            else if (current is DragType.RotLocalY)
                ApplyProperties(false, false, true);
            else if (current is DragType.MoveY)
                ApplyProperties(true, true, false);
            else
                ApplyProperties(false, false, false);
        }

        void ApplySpineProperties(DragType current)
        {
            if (current is DragType.None)
                ApplyProperties(true, true, false);
            else if (current is DragType.RotLocalY)
                ApplyProperties(true, true, false);
            else if (current is DragType.MoveY)
                ApplyProperties(false, false, true);
            else
                ApplyProperties(false, false, false);
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
