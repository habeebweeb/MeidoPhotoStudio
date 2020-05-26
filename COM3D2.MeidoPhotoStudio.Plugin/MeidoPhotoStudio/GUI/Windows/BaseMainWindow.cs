using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BaseMainWindow : BaseWindow
    {
        public BaseMainWindow() : base() { }
        public override void OnGUI(int id)
        {
            TabsPane.Draw();

            Draw();

            GUILayout.FlexibleSpace();
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 10;
            labelStyle.alignment = TextAnchor.LowerLeft;

            GUILayout.Label("MeidoPhotoStudio 1.0.0", labelStyle);
            GUI.DragWindow();
        }
    }
}
