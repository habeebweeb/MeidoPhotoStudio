using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleController : GeneralDragHandleController
{
    private readonly bool isMainLight;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly TabSelectionController tabSelectionController;

    private DragHandleMode scale;
    private DragHandleMode select;
    private DragHandleMode delete;

    public LightDragHandleController(
        DragHandle dragHandle,
        LightController lightController,
        LightRepository lightRepository,
        SelectionController<LightController> lightSelectionController,
        TabSelectionController tabSelectionController)
        : base(dragHandle, LightControllerTransform(lightController))
    {
        LightController = lightController ?? throw new ArgumentNullException(nameof(lightController));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        isMainLight = LightController.Light == GameMain.Instance.MainLight.GetComponent<Light>();
    }

    public override DragHandleMode Scale =>
        scale ??= new LightScaleMode(this);

    public override DragHandleMode Select =>
        select ??= new LightSelectMode(this);

    public override DragHandleMode Delete =>
        isMainLight ? None : delete ??= new LightDeleteMode(this);

    private LightController LightController { get; }

    private static Transform LightControllerTransform(LightController lightController) =>
        lightController is null
            ? throw new ArgumentNullException(nameof(lightController))
            : lightController.Light.transform;

    private class LightSelectMode(LightDragHandleController controller) : SelectMode<LightDragHandleController>(controller)
    {
        public override void OnClicked()
        {
            Controller.lightSelectionController.Select(Controller.LightController);
            Controller.tabSelectionController.SelectTab(Constants.Window.BG);
        }
    }

    private class LightScaleMode(LightDragHandleController controller) : ScaleMode<LightDragHandleController>(controller)
    {
        private LightController LightController =>
            Controller.LightController;

        public override void OnDoubleClicked()
        {
            if (LightController.Type is LightType.Directional)
                LightController.Intensity = 0.95f;
            else if (LightController.Type is LightType.Point)
                LightController.Range = 10f;
            else if (LightController.Type is LightType.Spot)
                LightController.SpotAngle = 50f;
        }

        public override void OnDragging()
        {
            var delta = MouseDelta.y;

            if (LightController.Type is LightType.Directional)
                LightController.Intensity += delta * 0.1f;
            else if (LightController.Type is LightType.Point)
                LightController.Range += delta * 5f;
            else if (LightController.Type is LightType.Spot)
                LightController.SpotAngle += delta * 5f;
        }
    }

    private class LightDeleteMode(LightDragHandleController controller) : DeleteMode<LightDragHandleController>(controller)
    {
        public override void OnClicked() =>
            Controller.lightRepository.RemoveLight(Controller.LightController);
    }
}
