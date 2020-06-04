using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidSelectorPane : BasePane
    {
        private MeidoManager meidoManager;
        public List<int> selectedMaidList { get; private set; }
        private Vector2 maidListScrollPos;
        private Button clearMaidsButton;
        private Button callMaidsButton;
        public MaidSelectorPane(MeidoManager meidoManager) : base()
        {
            this.meidoManager = meidoManager;
            selectedMaidList = new List<int>();
            clearMaidsButton = new Button("Clear");
            clearMaidsButton.ControlEvent += (s, a) => selectedMaidList.Clear();
            Controls.Add(clearMaidsButton);

            callMaidsButton = new Button("Call");
            callMaidsButton.ControlEvent += (s, a) => this.meidoManager.OnBeginCallMeidos(this.selectedMaidList);
            Controls.Add(callMaidsButton);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            clearMaidsButton.Draw();
            callMaidsButton.Draw();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            GUIStyle selectLabelStyle = new GUIStyle(labelStyle);
            selectLabelStyle.normal.textColor = Color.black;
            selectLabelStyle.alignment = TextAnchor.UpperRight;
            GUIStyle labelSelectedStyle = new GUIStyle(labelStyle);
            labelSelectedStyle.normal.textColor = Color.black;

            float windowHeight = Screen.height * 0.8f;
            int buttonHeight = 85;
            int buttonWidth = 205;
            Rect positionRect = new Rect(5, 115, buttonWidth + 15, windowHeight - 140);
            Rect viewRect = new Rect(0, 0, buttonWidth - 5, buttonHeight * meidoManager.meidos.Length + 5);
            maidListScrollPos = GUI.BeginScrollView(positionRect, maidListScrollPos, viewRect);

            for (int i = 0; i < meidoManager.meidos.Length; i++)
            {
                Meido meido = meidoManager.meidos[i];
                float y = i * buttonHeight;
                bool selectedMaid = selectedMaidList.Contains(i);

                if (GUI.Button(new Rect(0, y, buttonWidth, buttonHeight), ""))
                {
                    if (selectedMaid) selectedMaidList.Remove(i);
                    else selectedMaidList.Add(i);
                }

                if (selectedMaid)
                {
                    int selectedIndex = selectedMaidList.IndexOf(i) + 1;
                    GUI.DrawTexture(new Rect(5, y + 5, buttonWidth - 10, buttonHeight - 10), Texture2D.whiteTexture);
                    GUI.Label(
                        new Rect(0, y + 5, buttonWidth - 10, buttonHeight), selectedIndex.ToString(), selectLabelStyle
                    );
                }

                GUI.DrawTexture(new Rect(5, y, buttonHeight, buttonHeight), meido.Image);
                GUI.Label(
                    new Rect(95, y + 30, buttonWidth - 80, buttonHeight),
                    meido.NameJP, selectedMaid ? labelSelectedStyle : labelStyle
                );

            }
            GUI.EndScrollView();
        }
    }
}
