using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleController : GeneralDragHandleController
{
    private readonly bool isMainLight;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly TabSelectionController tabSelectionController;

    private WrappedRotationMode rotateLocalXZMode;
    private WrappedRotationMode rotateLocalYMode;
    private WrappedRotationMode rotateWorldYMode;
    private LightScaleMode scale;
    private LightSelectMode select;
    private LightDeleteMode delete;

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

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalXZ =>
        rotateLocalXZMode ??= new WrappedRotationMode(this, base.RotateLocalXZ);

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateLocalY =>
        rotateLocalYMode ??= new WrappedRotationMode(this, base.RotateLocalY);

    public override GeneralDragHandleMode<GeneralDragHandleController> RotateWorldY =>
        rotateWorldYMode ??= new WrappedRotationMode(this, base.RotateWorldY);

    public override GeneralDragHandleMode<GeneralDragHandleController> Scale =>
        scale ??= new LightScaleMode(this);

    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        select ??= new LightSelectMode(this);

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        delete ??= new LightDeleteMode(this);

    private LightController LightController { get; }

    private static Transform LightControllerTransform(LightController lightController) =>
        lightController is null
            ? throw new ArgumentNullException(nameof(lightController))
            : lightController.Light.transform;

    private void UpdateControllerRotation() =>
        LightController.Rotation = Target.rotation;

    private class LightSelectMode(LightDragHandleController controller) : SelectMode(controller)
    {
        private new LightDragHandleController Controller { get; } = controller;

        public override void OnClicked()
        {
            Controller.lightSelectionController.Select(Controller.LightController);
            Controller.tabSelectionController.SelectTab(Constants.Window.BG);
        }
    }

    private class LightScaleMode(LightDragHandleController controller) : ScaleMode(controller)
    {
        // NOTE: No covariant returns and I don't want to cast every update tick.
        private new LightDragHandleController Controller { get; } = controller;

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

    private class LightDeleteMode(LightDragHandleController controller) : DeleteMode(controller)
    {
        private new LightDragHandleController Controller { get; } = controller;

        public override void OnModeEnter()
        {
            if (Controller.isMainLight)
            {
                DragHandle.gameObject.SetActive(false);
                DragHandle.MovementType = DragHandle.MoveType.None;

                if (Gizmo)
                    Gizmo.gameObject.SetActive(false);
            }
            else
            {
                base.OnModeEnter();
            }
        }

        public override void OnClicked()
        {
            if (Controller.isMainLight)
                return;

            Controller.lightRepository.RemoveLight(Controller.LightController);
        }
    }

    private abstract class LightDragHandleRotateMode(LightDragHandleController controller)
        : GeneralDragHandleRotateMode(controller)
    {
        private new LightDragHandleController Controller { get; } = controller;

        public override void OnDoubleClicked() =>
            Controller.UpdateControllerRotation();
    }

    private class WrappedRotationMode(
        LightDragHandleController controller, GeneralDragHandleMode<GeneralDragHandleController> rotateMode)
        : LightDragHandleRotateMode(controller)
    {
        private readonly GeneralDragHandleMode<GeneralDragHandleController> rotateMode = rotateMode
            ?? throw new ArgumentNullException(nameof(rotateMode));

        private new LightDragHandleController Controller { get; } = controller;

        public override void OnDragging()
        {
            rotateMode.OnDragging();

            Controller.UpdateControllerRotation();
        }
    }
}
