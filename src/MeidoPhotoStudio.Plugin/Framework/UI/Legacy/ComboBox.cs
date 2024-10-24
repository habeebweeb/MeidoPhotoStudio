using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class ComboBox : DropdownBase<string>
{
    private static readonly Func<string, int, IDropdownItem> DefaultFormatter = (string item, int index) =>
        new LabelledDropdownItem(string.IsNullOrEmpty(item) ? string.Empty : item);

    private static readonly LazyStyle ButtonStyle = new(13, () => new(GUI.skin.button));

    private readonly SearchBar<string> searchBar;

    private bool clickedWhileOpen;
    private bool buttonClicked;

    public ComboBox(IEnumerable<string> items, Func<string, int, IDropdownItem> formatter = null)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        base.Formatter = formatter ?? DefaultFormatter;

        searchBar = new(SearchSelector, formatter ?? DefaultFormatter);
        searchBar.SelectedValue += OnValueSelected;

        SetItems(items);

        IEnumerable<string> SearchSelector(string query) =>
            this.Where(item => item.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public override Func<string, int, IDropdownItem> Formatter
    {
        get => base.Formatter;
        set
        {
            base.Formatter = value;
            searchBar.Formatter = value;
        }
    }

    public string Value
    {
        get => searchBar.Query;
        set => searchBar.Query = value;
    }

    public string Placeholder
    {
        get => searchBar.Placeholder;
        set => searchBar.Placeholder = value;
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(TextField.Style, ButtonStyle, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal();

        searchBar.Draw(textFieldStyle, layoutOptions);

        var clicked = GUILayout.Button("v", buttonStyle, GUILayout.MaxWidth(20));

        if (clicked)
        {
            buttonClicked = !clickedWhileOpen;
            clickedWhileOpen = false;
        }

        if (buttonClicked && Event.current.type is EventType.Repaint)
        {
            buttonClicked = false;

            DropdownHelper.OpenDropdown(this, GUILayoutUtility.GetLastRect());
        }

        GUILayout.EndHorizontal();
    }

    protected override void OnItemSelected(int index)
    {
        base.OnItemSelected(index);

        searchBar.SetQueryWithoutShowingResults(this[SelectedItemIndex]);
    }

    protected override void OnDropdownClosed(bool clickedButton)
    {
        clickedWhileOpen = clickedButton;

        if (!clickedButton)
            return;

        searchBar.SetQueryWithoutShowingResults(this[SelectedItemIndex]);
        searchBar.Query = this[SelectedItemIndex];
    }

    private void OnValueSelected(object sender, SearchBarSelectionEventArgs<string> e) =>
        searchBar.SetQueryWithoutShowingResults(e.Item);
}
