using System;

namespace MeidoPhotoStudio.Plugin;

public class DropdownEventArgs<T>(T item, int selectedItemIndex, int previousSelectedItemIndex) : EventArgs
{
    public T Item { get; } = item;

    public int SelectedItemIndex { get; } = selectedItemIndex;

    public int PreviousSelectedItemIndex { get; } = previousSelectedItemIndex;
}
