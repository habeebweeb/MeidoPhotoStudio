using System;

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
        CustomGizmo gizmo, CharacterController characterController)
        : base(gizmo) =>
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

    public CharacterDragHandleController(
        DragHandle dragHandle, CharacterController characterController)
        : base(dragHandle) =>
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

    public CharacterDragHandleController(
        DragHandle dragHandle, CustomGizmo gizmo, CharacterController characterController)
        : base(dragHandle, gizmo) =>
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));

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

    protected override void OnDestroying() =>
        characterController.ChangedTransform -= ResizeDragHandle;

    private void ResizeDragHandle(object sender, TransformChangeEventArgs e)
    {
        if (!DragHandle || e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = CharacterController.GameObject.transform.localScale.x;
    }

    private class IgnoreMode(CharacterDragHandleController controller)
        : DragHandleMode
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
