using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidSelectorPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private Vector2 maidListScrollPos;
        private readonly Button clearMaidsButton;
        private readonly Button callMaidsButton;
        public MaidSelectorPane(MeidoManager meidoManager) : base()
        {
            this.meidoManager = meidoManager;
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

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            GUIStyle selectLabelStyle = new GUIStyle(labelStyle);
            selectLabelStyle.normal.textColor = Color.black;
            selectLabelStyle.alignment = TextAnchor.UpperRight;
            GUIStyle labelSelectedStyle = new GUIStyle(labelStyle);
            labelSelectedStyle.normal.textColor = Color.black;

            float windowHeight = Screen.height * 0.8f;
            const int buttonHeight = 85;
            const int buttonWidth = 205;
            Rect positionRect = new Rect(5, 115, buttonWidth + 15, windowHeight - 140);
            Rect viewRect = new Rect(0, 0, buttonWidth - 5, (buttonHeight * meidoManager.Meidos.Length) + 5);
            maidListScrollPos = GUI.BeginScrollView(positionRect, maidListScrollPos, viewRect);

            for (int i = 0; i < meidoManager.Meidos.Length; i++)
            {
                Meido meido = meidoManager.Meidos[i];
                float y = i * buttonHeight;
                bool selectedMaid = meidoManager.SelectMeidoList.Contains(i);

                if (GUI.Button(new Rect(0, y, buttonWidth, buttonHeight), ""))
                {
                    if (selectedMaid) meidoManager.SelectMeidoList.Remove(i);
                    else meidoManager.SelectMeidoList.Add(i);
                }

                if (selectedMaid)
                {
                    int selectedIndex = meidoManager.SelectMeidoList.IndexOf(i) + 1;
                    GUI.DrawTexture(new Rect(5, y + 5, buttonWidth - 10, buttonHeight - 10), Texture2D.whiteTexture);
                    GUI.Label(
                        new Rect(0, y + 5, buttonWidth - 10, buttonHeight), selectedIndex.ToString(), selectLabelStyle
                    );
                }

                GUI.DrawTexture(new Rect(5, y, buttonHeight, buttonHeight), meido.Portrait);
                GUI.Label(
                    new Rect(95, y + 30, buttonWidth - 80, buttonHeight),
                    $"{meido.LastName}\n{meido.FirstName}", selectedMaid ? labelSelectedStyle : labelStyle
                );
            }
            GUI.EndScrollView();
        }
    }
}
