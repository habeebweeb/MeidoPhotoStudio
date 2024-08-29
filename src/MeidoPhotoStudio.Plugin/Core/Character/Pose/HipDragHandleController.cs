using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HipDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    Transform spineSegment)
    : CharacterDragHandleController(dragHandle, gizmo, characterController, undoRedoController)
{
    private readonly Transform spineSegment = spineSegment ? spineSegment : throw new ArgumentNullException(nameof(spineSegment));

    private NoneMode none;
    private RotateMode rotate;
    private MoveYMode moveY;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode MoveY =>
        moveY ??= new MoveYMode(this);

    private class NoneMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.Visible = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
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

    private class MoveYMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.Visible = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.Y;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }
    }

    private class RotateMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.GizmoActive = controller.BoneMode;
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }
}
