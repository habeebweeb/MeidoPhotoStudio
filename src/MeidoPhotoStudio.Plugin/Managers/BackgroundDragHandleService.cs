using System;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundDragHandleService
{
    private readonly GeneralDragPointInputService generalDragPointInputService;

    private BackgroundDragHandleController backgroundDragHandleController;
    private bool enabled = false;

    public BackgroundDragHandleService(GeneralDragPointInputService generalDragPointInputService, BackgroundService backgroundService)
    {
        this.generalDragPointInputService = generalDragPointInputService ?? throw new ArgumentNullException(nameof(generalDragPointInputService));
        _ = backgroundService ?? throw new ArgumentNullException(nameof(backgroundService));

        backgroundService.ChangingBackground += OnChangingBackground;
        backgroundService.ChangedBackground += OnChangedBackground;
    }

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            if (backgroundDragHandleController is null)
                return;

            backgroundDragHandleController.Enabled = enabled;
        }
    }

    private void CreateDragHandle(BackgroundModel backgroundModel, Transform backgroundTransform)
    {
        if (!backgroundTransform)
            return;

        var dragHandle = new DragHandle.Builder()
        {
            Name = $"[Background Drag Handle ({backgroundModel.AssetName})]",
            Target = backgroundTransform,
            ConstantSize = true,
            Scale = Vector3.one * 0.12f,
            PositionDelegate = () => backgroundTransform.position,
        }.Build();

        backgroundDragHandleController = new(dragHandle, backgroundTransform)
        {
            Enabled = Enabled,
        };

        backgroundDragHandleController.Enabled = Enabled;
        generalDragPointInputService.AddDragHandle(backgroundDragHandleController);
    }

    private void OnChangingBackground(object sender, BackgroundChangeEventArgs e)
    {
        if (backgroundDragHandleController is null)
            return;

        generalDragPointInputService.RemoveDragHandle(backgroundDragHandleController);
        backgroundDragHandleController.Destroy();
        backgroundDragHandleController = null;
    }

    private void OnChangedBackground(object sender, BackgroundChangeEventArgs e) =>
        CreateDragHandle(e.BackgroundInfo, e.BackgroundTransform);
}
