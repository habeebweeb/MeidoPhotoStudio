using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleService
{
    private static readonly (float Small, float Normal) HandleSize = (0.5f, 1f);

    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;
    private readonly LightService lightService;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<LightController, LightDragHandleController> lightDragHandleControllers = [];

    private bool smallHandle;
    private bool autoSelect;
    private bool autoSelectTab;

    public LightDragHandleService(
        GeneralDragHandleInputHandler generalDragHandleInputService,
        LightService lightService,
        SelectionController<LightController> lightSelectionController,
        TabSelectionController tabSelectionController)
    {
        this.generalDragHandleInputService = generalDragHandleInputService ?? throw new ArgumentNullException(nameof(generalDragHandleInputService));
        this.lightService = lightService ?? throw new ArgumentNullException(nameof(lightService));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.lightService.AddedLight += OnAddedLight;
        this.lightService.RemovingLight += OnRemovingLight;
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            foreach (var controller in lightDragHandleControllers.Values)
                controller.HandleSize = smallHandle ? HandleSize.Small : HandleSize.Normal;
        }
    }

    public bool AutoSelect
    {
        get => autoSelect;
        set
        {
            if (value == autoSelect)
                return;

            autoSelect = value;

            foreach (var controller in lightDragHandleControllers.Values)
                controller.AutoSelect = autoSelect;
        }
    }

    public bool AutoSelectTab
    {
        get => autoSelectTab;
        set
        {
            if (autoSelectTab == value)
                return;

            autoSelectTab = value;

            foreach (var controller in lightDragHandleControllers.Values)
                controller.AutoSelectTab = autoSelectTab;
        }
    }

    private void OnAddedLight(object sender, LightServiceEventArgs e)
    {
        var lightDragHandleController = BuildDragHandle(e.LightController);

        generalDragHandleInputService.AddController(lightDragHandleController);

        lightDragHandleControllers.Add(e.LightController, lightDragHandleController);

        LightDragHandleController BuildDragHandle(LightController lightController)
        {
            var lightTransform = lightController.Light.transform;

            var dragHandle = new DragHandle.Builder()
            {
                Name = "[MPS Light]",
                Target = lightTransform,
                Scale = Vector3.one * 0.12f,
                PositionDelegate = () => lightTransform.position,
                RotationDelegate = () => lightTransform.rotation,
                Size = SmallHandle ? HandleSize.Small : HandleSize.Normal,
            }.Build();

            var lightDragHandleController = new LightDragHandleController(
                    dragHandle, lightController, lightService, lightSelectionController, tabSelectionController)
            {
                AutoSelect = AutoSelect,
                AutoSelectTab = AutoSelectTab,
            };

            return lightDragHandleController;
        }
    }

    private void OnRemovingLight(object sender, LightServiceEventArgs e)
    {
        if (!lightDragHandleControllers.ContainsKey(e.LightController))
            return;

        var lightDragHandleController = lightDragHandleControllers[e.LightController];

        lightDragHandleController.Destroy();
        generalDragHandleInputService.RemoveController(lightDragHandleController);

        lightDragHandleControllers.Remove(e.LightController);
    }
}
