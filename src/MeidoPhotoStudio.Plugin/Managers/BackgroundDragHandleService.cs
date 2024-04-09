using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundDragHandleService
{
    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;

    private BackgroundDragHandleController backgroundDragHandleController;
    private bool enabled = false;

    public BackgroundDragHandleService(GeneralDragHandleInputHandler generalDragHandleInputService, BackgroundService backgroundService)
    {
        this.generalDragHandleInputService = generalDragHandleInputService ?? throw new ArgumentNullException(nameof(generalDragHandleInputService));
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
        generalDragHandleInputService.AddController(backgroundDragHandleController);
    }

    private void OnChangingBackground(object sender, BackgroundChangeEventArgs e)
    {
        if (backgroundDragHandleController is null)
            return;

        generalDragHandleInputService.RemoveController(backgroundDragHandleController);
        backgroundDragHandleController.Destroy();
        backgroundDragHandleController = null;
    }

    private void OnChangedBackground(object sender, BackgroundChangeEventArgs e) =>
        CreateDragHandle(e.BackgroundInfo, e.BackgroundTransform);
}
