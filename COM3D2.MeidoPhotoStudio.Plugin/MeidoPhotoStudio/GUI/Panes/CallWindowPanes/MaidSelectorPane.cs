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
            clearMaidsButton = new Button(Translation.Get("maidCallWindow", "clearButton"));
            clearMaidsButton.ControlEvent += (s, a) => this.meidoManager.SelectMeidoList.Clear();
            Controls.Add(clearMaidsButton);

            callMaidsButton = new Button(Translation.Get("maidCallWindow", "callButton"));
            callMaidsButton.ControlEvent += (s, a) => this.meidoManager.CallMeidos();
            Controls.Add(callMaidsButton);
        }

        protected override void ReloadTranslation()
        {
            clearMaidsButton.Label = Translation.Get("maidCallWindow", "clearButton");
            callMaidsButton.Label = Translation.Get("maidCallWindow", "callButton");
        }

        public override void Draw()
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
                bool selectedMaid = this.meidoManager.SelectMeidoList.Contains(i);

                if (GUI.Button(new Rect(0, y, buttonWidth, buttonHeight), ""))
                {
                    if (selectedMaid) this.meidoManager.SelectMeidoList.Remove(i);
                    else this.meidoManager.SelectMeidoList.Add(i);
                }

                if (selectedMaid)
                {
                    int selectedIndex = this.meidoManager.SelectMeidoList.IndexOf(i) + 1;
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
