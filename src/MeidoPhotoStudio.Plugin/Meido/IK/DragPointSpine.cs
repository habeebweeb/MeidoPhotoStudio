using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointSpine : DragPointMeido
{
    private Quaternion spineRotation;
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

        isHead = myObject.name.EndsWith("Head");
    }

    protected override void ApplyDragType()
    {
        var current = CurrentDragType;

        if (!IsBone || current is DragHandleMode.Ignore)
        {
            ApplyProperties(false, false, false);

            return;
        }

        if (current is DragHandleMode.None or DragHandleMode.SpineBoneRotation)
            ApplyProperties(true, true, false);
        else if (current is DragHandleMode.SpineBoneGizmoRotation)
            ApplyProperties(false, false, true);
        else
            ApplyProperties(false, false, false);
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

        if (CurrentDragType is DragHandleMode.None)
        {
            if (isHead)
                meido.HeadToCam = false;

            MyObject.rotation = spineRotation;
            MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 4.5f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
        }

        if (CurrentDragType is DragHandleMode.SpineBoneRotation)
        {
            if (isHead)
                meido.HeadToCam = false;

            MyObject.rotation = spineRotation;
            MyObject.Rotate(Vector3.right * mouseDelta.x / 4f);
        }
    }
}
