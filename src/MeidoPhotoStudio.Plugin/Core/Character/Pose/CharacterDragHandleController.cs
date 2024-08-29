using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public abstract class CharacterDragHandleController : DragHandleControllerBase, ICharacterDragHandleController
{
    private readonly CharacterController characterController;
    private bool boneMode;
    private DragHandleMode ignore;
    private bool iKEnabled = true;

    public CharacterDragHandleController(
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController)
        : base(gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController)
        : base(dragHandle)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController)
        : base(dragHandle, gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
    }

    public bool BoneMode
    {
        get => boneMode;
        set
        {
            boneMode = value;

            CurrentMode.OnModeEnter();
        }
    }

    public override bool Enabled
    {
        get => base.Enabled && IKEnabled;
        set => base.Enabled = value;
    }

    public override bool GizmoEnabled
    {
        get => base.GizmoEnabled && IKEnabled;
        set => base.GizmoEnabled = value;
    }

    public bool IKEnabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed.")
                : iKEnabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            iKEnabled = value;
            Enabled = value;
            GizmoEnabled = value;

            CurrentMode.OnModeEnter();
        }
    }

    public virtual DragHandleMode Ignore =>
        ignore ??= new IgnoreMode(this);

    protected CharacterController CharacterController
    {
        get => characterController;
        private init
        {
            characterController = value;

            if (DragHandle)
                characterController.ChangedTransform += ResizeDragHandle;
        }
    }

    protected AnimationController AnimationController =>
        CharacterController.Animation;

    protected IKController IKController =>
        CharacterController.IK;

    protected HeadController HeadController =>
        CharacterController.Head;

    protected CharacterUndoRedoController UndoRedoController { get; }

    protected override void OnDestroying() =>
        characterController.ChangedTransform -= ResizeDragHandle;

    private void ResizeDragHandle(object sender, TransformChangeEventArgs e)
    {
        if (!DragHandle || e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = CharacterController.GameObject.transform.localScale.x;
    }

    protected abstract class PoseableMode(CharacterDragHandleController controller)
        : DragHandleMode
    {
        private readonly CharacterDragHandleController controller = controller;

        public override void OnClicked()
        {
            controller.UndoRedoController.StartPoseChange();
            controller.IKController.Dirty = true;
        }

        public override void OnReleased() =>
            controller.UndoRedoController.EndPoseChange();

        public override void OnGizmoClicked() =>
            controller.UndoRedoController.StartPoseChange();

        public override void OnGizmoReleased() =>
            controller.UndoRedoController.EndPoseChange();
    }

    protected class IgnoreMode(CharacterDragHandleController controller)
        : PoseableMode(controller)
    {
        private readonly CharacterDragHandleController controller = controller;

        private bool exiting;

        public override void OnModeEnter()
        {
            if (exiting)
                return;

            controller.Enabled = false;
            controller.GizmoEnabled = false;
        }

        public override void OnModeExit()
        {
            exiting = true;
            controller.Enabled = true;
            controller.GizmoEnabled = true;
            exiting = false;
        }
    }
}
