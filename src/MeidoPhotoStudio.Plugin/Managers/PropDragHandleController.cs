using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropDragHandleController : GeneralDragHandleController
{
    private readonly PropController propController;
    private readonly PropService propService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TabSelectionController tabSelectionController;

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
    }

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

    private class PropSelectMode(PropDragHandleController controller) : SelectMode(controller)
    {
        private new PropDragHandleController Controller { get; } = controller;

        public override void OnClicked()
        {
            Controller.propSelectionController.Select(Controller.propController);
            Controller.tabSelectionController.SelectTab(Constants.Window.BG2);
        }

        public override void OnDoubleClicked() =>
            Controller.propController.Focus();
    }

    private class PropDeleteMode(PropDragHandleController controller) : DeleteMode(controller)
    {
        private new PropDragHandleController Controller { get; } = controller;

        public override void OnClicked() =>
            Controller.propService.Remove(Controller.propController);
    }
}
