using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightDragHandleController : GeneralDragHandleController
{
    private readonly bool isMainLight;
    private readonly LightController lightController;
    private readonly LightRepository lightRepository;
    private readonly LightSelectionController lightSelectionController;

    public LightDragHandleController(
        DragHandle dragHandle,
        LightController lightController,
        LightRepository lightRepository,
        LightSelectionController lightSelectionController)
        : base(dragHandle, LightControllerTransform(lightController))
    {
        this.lightController = lightController ?? throw new ArgumentNullException(nameof(lightController));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));

        isMainLight = this.lightController.Light == GameMain.Instance.MainLight.GetComponent<Light>();
    }

    protected override void OnDragHandleModeChanged()
    {
        base.OnDragHandleModeChanged();

        if (CurrentDragType is DragHandleMode.Delete)
        {
            if (isMainLight)
                DragHandle.gameObject.SetActive(false);
        }
    }

    protected override void Scale()
    {
        var delta = MouseDelta.y;

        if (lightController.Type is LightType.Directional)
            lightController.Intensity += delta * 0.1f;
        else if (lightController.Type is LightType.Point)
            lightController.Range += delta * 5f;
        else if (lightController.Type is LightType.Spot)
            lightController.SpotAngle += delta * 5f;
    }

    protected override void Select() =>
        lightSelectionController.Select(lightController);

    protected override void Delete()
    {
        if (isMainLight)
            return;

        lightRepository.RemoveLight(lightController);
    }

    protected override void ResetScale()
    {
        if (lightController.Type is LightType.Directional)
            lightController.Intensity = 0.95f;
        else if (lightController.Type is LightType.Point)
            lightController.Range = 10f;
        else if (lightController.Type is LightType.Spot)
            lightController.SpotAngle = 50f;
    }

    protected override void ResetRotation()
    {
        base.ResetRotation();

        UpdateControllerRotation();
    }

    protected override void RotateWorldY()
    {
        base.RotateWorldY();

        UpdateControllerRotation();
    }

    protected override void RotateLocalY()
    {
        base.RotateLocalY();

        UpdateControllerRotation();
    }

    protected override void RotateLocalXZ()
    {
        base.RotateLocalXZ();

        UpdateControllerRotation();
    }

    private static Transform LightControllerTransform(LightController lightController) =>
        lightController is null
            ? throw new ArgumentNullException(nameof(lightController))
            : lightController.Light.transform;

    private void UpdateControllerRotation() =>
        lightController.Rotation = Target.rotation;
}
