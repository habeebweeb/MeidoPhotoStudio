using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class MaidSelectorPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private Vector2 maidListScrollPos;
        private readonly Button clearMaidsButton;
        private readonly Button callMaidsButton;
        public MaidSelectorPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            clearMaidsButton = new Button(Translation.Get("maidCallWindow", "clearButton"));
            clearMaidsButton.ControlEvent += (s, a) => this.meidoManager.ClearSelectList();

            callMaidsButton = new Button(Translation.Get("maidCallWindow", "callButton"));
            callMaidsButton.ControlEvent += (s, a) => this.meidoManager.CallMeidos();
        }

        protected override void ReloadTranslation()
        {
            clearMaidsButton.Label = Translation.Get("maidCallWindow", "clearButton");
            callMaidsButton.Label = Translation.Get("maidCallWindow", "callButton");
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();
            clearMaidsButton.Draw(GUILayout.ExpandWidth(false));
            callMaidsButton.Draw();
            GUILayout.EndHorizontal();

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14 };
            GUIStyle selectLabelStyle = new GUIStyle(labelStyle);
            selectLabelStyle.normal.textColor = Color.black;
            selectLabelStyle.alignment = TextAnchor.UpperRight;
            GUIStyle labelSelectedStyle = new GUIStyle(labelStyle);
            labelSelectedStyle.normal.textColor = Color.black;

            Rect windowRect = parent.WindowRect;
            float windowHeight = windowRect.height;
            float buttonWidth = windowRect.width - 30f;
            const float buttonHeight = 85f;

            Rect positionRect = new Rect(5f, 90f, windowRect.width - 10f, windowHeight - 125f);
            Rect viewRect = new Rect(0f, 0f, buttonWidth, (buttonHeight * meidoManager.Meidos.Length) + 5f);
            maidListScrollPos = GUI.BeginScrollView(positionRect, maidListScrollPos, viewRect);

            for (int i = 0; i < meidoManager.Meidos.Length; i++)
            {
                Meido meido = meidoManager.Meidos[i];
                float y = i * buttonHeight;
                bool selectedMaid = meidoManager.SelectedMeidoSet.Contains(i);

                if (GUI.Button(new Rect(0f, y, buttonWidth, buttonHeight), string.Empty)) meidoManager.SelectMeido(i);

                if (selectedMaid)
                {
                    int selectedIndex = meidoManager.SelectMeidoList.IndexOf(i) + 1;
                    GUI.DrawTexture(
                        new Rect(5f, y + 5f, buttonWidth - 10f, buttonHeight - 10f), Texture2D.whiteTexture
                    );
                    GUI.Label(
                        new Rect(0f, y + 5f, buttonWidth - 10f, buttonHeight),
                        selectedIndex.ToString(), selectLabelStyle
                    );
                }

                if (meido.Portrait) GUI.DrawTexture(new Rect(5f, y, buttonHeight, buttonHeight), meido.Portrait);
                GUI.Label(
                    new Rect(95f, y + 30f, buttonWidth - 80f, buttonHeight),
                    $"{meido.LastName}\n{meido.FirstName}", selectedMaid ? labelSelectedStyle : labelStyle
                );
            }
            GUI.EndScrollView();
        }
    }
}
