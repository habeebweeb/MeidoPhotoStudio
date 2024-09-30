namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class Dropdown<T> : DropdownBase<T>
{
    private static readonly LazyStyle ButtonStyle = new(
        13,
        () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        });

    private GUIContent label = GUIContent.none;

    private bool clickedWhileOpen;
    private bool buttonClicked;

    public Dropdown(Func<T, int, IDropdownItem> formatter = null) =>
        Formatter = formatter;

    public Dropdown(IEnumerable<T> items, int selectedItemIndex = 0, Func<T, int, IDropdownItem> formatter = null)
        : this(formatter)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        SetItems(items, selectedItemIndex, notify: false);
    }

    public event EventHandler DropdownOpening;

    public event EventHandler<DropdownEventArgs<T>> SelectionChanged;

    public event EventHandler DropdownClosed;

    public override Func<T, int, IDropdownItem> Formatter
    {
        get => base.Formatter;
        set
        {
            base.Formatter = value;

            label = Count is 0 ? GUIContent.none : FormattedItem(SelectedItemIndex);
        }
    }

    public override int SelectedItemIndex
    {
        get => base.SelectedItemIndex;
        set => SetSelectedItemIndex(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(ButtonStyle, layoutOptions);

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        var clicked = GUILayout.Button(label, buttonStyle, layoutOptions);

        if (clicked && Count > 0)
        {
            buttonClicked = !clickedWhileOpen;
            clickedWhileOpen = false;
        }

        if (buttonClicked && Event.current.type is EventType.Repaint)
        {
            buttonClicked = false;

            DropdownOpening?.Invoke(this, EventArgs.Empty);

            DropdownHelper.OpenDropdown(this, GUILayoutUtility.GetLastRect());
        }
    }

    public void SetItemsWithoutNotify(IEnumerable<T> items, int? newIndex = null) =>
        SetItems(items, newIndex, false);

    public void SetSelectedIndexWithoutNotify(int index) =>
        SetSelectedItemIndex(index, false);

    public void CycleNext() =>
        SelectedItemIndex = Wrap(SelectedItemIndex + 1, 0, Count);

    public void CyclePrevious() =>
        SelectedItemIndex = Wrap(SelectedItemIndex - 1, 0, Count);

    public override void SetItems(IEnumerable<T> items, int? newIndex = null) =>
        SetItems(items, newIndex, true);

    protected override void OnDropdownClosed(bool clickedButton)
    {
        clickedWhileOpen = clickedButton;

        if (clickedWhileOpen)
            SelectionChanged?.Invoke(this, new(SelectedItem, SelectedItemIndex, SelectedItemIndex));

        DropdownClosed?.Invoke(this, EventArgs.Empty);
    }

    private static int Wrap(int value, int min, int max) =>
        value < min ? max :
        value >= max ? min :
        value;

    private void SetItems(IEnumerable<T> items, int? newIndex = null, bool notify = true)
    {
        var previousSelectedItemIndex = base.SelectedItemIndex;

        base.SetItems(items, newIndex);

        label = Count is 0 ? GUIContent.none : FormattedItem(SelectedItemIndex);

        if (!notify)
            return;

        SelectionChanged?.Invoke(this, new(SelectedItem, SelectedItemIndex, previousSelectedItemIndex));
    }

    private void SetSelectedItemIndex(int index, bool notify = true)
    {
        var previousSelectedItemIndex = base.SelectedItemIndex;

        base.SelectedItemIndex = index;

        label = Count is 0 ? GUIContent.none : FormattedItem(SelectedItemIndex);

        if (!notify)
            return;

        SelectionChanged?.Invoke(this, new(SelectedItem, SelectedItemIndex, previousSelectedItemIndex));
    }
}
