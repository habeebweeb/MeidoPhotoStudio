using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropDragHandleService : IEnumerable<PropDragHandleController>
{
    private readonly (float Small, float Normal) handleSize = (0.5f, 1f);
    private readonly (float Small, float Normal) gizmoSize = (0.225f, 0.45f);

    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;
    private readonly PropService propService;
    private readonly SelectionController<PropController> propSelectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<PropController, PropDragHandleController> propDragHandleControllers = [];

    private bool smallHandle;

    public PropDragHandleService(
        GeneralDragHandleInputHandler generalDragHandleInputService,
        PropService propService,
        SelectionController<PropController> propSelectionController,
        TabSelectionController tabSelectionController)
    {
        this.generalDragHandleInputService = generalDragHandleInputService ?? throw new ArgumentNullException(nameof(generalDragHandleInputService));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.propSelectionController = propSelectionController ?? throw new ArgumentNullException(nameof(propSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.propService.AddedProp += OnAddedProp;
        this.propService.RemovingProp += OnRemovingProp;
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            foreach (var controller in propDragHandleControllers.Values)
            {
                controller.HandleSize = smallHandle ? handleSize.Small : handleSize.Normal;
                controller.GizmoSize = smallHandle ? gizmoSize.Small : gizmoSize.Normal;
            }
        }
    }

    public PropDragHandleController this[PropController controller] =>
        controller is null
            ? throw new ArgumentNullException(nameof(controller))
            : propDragHandleControllers[controller];

    public IEnumerator<PropDragHandleController> GetEnumerator() =>
        propDragHandleControllers.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void OnAddedProp(object sender, PropServiceEventArgs e)
    {
        var propDragHandleController = BuildDragHandle(e.PropController);

        generalDragHandleInputService.AddController(propDragHandleController);

        propDragHandleControllers.Add(e.PropController, propDragHandleController);

        PropDragHandleController BuildDragHandle(PropController propController)
        {
            var propTransform = propController.GameObject.transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = "[MPS Prop]",
                Target = propTransform,
                ConstantSize = true,
                Scale = Vector3.one * 0.12f,
                Size = SmallHandle ? handleSize.Small : handleSize.Normal,
                PositionDelegate = () => propTransform.position,
            }.Build();

            var gizmo = new CustomGizmo.Builder()
            {
                Name = $"[MPS Prop Gizmo]",
                Target = propTransform,
                Size = SmallHandle ? gizmoSize.Small : gizmoSize.Normal,
                Mode = CustomGizmo.GizmoMode.World,
            }.Build();

            var propDragHandleController = new PropDragHandleController(
                dragHandle,
                propTransform,
                gizmo,
                propController,
                propService,
                propSelectionController,
                tabSelectionController);

            return propDragHandleController;
        }
    }

    private void OnRemovingProp(object sender, PropServiceEventArgs e)
    {
        var propDragHandleController = propDragHandleControllers[e.PropController];

        propDragHandleController.Destroy();
        generalDragHandleInputService.RemoveController(propDragHandleController);
        propDragHandleControllers.Remove(e.PropController);
    }
}
