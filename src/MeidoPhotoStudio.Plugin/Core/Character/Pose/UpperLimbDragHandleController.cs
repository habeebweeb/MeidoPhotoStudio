using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class UpperLimbDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController,
    Transform bone,
    Transform ikTarget)
    : CharacterIKDragHandleController(
        dragHandle,
        gizmo,
        characterController,
        undoRedoController,
        selectionController,
        tabSelectionController,
        bone,
        ikTarget)
{
    private DragHandleMode drag;
    private RotateMode rotate;
    private RotateBoneMode rotateBone;

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode Rotate =>
        BoneMode
            ? rotateBone ??= new(this)
            : rotate ??= new(this);

    protected override Transform[] Chain { get; } = [bone.parent, bone];

    private new class DragMode(UpperLimbDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Scale = controller.BoneMode ? Vector3.one * 0.04f : Vector3.one * 0.1f;
        }
    }

    private class RotateMode(UpperLimbDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = controller.BoneMode;
            controller.GizmoActive = false;
            controller.IKController.LockSolver();
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var parent = controller.Bone.parent;
            var (deltaX, _) = MouseDelta;

            parent.Rotate(Vector3.right, -deltaX * 7f);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }

    private class RotateBoneMode(UpperLimbDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = true;
            controller.GizmoMode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
            controller.IKController.LockSolver();
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }
}
