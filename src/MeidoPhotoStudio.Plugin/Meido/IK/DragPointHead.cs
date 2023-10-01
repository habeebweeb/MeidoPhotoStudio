using System;

using UnityEngine;

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
            var active = current is
                DragHandleMode.RotateEyesChest or DragHandleMode.RotateEyesChestAlternate or DragHandleMode.Select;

            ApplyProperties(active, false, false);
        }
        else
        {
            ApplyProperties(CurrentDragType is not DragHandleMode.None, false, false);
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        if (CurrentDragType is DragHandleMode.Select)
            Select?.Invoke(this, EventArgs.Empty);

        headRotation = MyObject.rotation;

        eyeRotationL = meido.Body.quaDefEyeL.eulerAngles;
        eyeRotationR = meido.Body.quaDefEyeR.eulerAngles;
    }

    protected override void OnDoubleClick()
    {
        if (CurrentDragType is DragHandleMode.RotateEyesChest or DragHandleMode.RotateEyesChestAlternate)
        {
            meido.Body.quaDefEyeL = meido.DefaultEyeRotL;
            meido.Body.quaDefEyeR = meido.DefaultEyeRotR;
        }
        else if (CurrentDragType is DragHandleMode.RotateBody or DragHandleMode.RotateBodyAlternate)
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
        if (IsIK || CurrentDragType is DragHandleMode.Select)
            return;

        if (CurrentDragType is not DragHandleMode.RotateEyesChest and not DragHandleMode.RotateEyesChestAlternate
            && isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is DragHandleMode.RotateBody)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 3f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
        }

        if (CurrentDragType is DragHandleMode.RotateBodyAlternate)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(Vector3.right * mouseDelta.x / 3f);
        }

        if (CurrentDragType is DragHandleMode.RotateEyesChest or DragHandleMode.RotateEyesChestAlternate)
        {
            var inv = CurrentDragType is DragHandleMode.RotateEyesChestAlternate ? -1 : 1;

            meido.Body.quaDefEyeL.eulerAngles =
                new(eyeRotationL.x, eyeRotationL.y - mouseDelta.x / 10f, eyeRotationL.z - mouseDelta.y / 10f);
            meido.Body.quaDefEyeR.eulerAngles =
                new(eyeRotationR.x, eyeRotationR.y + inv * mouseDelta.x / 10f, eyeRotationR.z + mouseDelta.y / 10f);
        }
    }
}
