using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class TextArea : BaseControl
    {
        public string Value { get; set; } = string.Empty;
        public void Draw(GUIStyle textAreaStyle, params GUILayoutOption[] layoutOptions)
        {
            Value = GUILayout.TextArea(Value, textAreaStyle, layoutOptions);
        }
        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            Draw(new GUIStyle(GUI.skin.textArea), layoutOptions);
        }
    }
}
