using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

public class Button : BaseControl
{
    private string label;
    private Texture icon;

    private GUIContent buttonContent;

    public Button(string label) =>
        Label = label;

    public Button(Texture icon) =>
        Icon = icon;

    public static LazyStyle Style { get; } = new(13, () => GUI.skin.button);

    public string Label
    {
        get => label;
        set
        {
            label = value;
            buttonContent = new(label);
        }
    }

    public Texture Icon
    {
        get => icon;
        set
        {
            icon = value;
            buttonContent = new(icon);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(Style, layoutOptions);

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
    {
        if (GUILayout.Button(buttonContent, buttonStyle, layoutOptions))
            OnControlEvent(EventArgs.Empty);
    }
}
