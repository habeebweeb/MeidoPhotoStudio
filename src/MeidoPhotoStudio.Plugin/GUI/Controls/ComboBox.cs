namespace MeidoPhotoStudio.Plugin;

public class ComboBox : BaseControl, IEnumerable<string>
{
    private readonly TextField textField;
    private readonly int id = DropdownHelper.DropdownID;

    private string[] items;
    private int selectedItemIndex;
    private Vector2? itemSize = null;
    private Vector2 scrollPosition;
    private bool clickedWhileOpen;
    private bool buttonClicked;

    public ComboBox(IEnumerable<string> items, string value = null)
    {
        this.items = [.. items ?? throw new ArgumentNullException(nameof(items))];

        textField = new()
        {
            Value = value ?? string.Empty,
        };
    }

    public string Value =>
        textField.Value;

    public IEnumerator<string> GetEnumerator() =>
        ((IEnumerable<string>)items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void SetItems(IEnumerable<string> items)
    {
        this.items = [.. items ?? throw new ArgumentNullException(nameof(items))];
        selectedItemIndex = 0;
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal();
        textField.Draw(new(GUI.skin.textField), layoutOptions);

        var clicked = GUILayout.Button("▾", GUILayout.ExpandWidth(false));

        if (clicked)
        {
            buttonClicked = !clickedWhileOpen;
            clickedWhileOpen = false;
        }

        if (buttonClicked && Event.current.type is EventType.Repaint)
            OpenDropdown(GUILayoutUtility.GetLastRect());

        GUILayout.EndHorizontal();

        void OpenDropdown(Rect buttonRect)
        {
            buttonClicked = false;

            var rectPos = GUIUtility.GUIToScreenPoint(new(buttonRect.x, buttonRect.y));

            buttonRect.x = rectPos.x;
            buttonRect.y = rectPos.y;

            itemSize ??= DropdownHelper.CalculateElementSize(items);

            DropdownHelper.SelectionChange += OnSelectionChanged;
            DropdownHelper.DropdownClose += OnDropdownClosed;

            DropdownHelper.OpenDropdown(
                id,
                scrollPosition,
                items,
                selectedItemIndex,
                buttonRect,
                itemSize);

            void OnSelectionChanged(object sender, DropdownHelper.DropdownSelectArgs e)
            {
                if (e.DropdownID != id)
                    return;

                DropdownHelper.SelectionChange -= OnSelectionChanged;

                selectedItemIndex = e.SelectedItemIndex;
                textField.Value = items[selectedItemIndex];
            }

            void OnDropdownClosed(object sender, DropdownHelper.DropdownCloseArgs e)
            {
                if (e.DropdownID != id)
                    return;

                DropdownHelper.DropdownClose -= OnDropdownClosed;

                scrollPosition = e.ScrollPos;
                clickedWhileOpen = e.ClickedYou;

                if (!clickedWhileOpen)
                    return;

                textField.Value = items[selectedItemIndex];
            }
        }
    }
}
