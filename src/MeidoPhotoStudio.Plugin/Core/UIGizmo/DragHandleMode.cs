namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class DragHandleMode
{
    public abstract void OnModeEnter();

    public virtual void OnModeExit()
    {
    }

    public virtual void OnClicked()
    {
    }

    public virtual void OnDragging()
    {
    }

    public virtual void OnReleased()
    {
    }

    public virtual void OnDoubleClicked()
    {
    }
}
