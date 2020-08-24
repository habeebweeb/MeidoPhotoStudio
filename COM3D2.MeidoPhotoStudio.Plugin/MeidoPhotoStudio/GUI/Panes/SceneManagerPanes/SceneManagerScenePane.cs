using UnityEngine;


namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneManagerScenePane : BasePane
    {
        public static readonly float thumbnailScale = 0.55f;
        private SceneManager sceneManager;
        private SceneModalWindow sceneModalWindow;
        private Button addSceneButton;
        private Vector2 sceneScrollPos;

        public SceneManagerScenePane(SceneManager sceneManager, SceneModalWindow sceneModalWindow)
        {
            this.sceneManager = sceneManager;
            this.sceneModalWindow = sceneModalWindow;

            this.addSceneButton = new Button("+");
            this.addSceneButton.ControlEvent += (s, a) => sceneManager.SaveScene(overwrite: false);
        }

        public override void Draw()
        {
            GUIStyle sceneImageStyle = new GUIStyle(GUI.skin.label);
            sceneImageStyle.alignment = TextAnchor.MiddleCenter;
            sceneImageStyle.padding = new RectOffset(0, 0, 0, 0);

            GUIStyle addSceneStyle = new GUIStyle(GUI.skin.button);
            addSceneStyle.alignment = TextAnchor.MiddleCenter;
            addSceneStyle.fontSize = 60;

            GUILayout.BeginVertical();

            float sceneWidth = SceneManager.sceneDimensions.x * thumbnailScale;
            float sceneHeight = SceneManager.sceneDimensions.y * thumbnailScale;
            float sceneGridWidth = parent.WindowRect.width - SceneManagerDirectoryPane.listWidth;

            GUILayoutOption[] sceneLayoutOptions = new[] { GUILayout.Height(sceneHeight), GUILayout.Width(sceneWidth) };

            int columns = Mathf.Max(1, (int)(sceneGridWidth / sceneWidth));
            int rows = (int)Mathf.Ceil((float)sceneManager.SceneList.Count + 1 / (float)columns);

            sceneScrollPos = GUILayout.BeginScrollView(sceneScrollPos);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            int currentScene = -1;
            for (int i = 0; i < rows; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < columns; j++, currentScene++)
                {
                    if (currentScene == -1)
                    {
                        addSceneButton.Draw(addSceneStyle, sceneLayoutOptions);
                    }
                    else if (currentScene < sceneManager.SceneList.Count)
                    {
                        SceneManager.Scene scene = sceneManager.SceneList[currentScene];
                        if (GUILayout.Button(scene.Thumbnail, sceneImageStyle, sceneLayoutOptions))
                        {
                            sceneManager.SelectScene(currentScene);
                            sceneModalWindow.ShowSceneDialogue();
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
