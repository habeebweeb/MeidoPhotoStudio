using System;
using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleRepository
{
    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<LightController, LightDragHandleController> lightDragHandleControllers = new();

    public LightDragHandleRepository(
        GeneralDragHandleInputHandler generalDragHandleInputService,
        LightRepository lightRepository,
        SelectionController<LightController> lightSelectionController,
        TabSelectionController tabSelectionController)
    {
        this.generalDragHandleInputService = generalDragHandleInputService ?? throw new ArgumentNullException(nameof(generalDragHandleInputService));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.lightRepository.AddedLight += OnAddedLight;
        this.lightRepository.RemovingLight += OnRemovingLight;
    }

    private void OnAddedLight(object sender, LightRepositoryEventArgs e)
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
            }.Build();

            var lightDragHandleController = new LightDragHandleController(
                    dragHandle, lightController, lightRepository, lightSelectionController, tabSelectionController);

            return lightDragHandleController;
        }
    }

    private void OnRemovingLight(object sender, LightRepositoryEventArgs e)
    {
        if (!lightDragHandleControllers.ContainsKey(e.LightController))
            return;

        var lightDragHandleController = lightDragHandleControllers[e.LightController];

        lightDragHandleController.Destroy();
        generalDragHandleInputService.RemoveController(lightDragHandleController);

        lightDragHandleControllers.Remove(e.LightController);
    }
}
