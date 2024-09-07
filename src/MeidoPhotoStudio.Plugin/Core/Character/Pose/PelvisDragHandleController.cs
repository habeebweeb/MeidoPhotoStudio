using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class PelvisDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    Transform pelvisBone)
    : CharacterDragHandleController(dragHandle, gizmo, characterController, undoRedoController)
{
    private readonly Transform pelvisBone = pelvisBone;

    private NoneMode none;
    private RotateMode rotate;
    private RotateAlternateMode rotateAlternate;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        rotateAlternate ??= new RotateAlternateMode(this);

    protected override Transform[] Transforms { get; } = [pelvisBone];

    private class NoneMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.GizmoActive = false;
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
        }
    }

    private class RotateMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.GizmoActive = controller.BoneMode;
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

            controller.pelvisBone.Rotate(cameraForward, deltaX * 5f, Space.World);
            controller.pelvisBone.Rotate(cameraRight, deltaY * 5f, Space.World);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }

    private class RotateAlternateMode(PelvisDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            controller.pelvisBone.Rotate(Vector3.right, deltaX * 6f);
        }
    }
}
