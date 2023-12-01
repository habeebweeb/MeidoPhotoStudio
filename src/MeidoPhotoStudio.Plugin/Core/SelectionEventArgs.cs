namespace MeidoPhotoStudio.Plugin.Core;

public class SelectionEventArgs<T> : System.EventArgs
{
    public SelectionEventArgs(T selected, int index)
    {
        Selected = selected;
        Index = index;
    }

    public T Selected { get; }

    public int Index { get; }
}
