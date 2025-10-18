using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public abstract class CharacterDragHandleController : DragHandleControllerBase, ICharacterDragHandleController
{
    private readonly CharacterController characterController;
    private readonly List<BoneBackup> boneBackups = [];

    private bool boneMode;
    private DragHandleMode ignore;
    private bool iKEnabled = true;

    public CharacterDragHandleController(
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        TabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        TabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
    }

    public CharacterDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController characterUndoRedoController,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, gizmo)
    {
        CharacterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        UndoRedoController = characterUndoRedoController ?? throw new ArgumentNullException(nameof(characterUndoRedoController));
        SelectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        TabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
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

            CurrentMode.OnModeEnter();
        }
    }

    public bool AutoSelect { get; set; }

    public bool AutoSelectTab { get; set; }

    public virtual DragHandleMode Ignore =>
        ignore ??= new IgnoreMode(this);

    protected abstract Transform[] Transforms { get; }

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

    protected SelectionController<CharacterController> SelectionController { get; set; }

    protected TabSelectionController TabSelectionController { get; set; }

    protected AnimationController AnimationController =>
        CharacterController.Animation;

    protected IKController IKController =>
        CharacterController.IK;

    protected HeadController HeadController =>
        CharacterController.Head;

    protected CharacterUndoRedoController UndoRedoController { get; }

    protected override void OnDestroying() =>
        characterController.ChangedTransform -= ResizeDragHandle;

    protected void BackupBoneRotations()
    {
        boneBackups.Clear();
        boneBackups.AddRange(CreateBackup());
    }

    protected void ApplyBackupBoneRotations()
    {
        foreach (var backup in boneBackups)
            backup.Apply();
    }

    protected virtual IEnumerable<BoneBackup> CreateBackup() =>
        Transforms.Select(BoneBackup.Create);

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

            controller.BackupBoneRotations();

            if (controller.AutoSelect)
                controller.SelectionController.Select(controller.CharacterController);

            if (controller.AutoSelectTab)
                controller.TabSelectionController.SelectTab(MainWindow.Tab.Character);
        }

        public override void OnReleased() =>
            controller.UndoRedoController.EndPoseChange();

        public override void OnGizmoClicked()
        {
            controller.UndoRedoController.StartPoseChange();
            controller.BackupBoneRotations();

            if (controller.AutoSelect)
                controller.SelectionController.Select(controller.CharacterController);

            if (controller.AutoSelectTab)
                controller.TabSelectionController.SelectTab(MainWindow.Tab.Character);
        }

        public override void OnGizmoReleased() =>
            controller.UndoRedoController.EndPoseChange();

        public override void OnCancelled() =>
            controller.ApplyBackupBoneRotations();

        public override void OnGizmoCancelled() =>
            controller.ApplyBackupBoneRotations();
    }

    // TODO: Refactor various controls with a "none" mode to use this instead
    protected class IgnoreMode(CharacterDragHandleController controller)
        : PoseableMode(controller)
    {
        private readonly CharacterDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }
}
