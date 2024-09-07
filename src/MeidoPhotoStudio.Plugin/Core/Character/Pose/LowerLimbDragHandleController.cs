using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class LowerLimbDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    Transform bone,
    Transform ikTarget)
    : CharacterIKDragHandleController(dragHandle, gizmo, characterController, undoRedoController, bone, ikTarget)
{
    private DragHandleMode drag;
    private RotateMode rotate;
    private RotateAlternateMode rotateAlternate;
    private DragMode constrained;

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this, Bone.name.EndsWith("Foot"));

    public DragHandleMode RotateAlternate =>
        rotateAlternate ??= new RotateAlternateMode(this);

    public DragHandleMode Constrained =>
        constrained ??= new DragMode(this, [Bone.parent, Bone]);

    protected override Transform[] Chain { get; } = [bone.parent.parent, bone.parent, bone];

    private new class DragMode(LowerLimbDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Scale = controller.BoneMode ? Vector3.one * 0.04f : Vector3.one * 0.1f;
        }
    }

    private class RotateMode(LowerLimbDragHandleController controller, bool inverse)
        : PoseableMode(controller)
    {
        private readonly float invert = inverse ? -1f : 1f;

        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = controller.BoneMode;
            controller.GizmoActive = controller.BoneMode;
            controller.GizmoMode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
            controller.IKController.LockSolver();
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            controller.Bone.Rotate(Vector3.forward, invert * deltaY * 7f);
            controller.Bone.Rotate(Vector3.up, invert * deltaX * 7f);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }

    private class RotateAlternateMode(LowerLimbDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = controller.BoneMode;
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

            controller.Bone.Rotate(Vector3.right, -deltaX * 7f);
        }
    }
}
