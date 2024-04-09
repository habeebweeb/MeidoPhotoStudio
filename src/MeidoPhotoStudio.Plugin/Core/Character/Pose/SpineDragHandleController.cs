using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class SpineDragHandleController : CharacterDragHandleController
{
    private readonly Transform spineSegment;
    private readonly bool isHead;

    private NoneMode none;
    private RotateMode rotate;
    private RotateAlternateMode rotateAlternate;

    public SpineDragHandleController(
        DragHandle dragHandle, CustomGizmo gizmo, CharacterController characterController, Transform spineSegment)
        : base(dragHandle, gizmo, characterController)
    {
        this.spineSegment = spineSegment ? spineSegment : throw new ArgumentNullException(nameof(spineSegment));

        isHead = spineSegment.name.EndsWith("Head");

        Gizmo.GizmoDrag += OnGizmoDragging;
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        rotateAlternate ??= new RotateAlternateMode(this);

    private void OnGizmoDragging(object sender, EventArgs e)
    {
        AnimationController.Playing = false;

        if (isHead)
            HeadController.HeadToCamera = false;
    }

    private class NoneMode(SpineDragHandleController controller)
        : DragHandleMode
    {
        private readonly SpineDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            controller.AnimationController.Playing = false;

            if (controller.isHead)
                controller.HeadController.HeadToCamera = false;
        }

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

    private class RotateMode(SpineDragHandleController controller)
        : DragHandleMode
    {
        private readonly SpineDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            controller.AnimationController.Playing = false;

            if (controller.isHead)
                controller.HeadController.HeadToCamera = false;
        }

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            controller.spineSegment.Rotate(Vector3.right, deltaX * 7f);
        }
    }

    private class RotateAlternateMode(SpineDragHandleController controller)
        : DragHandleMode
    {
        private readonly SpineDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = controller.BoneMode;
        }
    }
}
