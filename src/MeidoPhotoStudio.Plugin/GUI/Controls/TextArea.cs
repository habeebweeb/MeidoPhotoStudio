namespace MeidoPhotoStudio.Plugin;

public class TextArea : BaseControl
{
    public string Value { get; set; } = string.Empty;

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        Draw(new(GUI.skin.textArea), layoutOptions);

    public void Draw(GUIStyle textAreaStyle, params GUILayoutOption[] layoutOptions) =>
        Value = GUILayout.TextArea(Value, textAreaStyle, layoutOptions);
}
