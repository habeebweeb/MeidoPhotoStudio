using System;

using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class GeneralDragHandleController : DragHandleControllerBase
{
    public const float DragHandleAlpha = 0.75f;

    protected static readonly Color MoveColour = new(0.2f, 0.5f, 0.95f, DragHandleAlpha);
    protected static readonly Color RotateColour = new(0.2f, 0.75f, 0.3f, DragHandleAlpha);
    protected static readonly Color ScaleColour = new(0.8f, 0.7f, 0.3f, DragHandleAlpha);
    protected static readonly Color SelectColour = new(0.9f, 0.5f, 1f, DragHandleAlpha);
    protected static readonly Color DeleteColour = new(1f, 0.1f, 0.1f, DragHandleAlpha);

    public GeneralDragHandleController(DragHandle dragHandle, Transform target)
        : base(dragHandle)
    {
        Target = target ? target : throw new ArgumentNullException(nameof(target));

        DragHandle.Clicked.AddListener(OnClicked);
        DragHandle.Dragging.AddListener(OnDragging);
        DragHandle.Released.AddListener(OnRelease);
        DragHandle.DoubleClicked.AddListener(OnDoubleClicked);
        DragHandle.gameObject.SetActive(false);

        TransformBackup = new(Target);
    }

    public GeneralDragHandleController(DragHandle dragHandle, CustomGizmo gizmo, Transform target)
        : base(dragHandle, gizmo)
    {
        Target = target ? target : throw new ArgumentNullException(nameof(target));

        DragHandle.Clicked.AddListener(OnClicked);
        DragHandle.Dragging.AddListener(OnDragging);
        DragHandle.Released.AddListener(OnRelease);
        DragHandle.DoubleClicked.AddListener(OnDoubleClicked);
        DragHandle.gameObject.SetActive(false);

        TransformBackup = new(Target);
    }

    protected static Vector2 MouseDelta =>
        new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

    protected TransformBackup TransformBackup { get; }

    protected Transform Target { get; }

    protected override void OnDragHandleModeChanged()
    {
        switch (CurrentDragType)
        {
            case DragHandleMode.None:
                DragHandle.gameObject.SetActive(false);
                DragHandle.MovementType = DragHandle.MoveType.None;

                if (Gizmo)
                    Gizmo.gameObject.SetActive(false);

                break;
            case DragHandleMode.MoveWorldXZ:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.XZ;
                DragHandle.Color = MoveColour;

                if (Gizmo && GizmoEnabled)
                {
                    Gizmo.gameObject.SetActive(GizmoEnabled);
                    Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
                }

                break;
            case DragHandleMode.MoveWorldY:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.Y;
                DragHandle.Color = MoveColour;

                if (Gizmo)
                {
                    Gizmo.gameObject.SetActive(GizmoEnabled);
                    Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
                }

                break;
            case DragHandleMode.RotateWorldY or DragHandleMode.RotateLocalXZ or DragHandleMode.RotateLocalY:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.None;
                DragHandle.Color = RotateColour;

                if (Gizmo)
                {
                    Gizmo.gameObject.SetActive(GizmoEnabled);
                    Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
                }

                break;
            case DragHandleMode.Scale:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.None;
                DragHandle.Color = ScaleColour;

                if (Gizmo)
                {
                    Gizmo.gameObject.SetActive(GizmoEnabled);
                    Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Scale;
                }

                break;
            case DragHandleMode.Select:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.None;
                DragHandle.Color = SelectColour;

                break;
            case DragHandleMode.Delete:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.None;
                DragHandle.Color = DeleteColour;

                if (Gizmo)
                    Gizmo.gameObject.SetActive(false);

                break;
            default:
                DragHandle.gameObject.SetActive(Enabled);
                DragHandle.MovementType = DragHandle.MoveType.None;

                if (Gizmo)
                    Gizmo.gameObject.SetActive(false);

                break;
        }
    }

    protected virtual void RotateLocalXZ()
    {
        var cameraTransform = Camera.transform;
        var forward = cameraTransform.forward;
        var right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        var mouseDelta = MouseDelta;
        var mouseX = mouseDelta.x;
        var mouseY = mouseDelta.y;

        Target.Rotate(forward, -mouseX * 5f, Space.World);
        Target.Rotate(right, mouseY * 5f, Space.World);
    }

    protected virtual void RotateLocalY()
    {
        var mouseX = MouseDelta.x;

        Target.Rotate(Vector3.up, -mouseX * 5);
    }

    protected virtual void RotateWorldY()
    {
        var mouseX = MouseDelta.x;

        Target.Rotate(Vector3.up, -mouseX * 7, Space.World);
    }

    protected virtual void Scale()
    {
        var delta = MouseDelta.y * 0.1f;
        var currentScale = Target.localScale;
        var deltaScale = currentScale.normalized * delta;
        var newScale = currentScale + deltaScale;

        if (newScale.x < 0f || newScale.y < 0f || newScale.z < 0f)
            return;

        Target.localScale = newScale;
    }

    protected virtual void Select()
    {
    }

    protected virtual void Delete()
    {
    }

    protected virtual void ResetPosition() =>
        TransformBackup.ApplyPosition(Target);

    protected virtual void ResetRotation() =>
        TransformBackup.ApplyRotation(Target);

    protected virtual void ResetScale() =>
        TransformBackup.ApplyScale(Target);

    protected virtual void OnDoubleClicked()
    {
        if (CurrentDragType is DragHandleMode.MoveWorldXZ or DragHandleMode.MoveWorldY)
            ResetPosition();
        else if (CurrentDragType is DragHandleMode.RotateLocalXZ or DragHandleMode.RotateLocalY or DragHandleMode.RotateWorldY)
            ResetRotation();
        else if (CurrentDragType is DragHandleMode.Scale)
            ResetScale();
    }

    protected virtual void OnClicked()
    {
        if (CurrentDragType is DragHandleMode.Select)
            Select();
        else if (CurrentDragType is DragHandleMode.Delete)
            Delete();
    }

    protected virtual void OnDragging()
    {
        if (CurrentDragType is DragHandleMode.RotateWorldY)
            RotateWorldY();
        else if (CurrentDragType is DragHandleMode.RotateLocalY)
            RotateLocalY();
        else if (CurrentDragType is DragHandleMode.RotateLocalXZ)
            RotateLocalXZ();
        else if (CurrentDragType is DragHandleMode.Scale)
            Scale();
    }

    protected virtual void OnRelease()
    {
    }
}
