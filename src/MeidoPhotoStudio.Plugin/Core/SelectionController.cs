namespace MeidoPhotoStudio.Plugin.Core;

// TODO: What's the difference between this and a SelectList?
public class SelectionController<T>
{
    private readonly IIndexableCollection<T> indexable;

    public SelectionController(IIndexableCollection<T> indexable) =>
        this.indexable = indexable ?? throw new ArgumentNullException(nameof(indexable));

    public event EventHandler<SelectionEventArgs<T>> Selecting;

    public event EventHandler<SelectionEventArgs<T>> Selected;

    public int CurrentIndex { get; private set; }

    public T Current =>
        indexable.Count is 0 || CurrentIndex >= indexable.Count
            ? default
            : indexable[CurrentIndex];

    public void Select(int index)
    {
        if ((uint)index >= indexable.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        Selecting?.Invoke(this, new(Current, CurrentIndex));

        CurrentIndex = index;

        Selected?.Invoke(this, new(indexable[index], index));
    }

    public void Select(T selectedObject)
    {
        if (selectedObject == null)
            throw new ArgumentNullException(nameof(selectedObject));

        var objectIndex = indexable.IndexOf(selectedObject);

        if (objectIndex is -1)
            return;

        Selecting?.Invoke(this, new(Current, CurrentIndex));

        CurrentIndex = objectIndex;

        Selected?.Invoke(this, new(selectedObject, objectIndex));
    }
}
