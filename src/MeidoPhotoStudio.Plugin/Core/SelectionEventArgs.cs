namespace MeidoPhotoStudio.Plugin.Core;

public class SelectionEventArgs<T>(T selected, int index) : EventArgs
{
    public T Selected { get; } = selected;

    public int Index { get; } = index;
}
