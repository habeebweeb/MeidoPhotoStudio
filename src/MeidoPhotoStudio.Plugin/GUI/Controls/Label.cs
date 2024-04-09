using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class Label(string text) : BaseControl
{
    private string text = text;
    private GUIContent content = new(text);

    public string Text
    {
        get => text;
        set
        {
            text = value ?? string.Empty;
            content = new GUIContent(value);
        }
    }

    public override void Draw(params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content);

    public virtual void Draw(GUIStyle labelStyle, params GUILayoutOption[] layoutOptions) =>
        GUILayout.Label(content, labelStyle, layoutOptions);
}
