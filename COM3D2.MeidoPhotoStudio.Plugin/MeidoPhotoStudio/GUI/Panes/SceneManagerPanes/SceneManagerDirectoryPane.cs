using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SceneManagerDirectoryPane : BasePane
    {
        public static readonly int listWidth = 200;
        private readonly SceneManager sceneManager;
        private readonly SceneModalWindow sceneModalWindow;
        private readonly Button createDirectoryButton;
        private readonly Button deleteDirectoryButton;
        private readonly TextField directoryTextField;
        private readonly Button cancelButton;
        private readonly Texture2D selectedTexture = Utility.MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.4f));
        private Vector2 listScrollPos;
        private bool createDirectoryMode;

        public SceneManagerDirectoryPane(SceneManager sceneManager, SceneModalWindow sceneModalWindow)
        {
            this.sceneManager = sceneManager;
            this.sceneModalWindow = sceneModalWindow;

            createDirectoryButton = new Button(Translation.Get("sceneManager", "createDirectoryButton"));
            createDirectoryButton.ControlEvent += (s, a) => createDirectoryMode = true;

            deleteDirectoryButton = new Button(Translation.Get("sceneManager", "deleteDirectoryButton"));
            deleteDirectoryButton.ControlEvent += (s, a) => this.sceneModalWindow.ShowDirectoryDialogue();

            directoryTextField = new TextField();
            directoryTextField.ControlEvent += (s, a) =>
            {
                sceneManager.AddDirectory(directoryTextField.Value);
                createDirectoryMode = false;
                directoryTextField.Value = string.Empty;
            };

            cancelButton = new Button("X");
            cancelButton.ControlEvent += (s, a) => createDirectoryMode = false;
        }

        protected override void ReloadTranslation()
        {
            createDirectoryButton.Label = Translation.Get("sceneManager", "createDirectoryButton");
            deleteDirectoryButton.Label = Translation.Get("sceneManager", "deleteDirectoryButton");
        }

        public override void Draw()
        {
            GUIStyle directoryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12),
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 0, 0)
            };

            GUIStyle directorySelectedStyle = new GUIStyle(directoryStyle);
            directorySelectedStyle.normal.textColor = Color.white;
            directorySelectedStyle.normal.background = selectedTexture;
            directorySelectedStyle.hover.background = selectedTexture;

            GUILayout.BeginVertical(GUILayout.Width(Utility.GetPix(listWidth)));

            listScrollPos = GUILayout.BeginScrollView(listScrollPos);

            for (int i = 0; i < sceneManager.CurrentDirectoryList.Count; i++)
            {
                GUIStyle style = i == sceneManager.CurrentDirectoryIndex ? directorySelectedStyle : directoryStyle;
                string directoryName = sceneManager.CurrentDirectoryList[i];
                if (GUILayout.Button(directoryName, style, GUILayout.Height(Utility.GetPix(20))))
                {
                    sceneManager.SelectDirectory(i);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = Utility.GetPix(12) };

            GUILayoutOption buttonHeight = GUILayout.Height(Utility.GetPix(20));

            if (createDirectoryMode)
            {
                directoryTextField.Draw(buttonHeight, GUILayout.Width(Utility.GetPix(listWidth - 30)));
                cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
            }
            else
            {
                createDirectoryButton.Draw(buttonStyle, buttonHeight);
                GUI.enabled = sceneManager.CurrentDirectoryIndex > 0;
                deleteDirectoryButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}
