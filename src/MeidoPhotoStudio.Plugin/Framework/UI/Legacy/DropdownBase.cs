namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class DropdownBase<T> : BaseControl, IEnumerable<T>, IDropdownHandler
{
    private static readonly Func<T, int, IDropdownItem> DefaultItemFormatter = (T item, int index) =>
        new LabelledDropdownItem(item?.ToString() ?? string.Empty);

    private T[] items = [];
    private Dictionary<int, Vector2> itemSizeCache = [];
    private Dictionary<int, IDropdownItem> dropdownItems = [];
    private int selectedItemIndex;
    private Func<T, int, IDropdownItem> itemFormatter = DefaultItemFormatter;
    private Vector2 scrollPosition;

    public T SelectedItem =>
        items.Length is 0
            ? default
            : items[SelectedItemIndex];

    public virtual int SelectedItemIndex
    {
        get => selectedItemIndex;
        set => selectedItemIndex = Mathf.Clamp(value, 0, items.Length - 1);
    }

    public virtual Func<T, int, IDropdownItem> Formatter
    {
        get => itemFormatter;
        set
        {
            itemFormatter = value ?? DefaultItemFormatter;

            Reformat();
        }
    }

    Vector2 IDropdownHandler.ScrollPosition
    {
        get => scrollPosition;
        set => scrollPosition = value;
    }

    public int Count =>
        items.Length;

    public T this[int index] =>
        (uint)index >= items.Length
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : items[index];

    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable<T>)items).GetEnumerator();

    public virtual void SetItems(IEnumerable<T> items, int? newIndex = null)
    {
        this.items = [.. items ?? throw new ArgumentNullException(nameof(items))];

        selectedItemIndex = Mathf.Clamp(newIndex ?? selectedItemIndex, 0, this.items.Length - 1);

        Reformat();

        scrollPosition = Vector2.zero;
    }

    public void Reformat()
    {
        itemSizeCache = [];

        foreach (var item in dropdownItems.Values)
            item.Dispose();

        dropdownItems = [];
    }

    public void Clear() =>
        SetItems([], 0);

    public int IndexOf(T item) =>
        Array.IndexOf(items, item);

    public int FindIndex(Func<T, bool> predicate)
    {
        _ = predicate ?? throw new ArgumentNullException(nameof(predicate));

        return Array.FindIndex(items, new(predicate));
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    GUIContent IDropdownHandler.FormattedItem(int index) =>
        FormattedItem(index);

    Vector2 IVirtualListHandler.ItemDimensions(int index)
    {
        if (itemSizeCache.TryGetValue(index, out var size))
            return size;

        var dropdownItem = GetDropdownItem(index);

        size = DropdownHelper.CalculateItemDimensions(new(dropdownItem.Label));

        if (dropdownItem.HasIcon)
        {
            var height = size.y;
            var additionalHeight = 0f;

            var iconSize = Mathf.Min(dropdownItem.IconSize, 90f);

            if (iconSize > height)
                additionalHeight = iconSize - height;

            size = size with
            {
                x = size.x + iconSize,
                y = size.y + additionalHeight,
            };
        }

        itemSizeCache[index] = size;

        return size;
    }

    void IDropdownHandler.OnItemSelected(int index) =>
        OnItemSelected(index);

    void IDropdownHandler.OnDropdownClosed(bool clickedButton) =>
        OnDropdownClosed(clickedButton);

    protected IDropdownItem GetDropdownItem(int index) =>
        dropdownItems.TryGetValue(index, out var item)
            ? item
            : dropdownItems[index] = Formatter(items[index], index);

    protected GUIContent FormattedItem(int index) =>
        GetDropdownItem(index).Formatted;

    protected virtual void OnItemSelected(int index) =>
        SelectedItemIndex = index;

    protected virtual void OnDropdownClosed(bool clickedButton)
    {
    }
}
