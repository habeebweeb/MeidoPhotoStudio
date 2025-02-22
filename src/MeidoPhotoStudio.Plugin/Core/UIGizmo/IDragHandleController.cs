namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public interface IDragHandleController
{
    DragHandleMode CurrentMode { get; set; }

    bool Destroyed { get; }

    bool DragHandleDragging { get; }

    bool GizmoDragging { get; }

    void Destroy();
}
