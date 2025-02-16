namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BaseWindow
{
    private static int id = 765;

    private Rect rect;
    private bool enabled = true;

    public BaseWindow() =>
        ID = id++;

    public int ID { get; }

    public virtual Rect WindowRect
    {
        get => rect;
        set => rect = value with
        {
            x = Mathf.Clamp(value.x, -value.width + UIUtility.Scaled(20), Screen.width - UIUtility.Scaled(20)),
            y = Mathf.Clamp(value.y, -value.height + UIUtility.Scaled(20), Screen.height - UIUtility.Scaled(20)),
        };
    }

    public virtual bool Visible { get; set; }

    public virtual bool Enabled
    {
        get => Modal.Visible
            ? enabled && Modal.ID == ID
            : enabled;

        set =>
            enabled = value;
    }

    public virtual void GUIFunc(int id)
    {
        GUI.enabled = Enabled;

        Draw();

        GUI.enabled = true;

        GUI.DragWindow();
    }

    public abstract void Draw();

    public virtual void Activate()
    {
    }

    public virtual void Deactivate()
    {
    }

    public virtual void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
    }
}
