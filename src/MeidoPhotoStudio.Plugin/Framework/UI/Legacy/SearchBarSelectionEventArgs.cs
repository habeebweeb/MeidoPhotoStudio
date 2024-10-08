namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class SearchBarSelectionEventArgs<T>(T item) : EventArgs
{
    public T Item { get; } = item;
}
