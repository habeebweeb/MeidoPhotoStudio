namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class SearchBar<T> : DropdownBase<T>
{
    private static readonly Func<T, int, IDropdownItem> DefaultFormatter = (item, _) =>
        new LabelledDropdownItem(item?.ToString() ?? string.Empty);

    private readonly TextField textField;
    private readonly Func<string, IEnumerable<T>> searchSelector;

    private string searchQuery;
    private bool clickedWhileOpen;
    private bool openDropdown;

    public SearchBar(Func<string, IEnumerable<T>> searchSelector, Func<T, int, IDropdownItem> formatter = null)
    {
        this.searchSelector = searchSelector ?? throw new ArgumentNullException(nameof(searchSelector));

        Formatter = formatter ?? DefaultFormatter;

        textField = new() { HasClearButton = true };
        textField.GainedFocus += OnFocusGained;
        textField.ChangedValue += OnSearchQueryChanged;
    }

    public event EventHandler<SearchBarSelectionEventArgs<T>> SelectedValue;

    public string Placeholder
    {
        get => textField.Placeholder;
        set => textField.Placeholder = value;
    }

    public string Query
    {
        get => textField.Value;
        set
        {
            var newValue = value ?? string.Empty;

            textField.Value = newValue;
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(TextField.Style, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, params GUILayoutOption[] layoutOptions)
    {
        textField.Draw(textFieldStyle, layoutOptions);

        if ((openDropdown || clickedWhileOpen) && Event.current.type is EventType.Repaint)
        {
            openDropdown = false;
            clickedWhileOpen = false;

            if (Count is 0)
                return;

            DropdownHelper.OpenDropdown(this, GUILayoutUtility.GetLastRect());
        }
    }

    public void ClearQuery()
    {
        textField.Value = string.Empty;

        DropdownHelper.CloseDropdown();
    }

    protected override void OnItemSelected(int index)
    {
        base.OnItemSelected(index);

        GUI.FocusControl(null);

        SelectedValue?.Invoke(this, new(SelectedItem));
    }

    protected override void OnDropdownClosed(bool clickedButton) =>
        clickedWhileOpen = clickedButton;

    private void OnSearchQueryChanged(object sender, EventArgs e) =>
        Search(textField.Value);

    private void Search(string query)
    {
        if (string.Equals(query, searchQuery, StringComparison.OrdinalIgnoreCase))
            return;

        searchQuery = query;

        if (string.IsNullOrEmpty(searchQuery))
        {
            Clear();

            DropdownHelper.CloseDropdown();

            return;
        }

        SetItems(searchSelector(searchQuery));

        if (Count is 0)
        {
            DropdownHelper.CloseDropdown();

            return;
        }

        SelectedItemIndex = 0;

        openDropdown = true;
    }

    private void OnFocusGained(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(textField.Value) || Count is 0)
            return;

        openDropdown = true;
    }
}
