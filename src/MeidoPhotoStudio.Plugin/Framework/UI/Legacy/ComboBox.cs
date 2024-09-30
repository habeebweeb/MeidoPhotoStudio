namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class ComboBox : DropdownBase<string>
{
    private static readonly Func<string, int, IDropdownItem> DefaultFormatter = (string item, int index) =>
        new LabelledDropdownItem(string.IsNullOrEmpty(item) ? string.Empty : item);

    private static readonly LazyStyle ButtonStyle = new(13, () => new(GUI.skin.button));

    private readonly TextField textField;

    private bool clickedWhileOpen;
    private bool buttonClicked;

    public ComboBox(IEnumerable<string> items, string value = null, Func<string, int, IDropdownItem> formatter = null)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        textField = new()
        {
            Value = value ?? string.Empty,
        };

        Formatter = formatter ?? DefaultFormatter;

        SetItems(items);
    }

    public string Value =>
        textField.Value;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(TextField.Style, ButtonStyle, layoutOptions);

    public void Draw(GUIStyle textFieldStyle, GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal();

        textField.Draw(textFieldStyle, layoutOptions);

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

        textField.Value = this[SelectedItemIndex];
    }

    protected override void OnDropdownClosed(bool clickedButton)
    {
        clickedWhileOpen = clickedButton;

        if (!clickedButton)
            return;

        textField.Value = this[SelectedItemIndex];
    }
}
