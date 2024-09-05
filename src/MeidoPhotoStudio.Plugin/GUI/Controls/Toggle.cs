using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

public class Toggle : BaseControl
{
    private string label;
    private Texture icon;
    private GUIContent toggleContent;
    private bool value;

    public Toggle(string label, bool state = false)
        : this(state) =>
        Label = label;

    public Toggle(Texture icon, bool state = false)
        : this(state) =>
        Icon = icon;

    private Toggle(bool state = false) =>
        value = state;

    public static LazyStyle Style { get; } = new(13, () => new(GUI.skin.toggle));

    public string Label
    {
        get => toggleContent.text;
        set
        {
            label = value;
            toggleContent = new(label);
        }
    }

    public Texture Icon
    {
        get => toggleContent.image;
        set
        {
            icon = value;
            toggleContent = new(icon);
        }
    }

    public bool Value
    {
        get => value;
        set => SetEnabled(value);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle toggleStyle, params GUILayoutOption[] layoutOptions)
    {
        var value = GUILayout.Toggle(Value, toggleContent, toggleStyle, layoutOptions);

        if (value != Value)
            Value = value;
    }

    public void SetEnabledWithoutNotify(bool enabled) =>
        SetEnabled(enabled, false);

    private void SetEnabled(bool enabled, bool notify = true)
    {
        value = enabled;

        if (notify)
            OnControlEvent(EventArgs.Empty);
    }
}
