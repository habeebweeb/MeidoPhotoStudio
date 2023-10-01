using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPelvis : DragPointMeido
{
    private Quaternion pelvisRotation;

    protected override void ApplyDragType()
    {
        if (IsBone)
            ApplyProperties(false, false, false);
        else
            ApplyProperties(CurrentDragType is not DragHandleMode.None, false, false);
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        pelvisRotation = MyObject.rotation;
    }

    protected override void Drag()
    {
        if (CurrentDragType is DragHandleMode.None)
            return;

        if (isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragHandleMode.RotateBody)
        {
            MyObject.rotation = pelvisRotation;
            MyObject.Rotate(camera.transform.forward, mouseDelta.x / 6f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 4f, Space.World);
        }

        if (CurrentDragType is DragHandleMode.RotateBodyAlternate)
        {
            MyObject.rotation = pelvisRotation;
            MyObject.Rotate(Vector3.right * (mouseDelta.x / 2.2f));
        }
    }
}
