using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidSwitcherPane : BasePane
    {
        public static MeidoManager meidoManager;
        private static Button PreviousButton;
        private static Button NextButton;
        public static event EventHandler<MeidoChangeEventArgs> MaidChange;
        private static int SelectedMeido => meidoManager.SelectedMeido;
        static MaidSwitcherPane()
        {
            PreviousButton = new Button("<");
            PreviousButton.ControlEvent += (s, a) => ChangeMaid(-1);

            NextButton = new Button(">");
            NextButton.ControlEvent += (s, a) => ChangeMaid(1);
        }

        public static void Draw()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            boxStyle.padding.top = -15;
            buttonStyle.margin.top = 20;
            labelStyle.alignment = TextAnchor.UpperLeft;

            GUILayout.BeginHorizontal();

            bool previousState = GUI.enabled;
            GUI.enabled = meidoManager.HasActiveMeido;

            PreviousButton.Draw(buttonStyle, GUILayout.Height(40), GUILayout.ExpandWidth(false));

            if (meidoManager.HasActiveMeido)
                MiscGUI.DrawTexture(meidoManager.ActiveMeido.Image, GUILayout.Width(70), GUILayout.Height(70));
            else
                GUILayout.Box("", boxStyle, GUILayout.Height(70), GUILayout.Width(70));


            GUILayout.BeginVertical();
            GUILayout.Space(30);
            GUILayout.Label(meidoManager.HasActiveMeido ? meidoManager.ActiveMeido.NameJP : "", labelStyle, GUILayout.ExpandWidth(false));
            GUILayout.EndVertical();

            NextButton.Draw(buttonStyle, GUILayout.Height(40), GUILayout.ExpandWidth(false));

            GUI.enabled = previousState;

            GUILayout.EndHorizontal();
        }

        private static void ChangeMaid(int dir)
        {
            dir = (int)Mathf.Sign(dir);
            int selected = Utility.Wrap(SelectedMeido + dir, 0, meidoManager.ActiveMeidoList.Count);
            OnMaidChange(new MeidoChangeEventArgs(selected));
        }

        private static void OnMaidChange(MeidoChangeEventArgs args)
        {
            MaidChange?.Invoke(null, args);
        }
    }
}
