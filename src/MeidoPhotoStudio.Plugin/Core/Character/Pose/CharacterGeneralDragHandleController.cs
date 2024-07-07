using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class CharacterGeneralDragHandleController : GeneralDragHandleController, ICharacterDragHandleController
{
    private readonly CharacterController character;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;

    private bool ikEnabled = true;
    private CharacterSelectMode select;

    public CharacterGeneralDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CharacterController character,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, target)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.character.ChangedTransform += OnTransformChanged;

        TransformBackup = new(Space.World, Vector3.zero, Quaternion.identity, Vector3.one);
    }

    public bool IsCube { get; init; }

    public bool ScalesWithCharacter { get; init; }

    public bool BoneMode { get; set; }

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

    public float HandleSize
    {
        get => DragHandle.Size;
        set
        {
            if (!IsCube)
                return;

            DragHandle.Size = value;
        }
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set
        {
            if (!IsCube)
                return;

            if (!Gizmo)
                return;

            Gizmo.offsetScale = value;
        }
    }

    public bool IKEnabled
    {
        get => Destroyed
            ? throw new InvalidOperationException("Drag handle controller is destroyed.")
            : IsCube || ikEnabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            if (IsCube)
                return;

            ikEnabled = value;
            Enabled = ikEnabled;
            GizmoEnabled = ikEnabled;

            CurrentMode.OnModeEnter();
        }
    }

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        None;

    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        IKEnabled ? select ??= new CharacterSelectMode(this) : None;

    protected override void OnDestroying() =>
        character.ChangedTransform -= OnTransformChanged;

    private void OnTransformChanged(object sender, TransformChangeEventArgs e)
    {
        if (!ScalesWithCharacter)
            return;

        if (e.Type is not TransformChangeEventArgs.TransformType.Scale)
            return;

        DragHandle.Size = character.GameObject.transform.localScale.x;
    }

    private class CharacterSelectMode(CharacterGeneralDragHandleController controller)
        : SelectMode(controller)
    {
        private new CharacterGeneralDragHandleController Controller { get; } = controller;

        public override void OnClicked()
        {
            Controller.selectionController.Select(Controller.character);
            Controller.tabSelectionController.SelectTab(Constants.Window.Pose);
        }

        public override void OnDoubleClicked() =>
            Controller.character.FocusOnBody();
    }
}
