namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public interface IDragHandleController
{
    DragHandleMode CurrentMode { get; set; }

    void Destroy();
}
