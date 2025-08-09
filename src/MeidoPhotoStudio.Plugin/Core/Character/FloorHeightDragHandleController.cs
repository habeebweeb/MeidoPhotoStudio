using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class FloorHeightDragHandleController : DragHandleControllerBase, IColourableDragHandle
{
    private readonly CharacterController characterController;
    private readonly SelectionController<CharacterController> selectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly GameObject floorHeightTarget;

    private float floorHeightBackup;
    private DragHandleMode normalMode;
    private DragHandleMode ignoreMode;

    public FloorHeightDragHandleController(
        DragHandle dragHandle,
        CharacterController characterController,
        GameObject floorHeightTarget,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle)
    {
        this.characterController = characterController ?? throw new ArgumentNullException(nameof(characterController));
        this.floorHeightTarget = floorHeightTarget ? floorHeightTarget : throw new ArgumentNullException(nameof(floorHeightTarget));
        this.selectionController = selectionController ?? throw new ArgumentNullException(nameof(selectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.characterController.Clothing.PropertyChanged += OnClothingPropertyChanged;

        CurrentMode = new AdjustFloorHeightMode(this);
    }

    public Color DragHandleColour
    {
        get => DragHandle.Color;
        set => DragHandle.Color = value;
    }

    public bool AutoSelect { get; set; }

    public bool AutoSelectTab { get; set; }

    public DragHandleMode AdjustMode =>
        normalMode ??= new AdjustFloorHeightMode(this);

    public DragHandleMode Ignore =>
        ignoreMode ??= new IgnoreMode(this);

    protected override void OnDestroying()
    {
        base.OnDestroying();

        characterController.Clothing.PropertyChanged -= OnClothingPropertyChanged;

        if (floorHeightTarget)
            Object.Destroy(floorHeightTarget);
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not nameof(ClothingController.CustomFloorHeight))
            return;

        var clothing = (ClothingController)sender;

        if (!clothing.CustomFloorHeight)
            DragHandleEnabled = false;
    }

    private abstract class Mode(FloorHeightDragHandleController controller)
        : DragHandleMode
    {
        protected float FloorHeightBackup
        {
            get => controller.floorHeightBackup;
            set => controller.floorHeightBackup = value;
        }

        protected Transform CharacterTransform =>
            Controller.characterController.Transform;

        protected ClothingController Clothing =>
            Controller.characterController.Clothing;

        protected Transform Target =>
            Controller.floorHeightTarget.transform;

        protected FloorHeightDragHandleController Controller =>
            controller;

        public override void OnClicked()
        {
            Target.position = CharacterTransform.position with { y = Clothing.FloorHeight };

            if (Controller.AutoSelect)
                Controller.selectionController.Select(Controller.characterController);

            if (Controller.AutoSelectTab)
                Controller.tabSelectionController.SelectTab(MainWindow.Tab.Character);
        }

        public override void OnCancelled()
        {
            Clothing.FloorHeight = FloorHeightBackup;
            Target.position = CharacterTransform.position with { y = Clothing.FloorHeight };
        }
    }

    private class AdjustFloorHeightMode(FloorHeightDragHandleController controller) : Mode(controller)
    {
        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            Controller.DragHandle.MovementType = DragHandle.MoveType.Y;
        }

        public override void OnDragging() =>
            Clothing.FloorHeight = Target.position.y;

        public override void OnDoubleClicked()
        {
            Clothing.FloorHeight = 0f;
            Target.position = CharacterTransform.position with { y = Clothing.FloorHeight };
        }
    }

    private class IgnoreMode(FloorHeightDragHandleController controller) : Mode(controller)
    {
        public override void OnModeEnter() =>
            Controller.DragHandleActive = false;
    }
}
