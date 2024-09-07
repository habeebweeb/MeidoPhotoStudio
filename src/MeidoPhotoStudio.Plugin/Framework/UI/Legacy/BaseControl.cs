namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BaseControl
{
    public event EventHandler ControlEvent;

    public virtual void Draw(params GUILayoutOption[] layoutOptions)
    {
    }

    public virtual void OnControlEvent(EventArgs args) =>
        ControlEvent?.Invoke(this, args);
}