namespace MeidoPhotoStudio.Plugin.Core;

public interface IIndexableCollection<T>
{
    int Count { get; }

    T this[int index] { get; }

    int IndexOf(T selectable);
}
