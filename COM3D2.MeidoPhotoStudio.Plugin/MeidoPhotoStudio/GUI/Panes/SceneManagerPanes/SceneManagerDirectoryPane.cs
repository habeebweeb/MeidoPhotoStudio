using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneManagerDirectoryPane : BasePane
    {
        public static readonly int listWidth = 200;
        private SceneManager sceneManager;
        private SceneModalWindow sceneModalWindow;
        private Button createDirectoryButton;
        private Button deleteDirectoryButton;
        private TextField directoryTextField;
        private Button cancelButton;
        private Vector2 listScrollPos;
        private bool createDirectoryMode;
        private Texture2D selectedTexture = Utility.MakeTex(2, 2, new Color(0.5f, 0.5f, 0.5f, 0.4f));

        public SceneManagerDirectoryPane(SceneManager sceneManager, SceneModalWindow sceneModalWindow)
        {
            this.sceneManager = sceneManager;
            this.sceneModalWindow = sceneModalWindow;

            this.createDirectoryButton = new Button("New Folder");
            this.createDirectoryButton.ControlEvent += (s, a) => createDirectoryMode = true;

            this.deleteDirectoryButton = new Button("Delete");
            this.deleteDirectoryButton.ControlEvent += (s, a) => sceneModalWindow.ShowDirectoryDialogue();

            this.directoryTextField = new TextField();
            this.directoryTextField.ControlEvent += (s, a) =>
            {
                sceneManager.AddDirectory(directoryTextField.Value);
                createDirectoryMode = false;
                directoryTextField.Value = string.Empty;
            };

            this.cancelButton = new Button("X");
            this.cancelButton.ControlEvent += (s, a) => createDirectoryMode = false;
        }

        public override void Draw()
        {
            GUIStyle directoryStyle = new GUIStyle(GUI.skin.button);
            directoryStyle.fontSize = Utility.GetPix(12);
            directoryStyle.alignment = TextAnchor.MiddleLeft;
            directoryStyle.margin = new RectOffset(0, 0, 0, 0);

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

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = Utility.GetPix(12);

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
