using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidSwitcherPane : BasePane
    {
        private MeidoManager meidoManager;
        private Button PreviousButton;
        private Button NextButton;
        private int SelectedMeido => meidoManager.SelectedMeido;
        public MaidSwitcherPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            PreviousButton = new Button("<");
            PreviousButton.ControlEvent += (s, a) => ChangeMaid(-1);

            NextButton = new Button(">");
            NextButton.ControlEvent += (s, a) => ChangeMaid(1);
        }

        public override void Draw()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            boxStyle.padding.top = -15;
            buttonStyle.margin.top = 20;
            labelStyle.alignment = TextAnchor.UpperLeft;

            GUILayout.BeginHorizontal();

            GUI.enabled = meidoManager.HasActiveMeido;

            PreviousButton.Draw(buttonStyle, GUILayout.Height(40), GUILayout.ExpandWidth(false));

            if (meidoManager.HasActiveMeido)
                MiscGUI.DrawTexture(meidoManager.ActiveMeido.Image, GUILayout.Width(70), GUILayout.Height(70));
            else
                GUILayout.Box("", boxStyle, GUILayout.Height(70), GUILayout.Width(70));

            GUILayout.BeginVertical();
            GUILayout.Space(30);
            GUILayout.Label(
                meidoManager.HasActiveMeido
                    ? meidoManager.ActiveMeido.NameJP
                    : "",
                labelStyle, GUILayout.ExpandWidth(false)
            );
            GUILayout.EndVertical();

            NextButton.Draw(buttonStyle, GUILayout.Height(40), GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        private void ChangeMaid(int dir)
        {
            dir = (int)Mathf.Sign(dir);
            int selected = Utility.Wrap(
                this.meidoManager.SelectedMeido + dir, 0, this.meidoManager.ActiveMeidoList.Count
            );
            this.meidoManager.ChangeMaid(selected);
        }
    }
}
