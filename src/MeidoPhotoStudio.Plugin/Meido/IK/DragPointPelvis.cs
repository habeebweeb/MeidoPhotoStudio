using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPelvis : DragPointMeido
{
    private Quaternion pelvisRotation;

    protected override void ApplyDragType()
    {
        if (CurrentDragType is DragType.Ignore)
            ApplyProperties();
        else if (IsBone)
            ApplyProperties(false, false, false);
        else
            ApplyProperties(CurrentDragType is not DragType.None, false, false);
    }

    protected override void UpdateDragType() =>

        // TODO: Rethink this formatting
        CurrentDragType = Input.Alt && !Input.Control
            ? Input.Shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ
            : OtherDragType()
                ? DragType.Ignore
                : DragType.None;

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        pelvisRotation = MyObject.rotation;
    }

    protected override void Drag()
    {
        if (CurrentDragType is DragType.None)
            return;

        if (isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragType.RotLocalXZ)
        {
            MyObject.rotation = pelvisRotation;
            MyObject.Rotate(camera.transform.forward, mouseDelta.x / 6f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 4f, Space.World);
        }

        if (CurrentDragType is DragType.RotLocalY)
        {
            MyObject.rotation = pelvisRotation;
            MyObject.Rotate(Vector3.right * (mouseDelta.x / 2.2f));
        }
    }
}
