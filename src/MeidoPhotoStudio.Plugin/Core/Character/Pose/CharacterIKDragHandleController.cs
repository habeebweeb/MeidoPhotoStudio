using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using RootMotion.FinalIK;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public abstract class CharacterIKDragHandleController : CharacterDragHandleController
{
    private readonly RotationLimit rotationLimit;
    private DragMode drag;

    protected CharacterIKDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController, characterUndoRedoController)
    {
        Bone = bone ? bone : throw new ArgumentNullException(nameof(bone));
        IKTarget = ikTarget ? ikTarget : throw new ArgumentNullException(nameof(ikTarget));

        rotationLimit = IKController.GetRotationLimit(Bone.name);
    }

    protected CharacterIKDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, characterController, characterUndoRedoController)
    {
        Bone = bone ? bone : throw new ArgumentNullException(nameof(bone));
        IKTarget = ikTarget ? ikTarget : throw new ArgumentNullException(nameof(ikTarget));

        rotationLimit = IKController.GetRotationLimit(Bone.name);
    }

    public virtual DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    protected abstract Transform[] Chain { get; }

    protected Transform Bone { get; }

    protected Transform IKTarget { get; }

    protected override void OnDestroying()
    {
        if (IKTarget)
            Object.Destroy(IKTarget.gameObject);
    }

    protected void LimitRotation()
    {
        if (!rotationLimit)
            return;

        rotationLimit.Apply();
    }

    protected class DragMode(CharacterIKDragHandleController controller, Transform[] chain)
        : PoseableMode(controller)
    {
        private readonly Transform[] chain = chain;

        private bool clicked = false;

        public override void OnModeEnter()
        {
            clicked = false;
            controller.DragHandleActive = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.All;
            controller.DragHandle.Visible = controller.BoneMode;

            controller.GizmoActive = false;
        }

        public override void OnModeExit()
        {
            clicked = false;
            controller.IKController.LockSolver();
            controller.IKTarget.position = controller.Bone.position;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            UpdateIKController();
        }

        public override void OnDragging()
        {
            if (!clicked)
                UpdateIKController();

            controller.IKController.FixLocalPositions();
        }

        public override void OnReleased()
        {
            base.OnReleased();

            clicked = false;
            controller.IKTarget.position = controller.Bone.position;
            controller.IKController.FixLocalPositions();
            controller.IKController.LockSolver();
        }

        public override void OnDoubleClicked() =>
            UpdateIKController();

        private void UpdateIKController()
        {
            clicked = true;
            controller.AnimationController.Playing = false;
            controller.IKTarget.position = controller.Bone.position;
            controller.IKController.SetSolverTarget(controller.IKTarget);
            controller.IKController.SetChain(chain);
            controller.IKController.UnlockSolver();
        }
    }
}
