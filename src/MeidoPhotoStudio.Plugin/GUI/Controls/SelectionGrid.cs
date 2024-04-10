namespace MeidoPhotoStudio.Plugin;

public class SelectionGrid : BaseControl
{
    private SimpleToggle[] toggles;
    private int selectedItemIndex;

    public SelectionGrid(string[] items, int selected = 0)
    {
        selectedItemIndex = Mathf.Clamp(selected, 0, items.Length - 1);
        toggles = MakeToggles(items);
    }

    public int SelectedItemIndex
    {
        get => selectedItemIndex;
        set => SetValue(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        GUILayout.BeginHorizontal();

        foreach (var toggle in toggles)
            toggle.Draw(layoutOptions);

        GUILayout.EndHorizontal();
    }

    public void SetItems(string[] items, int selectedItemIndex = -1) =>
        SetItems(items, selectedItemIndex, true);

    public void SetItemsWithoutNotify(string[] items, int selectedItemIndex = -1) =>
        SetItems(items, selectedItemIndex, false);

    public void SetValueWithoutNotify(int value) =>
        SetValue(value, false);

    private void SetItems(string[] items, int selectedItemIndex, bool notify)
    {
        if (selectedItemIndex < 0)
            selectedItemIndex = SelectedItemIndex;

        if (items.Length != toggles.Length)
            toggles = MakeToggles(items);
        else
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];

                toggles[i].Value = i == SelectedItemIndex;
                toggles[i].Label = item;
            }

        SetValue(selectedItemIndex, notify);
    }

    private void SetValue(int value, bool notify = true)
    {
        selectedItemIndex = Mathf.Clamp(value, 0, toggles.Length - 1);

        foreach (var toggle in toggles)
            toggle.Value = toggle.ToggleIndex == selectedItemIndex;

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }

    private SimpleToggle[] MakeToggles(string[] items)
    {
        var toggles = new SimpleToggle[items.Length];

        for (var i = 0; i < items.Length; i++)
        {
            var toggle = new SimpleToggle(items[i], i == SelectedItemIndex)
            {
                ToggleIndex = i,
            };

            toggle.ControlEvent += (sender, _) =>
            {
                var value = (sender as SimpleToggle).ToggleIndex;

                if (value != SelectedItemIndex)
                    SelectedItemIndex = value;
            };

            toggles[i] = toggle;
        }

        return toggles;
    }

    private class SimpleToggle(string label, bool value = false)
    {
        public int ToggleIndex;
        public bool Value = value;
        public string Label = label;

        public event EventHandler ControlEvent;

        public void Draw(params GUILayoutOption[] layoutOptions)
        {
            var value = GUILayout.Toggle(Value, Label, layoutOptions);

            if (value == Value)
                return;

            if (!value)
            {
                Value = true;
            }
            else
            {
                Value = value;

                ControlEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
