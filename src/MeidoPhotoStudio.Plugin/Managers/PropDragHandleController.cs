using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropDragHandleController : GeneralDragHandleController
{
    private readonly PropController propController;
    private readonly PropService propService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TabSelectionController tabSelectionController;

    private UpdateTransformMode moveWorldXZ;
    private UpdateTransformMode moveWorldY;
    private UpdateTransformMode rotateLocalXZ;
    private UpdateTransformMode rotateWorldY;
    private UpdateTransformMode rotateLocalY;
    private UpdateTransformMode scale;
    private PropSelectMode select;
    private PropDeleteMode delete;

    public PropDragHandleController(
        DragHandle dragHandle,
        Transform target,
        CustomGizmo gizmo,
        PropController propController,
        PropService propService,
        SelectionController<PropController> propSelectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, gizmo, target)
    {
        this.propController = propController ?? throw new ArgumentNullException(nameof(propController));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));

        Gizmo.gameObject.SetActive(false);
        Gizmo.GizmoDrag += OnGizmoDragged;
    }

    public override GeneralDragHandleMode<GeneralDragHandleController> MoveWorldXZ =>
        moveWorldXZ ??= new UpdateTransformMode(this, base.MoveWorldXZ);

    public override GeneralDragHandleMode<GeneralDragHandleController> MoveWorldY =>
        moveWorldY ??= new UpdateTransformMode(this, base.MoveWorldY);

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalXZ =>
        rotateLocalXZ ??= new UpdateTransformMode(this, base.RotateLocalXZ);

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateWorldY =>
        rotateWorldY ??= new UpdateTransformMode(this, base.RotateWorldY);

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalY =>
        rotateLocalY ??= new UpdateTransformMode(this, base.RotateLocalY);

    public override GeneralDragHandleMode<GeneralDragHandleController> Scale =>
        scale ??= new UpdateTransformMode(this, base.Scale);

    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        select ??= new PropSelectMode(this);

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        delete ??= new PropDeleteMode(this);

    public float HandleSize
    {
        get => DragHandle.Size;
        set => DragHandle.Size = value;
    }

    public float GizmoSize
    {
        get => Gizmo.offsetScale;
        set => Gizmo.offsetScale = value;
    }

    private void UpdatePropTransform() =>
        propController.UpdateTransform();

    private void OnGizmoDragged(object sender, EventArgs e) =>
        UpdatePropTransform();

    private class PropSelectMode : SelectMode
    {
        public PropSelectMode(PropDragHandleController controller)
            : base(controller) =>
            Controller = controller;

        private new PropDragHandleController Controller { get; }

        public override void OnClicked()
        {
            Controller.propSelectionController.Select(Controller.propController);
            Controller.tabSelectionController.SelectTab(Constants.Window.BG2);
        }

        public override void OnDoubleClicked() =>
            Controller.propController.Focus();
    }

    private class PropDeleteMode : DeleteMode
    {
        public PropDeleteMode(PropDragHandleController controller)
            : base(controller) =>
            Controller = controller;

        private new PropDragHandleController Controller { get; }

        public override void OnClicked() =>
            Controller.propService.Remove(Controller.propController);
    }

    private class UpdateTransformMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        private readonly GeneralDragHandleMode<GeneralDragHandleController> mode;

        public UpdateTransformMode(PropDragHandleController controller, GeneralDragHandleMode<GeneralDragHandleController> mode)
            : base(controller)
        {
            Controller = controller;
            this.mode = mode ?? throw new ArgumentNullException(nameof(mode));
        }

        private new PropDragHandleController Controller { get; }

        public override void OnModeEnter() =>
            mode.OnModeEnter();

        public override void OnClicked() =>
            mode.OnClicked();

        public override void OnDragging()
        {
            mode.OnDragging();

            Controller.UpdatePropTransform();
        }

        public override void OnReleased() =>
            mode.OnReleased();

        public override void OnDoubleClicked()
        {
            mode.OnDoubleClicked();

            Controller.UpdatePropTransform();
        }
    }
}
