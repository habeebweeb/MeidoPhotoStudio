using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HipDragHandleController : CharacterDragHandleController
{
    private readonly Transform spineSegment;

    private NoneMode none;
    private RotateMode rotate;
    private MoveYMode moveY;

    public HipDragHandleController(
        DragHandle dragHandle, CustomGizmo gizmo, CharacterController characterController, Transform spineSegment)
        : base(dragHandle, gizmo, characterController)
    {
        this.spineSegment = spineSegment ? spineSegment : throw new ArgumentNullException(nameof(spineSegment));

        Gizmo.GizmoDrag += OnGizmoDragging;
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode MoveY =>
        moveY ??= new MoveYMode(this);

    private void OnGizmoDragging(object sender, EventArgs e) =>
        AnimationController.Playing = false;

    private class NoneMode(HipDragHandleController controller)
        : DragHandleMode
    {
        private readonly HipDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.Visible = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            var cameraTransform = Camera.transform;
            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            controller.spineSegment.Rotate(cameraForward, -deltaX * 5f, Space.World);
            controller.spineSegment.Rotate(cameraRight, deltaY * 5f, Space.World);
        }
    }

    private class MoveYMode(HipDragHandleController controller)
        : DragHandleMode
    {
        private readonly HipDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.Visible = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.Y;
            controller.GizmoActive = false;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;
    }

    private class RotateMode(HipDragHandleController controller)
        : DragHandleMode
    {
        private readonly HipDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.GizmoActive = controller.BoneMode;
        }
    }
}
