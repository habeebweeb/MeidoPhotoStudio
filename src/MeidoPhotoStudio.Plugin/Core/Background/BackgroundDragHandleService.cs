using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundDragHandleService
{
    private static readonly (float Small, float Normal) HandleSize = (0.5f, 1f);

    private readonly GeneralDragHandleInputHandler generalDragHandleInputService;

    private BackgroundDragHandleController backgroundDragHandleController;
    private bool enabled = false;
    private bool smallHandle;

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

            backgroundDragHandleController.DragHandleEnabled = enabled;
        }
    }

    public bool SmallHandle
    {
        get => smallHandle;
        set
        {
            if (value == smallHandle)
                return;

            smallHandle = value;

            if (backgroundDragHandleController is null)
                return;

            backgroundDragHandleController.HandleSize = smallHandle ? HandleSize.Small : HandleSize.Normal;
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
            Size = SmallHandle ? HandleSize.Small : HandleSize.Normal,
        }.Build();

        backgroundDragHandleController = new(dragHandle, backgroundTransform)
        {
            DragHandleEnabled = Enabled,
        };

        backgroundDragHandleController.DragHandleEnabled = Enabled;
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
