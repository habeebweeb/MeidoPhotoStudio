namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public abstract class BasePane
{
    protected BaseWindow parent;

    protected BasePane() =>
        Translation.ReloadTranslationEvent += OnReloadTranslation;

    // TODO: This does not work how I think it works. Probably just remove entirely.
    ~BasePane() =>
        Translation.ReloadTranslationEvent -= OnReloadTranslation;

    public virtual bool Visible { get; set; }

    public virtual bool Enabled { get; set; }

    public virtual void SetParent(BaseWindow window) =>
        parent = window;

    public virtual void UpdatePane()
    {
    }

    public virtual void Draw()
    {
    }

    public virtual void Activate()
    {
    }

    public virtual void Deactivate()
    {
    }

    public virtual void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
    }

    protected virtual void ReloadTranslation()
    {
    }

    protected void DrawDropdown<T>(Dropdown<T> dropdown)
    {
        GUILayout.BeginHorizontal();

        var buttonAndScrollbarSize = 33 * 2 + 15;
        var dropdownButtonWidth = parent.WindowRect.width - buttonAndScrollbarSize;

        dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

        var arrowLayoutOptions = GUILayout.MaxWidth(20);

        if (GUILayout.Button("<", arrowLayoutOptions))
            dropdown.CyclePrevious();

        if (GUILayout.Button(">", arrowLayoutOptions))
            dropdown.CycleNext();

        GUILayout.EndHorizontal();
    }

    protected void DrawComboBox(ComboBox comboBox)
    {
        var textFieldWidth = parent.WindowRect.width - 56;

        comboBox.Draw(GUILayout.Width(textFieldWidth));
    }

    protected void DrawTextFieldMaxWidth(TextField textField)
    {
        var maxWidth = parent.WindowRect.width - 25;

        textField.Draw(GUILayout.MaxWidth(maxWidth));
    }

    private void OnReloadTranslation(object sender, EventArgs args) =>
        ReloadTranslation();
}
