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
                LegacyDragHandleMode.RotateEyesChest or LegacyDragHandleMode.RotateEyesChestAlternate or LegacyDragHandleMode.Select;

            ApplyProperties(active, false, false);
        }
        else
        {
            ApplyProperties(CurrentDragType is not LegacyDragHandleMode.None, false, false);
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        if (CurrentDragType is LegacyDragHandleMode.Select)
            Select?.Invoke(this, EventArgs.Empty);

        headRotation = MyObject.rotation;

        eyeRotationL = meido.Body.quaDefEyeL.eulerAngles;
        eyeRotationR = meido.Body.quaDefEyeR.eulerAngles;
    }

    protected override void OnDoubleClick()
    {
        if (CurrentDragType is LegacyDragHandleMode.RotateEyesChest or LegacyDragHandleMode.RotateEyesChestAlternate)
        {
            meido.Body.quaDefEyeL = meido.DefaultEyeRotL;
            meido.Body.quaDefEyeR = meido.DefaultEyeRotR;
        }
        else if (CurrentDragType is LegacyDragHandleMode.RotateBody or LegacyDragHandleMode.RotateBodyAlternate)
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
        if (IsIK || CurrentDragType is LegacyDragHandleMode.Select)
            return;

        if (CurrentDragType is not LegacyDragHandleMode.RotateEyesChest and not LegacyDragHandleMode.RotateEyesChestAlternate
            && isPlaying)
            meido.Stop = true;

        var mouseDelta = MouseDelta();

        if (CurrentDragType is LegacyDragHandleMode.RotateBody)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 3f, Space.World);
            MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
        }

        if (CurrentDragType is LegacyDragHandleMode.RotateBodyAlternate)
        {
            MyObject.rotation = headRotation;
            MyObject.Rotate(Vector3.right * mouseDelta.x / 3f);
        }

        if (CurrentDragType is LegacyDragHandleMode.RotateEyesChest or LegacyDragHandleMode.RotateEyesChestAlternate)
        {
            var inv = CurrentDragType is LegacyDragHandleMode.RotateEyesChestAlternate ? -1 : 1;

            meido.Body.quaDefEyeL.eulerAngles =
                new(eyeRotationL.x, eyeRotationL.y - mouseDelta.x / 10f, eyeRotationL.z - mouseDelta.y / 10f);
            meido.Body.quaDefEyeR.eulerAngles =
                new(eyeRotationR.x, eyeRotationR.y + inv * mouseDelta.x / 10f, eyeRotationR.z + mouseDelta.y / 10f);
        }
    }
}
