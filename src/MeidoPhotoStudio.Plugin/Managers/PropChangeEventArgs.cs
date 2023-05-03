namespace MeidoPhotoStudio.Plugin;

public class PropChangeEventArgs : System.EventArgs
{
    public PropChangeEventArgs(DragPointProp prop) =>
        Prop = prop;

    public DragPointProp Prop { get; }
}
