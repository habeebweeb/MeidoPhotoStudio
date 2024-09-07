using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleController : GeneralDragHandleController
{
    private readonly bool isMainLight;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly TabSelectionController tabSelectionController;

    private LightScaleValueBackup lightScaleBackup;
    private DragHandleMode none;
    private DragHandleMode moveWorldXZ;
    private DragHandleMode moveWorldY;
    private DragHandleMode rotateLocalXZ;
    private DragHandleMode rotateWorldY;
    private DragHandleMode rotateLocalY;
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

    public override DragHandleMode None =>
        none ??= new LightTransformMode(this, base.None);

    public override DragHandleMode MoveWorldXZ =>
        moveWorldXZ ??= new LightTransformMode(this, base.MoveWorldXZ);

    public override DragHandleMode MoveWorldY =>
        moveWorldY ??= new LightTransformMode(this, base.MoveWorldY);

    public override DragHandleMode RotateLocalXZ =>
        rotateLocalXZ ??= new LightTransformMode(this, base.RotateLocalXZ);

    public override DragHandleMode RotateWorldY =>
        rotateWorldY ??= new LightTransformMode(this, base.RotateWorldY);

    public override DragHandleMode RotateLocalY =>
        rotateLocalY ??= new LightTransformMode(this, base.RotateLocalY);

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

    private readonly record struct LightScaleValueBackup(LightType Type, float Value)
    {
        public static LightScaleValueBackup Create(LightController lightController)
        {
            _ = lightController ?? throw new ArgumentNullException(nameof(lightController));

            var value = lightController.Type switch
            {
                LightType.Directional => lightController.Intensity,
                LightType.Point => lightController.Range,
                LightType.Spot => lightController.SpotAngle,
                LightType.Area => throw new NotSupportedException($"{nameof(LightType.Area)} is not supported"),
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(
                    nameof(lightController.Type), (int)lightController.Type, typeof(LightType)),
            };

            return new(lightController.Type, value);
        }

        public void Apply(LightController lightController)
        {
            _ = lightController ?? throw new ArgumentNullException(nameof(lightController));

            if (Type is LightType.Directional)
                lightController.Intensity = Value;
            else if (Type is LightType.Point)
                lightController.Range = Value;
            else if (Type is LightType.Spot)
                lightController.SpotAngle = Value;
        }
    }

    private class LightTransformMode(
        LightDragHandleController controller,
        DragHandleMode originalMode)
        : WrapperDragHandleMode<DragHandleMode>(originalMode)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            controller.lightScaleBackup = LightScaleValueBackup.Create(controller.LightController);
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.lightScaleBackup.Apply(controller.LightController);
        }
    }

    private class LightSelectMode(LightDragHandleController controller) : SelectMode<LightDragHandleController>(controller)
    {
        public override void OnClicked()
        {
            base.OnClicked();

            Controller.lightScaleBackup = LightScaleValueBackup.Create(Controller.LightController);
            Controller.lightSelectionController.Select(Controller.LightController);
            Controller.tabSelectionController.SelectTab(Constants.Window.BG);
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            Controller.lightScaleBackup.Apply(Controller.LightController);
        }
    }

    private class LightScaleMode(LightDragHandleController controller) : ScaleMode<LightDragHandleController>(controller)
    {
        private LightController LightController =>
            Controller.LightController;

        public override void OnClicked()
        {
            base.OnClicked();

            Controller.lightScaleBackup = LightScaleValueBackup.Create(LightController);
        }

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

        public override void OnCancelled()
        {
            base.OnCancelled();

            Controller.lightScaleBackup.Apply(LightController);
        }
    }

    private class LightDeleteMode(LightDragHandleController controller) : DeleteMode<LightDragHandleController>(controller)
    {
        public override void OnClicked() =>
            Controller.lightRepository.RemoveLight(Controller.LightController);

        public override void OnCancelled()
        {
            base.OnCancelled();

            Controller.lightScaleBackup.Apply(Controller.LightController);
        }
    }
}
