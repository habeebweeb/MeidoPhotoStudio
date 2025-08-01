using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class CharacterGeneralDragHandleController : GeneralDragHandleController, ICharacterDragHandleController
{
    private readonly CharacterController character;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;

    private bool ikEnabled = true;
    private CharacterSelectMode select;
    private DragHandleMode moveWorldXZ;
    private DragHandleMode moveWorldY;
    private DragHandleMode rotateLocalXZ;
    private DragHandleMode rotateWorldY;
    private DragHandleMode rotateLocalY;
    private DragHandleMode scale;

    public CharacterGeneralDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        Transform target,
        CharacterController character,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, gizmo, target)
    {
        this.character = character ?? throw new ArgumentNullException(nameof(character));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.character.ChangedTransform += OnTransformChanged;

        TransformBackup = new(Space.World, Vector3.zero, Quaternion.identity, Vector3.one);
    }

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

    public override bool DragHandleEnabled
    {
        get => !IsCube || base.DragHandleEnabled;
        set
        {
            if (!IsCube)
                return;

            base.DragHandleEnabled = value;
        }
    }

    public bool IsCube { get; init; }

    public bool ScalesWithCharacter { get; init; }

    public bool BoneMode { get; set; }

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

            ikEnabled = IsCube || value;

            if (!ikEnabled)
                CurrentMode = None;
            else
                CurrentMode.OnModeEnter();
        }
    }

    public bool AutoSelect { get; set; }

    public bool AutoSelectTab { get; set; }

    public override DragHandleMode MoveWorldXZ =>
        IKEnabled ? moveWorldXZ ??= new TransformMode(this, base.MoveWorldXZ) : None;

    public override DragHandleMode MoveWorldY =>
        IKEnabled ? moveWorldY ??= new TransformMode(this, base.MoveWorldY) : None;

    public override DragHandleMode RotateLocalXZ =>
        IKEnabled ? rotateLocalXZ ??= new TransformMode(this, base.RotateLocalXZ) : None;

    public override DragHandleMode RotateWorldY =>
        IKEnabled ? rotateWorldY ??= new TransformMode(this, base.RotateWorldY) : None;

    public override DragHandleMode RotateLocalY =>
        IKEnabled ? rotateLocalY ??= new TransformMode(this, base.RotateLocalY) : None;

    public override DragHandleMode Scale =>
        IKEnabled ? scale ??= new TransformMode(this, new ScaleMode(this)) : None;

    public override DragHandleMode Select =>
        select ??= new CharacterSelectMode(this);

    public override DragHandleMode Delete =>
        None;

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

    private class ScaleMode(CharacterGeneralDragHandleController controller) : ScaleMode<CharacterGeneralDragHandleController>(controller)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            Controller.GizmoActive = false;
        }
    }

    private class TransformMode(
        CharacterGeneralDragHandleController controller,
        DragHandleMode originalMode)
        : WrapperDragHandleMode<DragHandleMode>(originalMode)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            if (controller.AutoSelect)
                controller.selectionController.Select(controller.character);

            if (controller.AutoSelectTab)
                controller.tabSelectionController.SelectTab(MainWindow.Tab.Character);
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            if (controller.AutoSelect)
                controller.selectionController.Select(controller.character);

            if (controller.AutoSelectTab)
                controller.tabSelectionController.SelectTab(MainWindow.Tab.Character);
        }
    }

    private class CharacterSelectMode(CharacterGeneralDragHandleController controller)
        : SelectMode<CharacterGeneralDragHandleController>(controller)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            Controller.selectionController.Select(Controller.character);
            Controller.tabSelectionController.SelectTab(MainWindow.Tab.CharacterPose, true);
        }

        public override void OnDoubleClicked() =>
            Controller.character.FocusOnBody();
    }
}
