using System;

using UnityEngine;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

public class DragPointHead : DragPointMeido
{
    private Quaternion headRotation;
    private Vector3 eyeRotationL;
    private Vector3 eyeRotationR;

    public event EventHandler Select;

    public bool IsIK { get; set; }

    public void Focus() =>
        meido.FocusOnFace();

    protected override void ApplyDragType()
    {
        if (IsBone)
        {
            var current = CurrentDragType;
            var active = current is DragType.MoveY or DragType.MoveXZ or DragType.Select;

            ApplyProperties(active, false, false);
        }
        else
        {
            ApplyProperties(CurrentDragType is not DragType.None, false, false);
        }
    }

    protected override void UpdateDragType()
    {
        var shift = Input.Shift;
        var alt = Input.Alt;

        if (alt && Input.Control)
        {
            // eyes
            CurrentDragType = shift
                ? DragType.MoveY
                : DragType.MoveXZ;
        }
        else if (alt)
        {
            // head
            CurrentDragType = shift
                ? DragType.RotLocalY
                : DragType.RotLocalXZ;
        }
        else
        {
            CurrentDragType = Input.GetKey(MpsKey.DragSelect)
                ? DragType.Select
                : DragType.None;
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        if (CurrentDragType is DragType.Select)
            Select?.Invoke(this, EventArgs.Empty);

        headRotation = MyObject.rotation;

        eyeRotationL = meido.Body.quaDefEyeL.eulerAngles;
        eyeRotationR = meido.Body.quaDefEyeR.eulerAngles;
    }

    protected override void OnDoubleClick()
    {
        if (CurrentDragType is DragType.MoveXZ or DragType.MoveY)
        {
            meido.Body.quaDefEyeL = meido.DefaultEyeRotL;
            meido.Body.quaDefEyeR = meido.DefaultEyeRotR;
        }
        else if (CurrentDragType is DragType.RotLocalY or DragType.RotLocalXZ)
        {
            meido.FreeLook = !meido.FreeLook;
        }
        else if (Selecting)
        {
            Focus();
        }
    }

    protected override void Drag()
    {
        if (IsIK || CurrentDragType is DragType.Select)
            return;

        if (CurrentDragType is not DragType.MoveXZ and not DragType.MoveY && isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragType.RotLocalXZ)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 3f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
        }

        if (CurrentDragType is DragType.RotLocalY)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(Vector3.right * mouseDelta.x / 3f);
        }

        if (CurrentDragType is DragType.MoveXZ or DragType.MoveY)
        {
            var inv = CurrentDragType is DragType.MoveY ? -1 : 1;

            meido.Body.quaDefEyeL.eulerAngles =
                new(eyeRotationL.x, eyeRotationL.y - mouseDelta.x / 10f, eyeRotationL.z - mouseDelta.y / 10f);
            meido.Body.quaDefEyeR.eulerAngles =
                new(eyeRotationR.x, eyeRotationR.y + inv * mouseDelta.x / 10f, eyeRotationR.z + mouseDelta.y / 10f);
        }
    }
}
