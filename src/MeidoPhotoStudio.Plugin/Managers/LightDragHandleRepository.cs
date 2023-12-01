using System;
using System.Collections.Generic;

using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleRepository
{
    private readonly GeneralDragPointInputService generalDragPointInputService;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly Dictionary<LightController, LightDragHandleController> lightDragHandleControllers = new();

    public LightDragHandleRepository(
        GeneralDragPointInputService generalDragPointInputService,
        LightRepository lightRepository,
        SelectionController<LightController> lightSelectionController)
    {
        this.generalDragPointInputService = generalDragPointInputService ?? throw new ArgumentNullException(nameof(generalDragPointInputService));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));

        this.lightRepository.AddedLight += OnAddedLight;
        this.lightRepository.RemovingLight += OnRemovingLight;
    }

    private void OnAddedLight(object sender, LightRepositoryEventArgs e)
    {
        var lightDragHandleController = BuildDragHandle(e.LightController);

        generalDragPointInputService.AddDragHandle(lightDragHandleController);

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
                    dragHandle, lightController, lightRepository, lightSelectionController);

            return lightDragHandleController;
        }
    }

    private void OnRemovingLight(object sender, LightRepositoryEventArgs e)
    {
        if (!lightDragHandleControllers.ContainsKey(e.LightController))
            return;

        var lightDragHandleController = lightDragHandleControllers[e.LightController];

        lightDragHandleController.Destroy();
        generalDragPointInputService.RemoveDragHandle(lightDragHandleController);

        lightDragHandleControllers.Remove(e.LightController);
    }
}
