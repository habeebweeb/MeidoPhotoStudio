namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class WrapperDragHandleMode<T>(T originalMode) : DragHandleMode
    where T : DragHandleMode
{
    public override void OnModeEnter() =>
        originalMode.OnModeEnter();

    public override void OnModeExit() =>
        originalMode.OnModeExit();

    public override void OnClicked() =>
        originalMode.OnClicked();

    public override void OnDragging() =>
        originalMode.OnDragging();

    public override void OnReleased() =>
        originalMode.OnReleased();

    public override void OnCancelled() =>
        originalMode.OnCancelled();

    public override void OnDoubleClicked() =>
        originalMode.OnDoubleClicked();

    public override void OnGizmoClicked() =>
        originalMode.OnGizmoClicked();

    public override void OnGizmoDragging() =>
        originalMode.OnGizmoDragging();

    public override void OnGizmoReleased() =>
        originalMode.OnGizmoReleased();

    public override void OnGizmoCancelled() =>
        originalMode.OnGizmoCancelled();
}
