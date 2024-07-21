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
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController)
    {
        Bone = bone ? bone : throw new ArgumentNullException(nameof(bone));
        IKTarget = ikTarget ? ikTarget : throw new ArgumentNullException(nameof(ikTarget));

        rotationLimit = IKController.GetRotationLimit(Bone.name);
    }

    protected CharacterIKDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, characterController)
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
        : DragHandleMode
    {
        private readonly CharacterIKDragHandleController controller = controller
            ?? throw new ArgumentNullException(nameof(controller));

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
        }

        public override void OnClicked()
        {
            clicked = true;
            controller.AnimationController.Playing = false;
            controller.IKTarget.position = controller.Bone.position;
            controller.IKController.SetSolverTarget(controller.IKTarget);
            controller.IKController.SetChain(chain);
            controller.IKController.UnlockSolver();
        }

        public override void OnDragging()
        {
            if (!clicked)
                OnClicked();

            controller.IKController.FixLocalPositions();
        }

        public override void OnReleased()
        {
            clicked = false;
            controller.IKController.LockSolver();
            controller.IKTarget.position = controller.Bone.position;
            controller.IKController.FixLocalPositions();
        }
    }
}
