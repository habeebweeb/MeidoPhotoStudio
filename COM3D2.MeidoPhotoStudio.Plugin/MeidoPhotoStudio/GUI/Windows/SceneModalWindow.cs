using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class SceneModalWindow : BaseWindow
    {
        private static readonly Texture2D infoHighlight = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
        private readonly SceneManager sceneManager;
        public override Rect WindowRect
        {
            set
            {
                value.width = Mathf.Clamp(Screen.width * 0.3f, 360f, 500f);
                value.height = directoryMode ? 150f : Mathf.Clamp(Screen.height * 0.4f, 240f, 380f);
                base.WindowRect = value;
            }
        }
        private bool visible;
        public override bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                if (value)
                {
                    WindowRect = WindowRect;
                    windowRect.x = MiddlePosition.x;
                    windowRect.y = MiddlePosition.y;
                }
            }
        }

        private readonly Button okButton;
        private readonly Button cancelButton;
        private readonly Button deleteButton;
        private readonly Button overwriteButton;
        private string deleteDirectoryMessage;
        private string deleteSceneMessage;
        private string directoryDeleteCommit;
        private string sceneDeleteCommit;
        private string sceneLoadCommit;
        private string infoKankyo;
        private string infoMaidSingular;
        private string infoMaidPlural;
        private bool directoryMode;
        private bool deleteScene;

        public SceneModalWindow(SceneManager sceneManager)
        {
            ReloadTranslation();

            this.sceneManager = sceneManager;

            windowRect.x = MiddlePosition.x;
            windowRect.y = MiddlePosition.y;
            okButton = new Button(sceneLoadCommit);
            okButton.ControlEvent += (s, a) => Commit();

            cancelButton = new Button("Cancel");
            cancelButton.ControlEvent += (s, a) => Cancel();

            deleteButton = new Button("Delete");
            deleteButton.ControlEvent += (s, a) =>
            {
                okButton.Label = sceneDeleteCommit;
                deleteScene = true;
            };

            overwriteButton = new Button("Overwrite");
            overwriteButton.ControlEvent += (s, a) =>
            {
                sceneManager.OverwriteScene();
                Visible = false;
            };
        }

        protected override void ReloadTranslation()
        {
            deleteDirectoryMessage = Translation.Get("sceneManagerModal", "deleteDirectoryConfirm");
            deleteSceneMessage = Translation.Get("sceneManagerModal", "deleteFileConfirm");
            directoryDeleteCommit = Translation.Get("sceneManagerModal", "deleteDirectoryButton");
            sceneDeleteCommit = Translation.Get("sceneManagerModal", "deleteFileCommit");
            sceneLoadCommit = Translation.Get("sceneManagerModal", "fileLoadCommit");
            infoKankyo = Translation.Get("sceneManagerModal", "infoKankyo");
            infoMaidSingular = Translation.Get("sceneManagerModal", "infoMaidSingular");
            infoMaidPlural = Translation.Get("sceneManagerModal", "infoMaidPlural");
        }

        public override void Draw()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

            // thumbnail
            if (!directoryMode)
            {
                SceneManager.Scene scene = sceneManager.CurrentScene;
                Texture2D thumb = scene.Thumbnail;

                float scale = Mathf.Min(
                    (WindowRect.width - 20f) / thumb.width, (WindowRect.height - 110f) / thumb.height
                );
                float width = Mathf.Min(thumb.width, thumb.width * scale);
                float height = Mathf.Min(thumb.height, thumb.height * scale);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                MpsGui.DrawTexture(thumb, GUILayout.Width(width), GUILayout.Height(height));

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Utility.GetPix(12),
                    alignment = TextAnchor.MiddleCenter
                };
                labelStyle.normal.background = infoHighlight;

                Rect labelBox = GUILayoutUtility.GetLastRect();

                if (scene.NumberOfMaids != SceneManager.Scene.initialNumberOfMaids)
                {
                    int numberOfMaids = scene.NumberOfMaids;
                    string infoString = numberOfMaids == MeidoPhotoStudio.kankyoMagic
                        ? infoKankyo
                        : string.Format(numberOfMaids == 1 ? infoMaidSingular : infoMaidPlural, numberOfMaids);

                    Vector2 labelSize = labelStyle.CalcSize(new GUIContent(infoString));

                    labelBox = new Rect(
                        labelBox.x + 10, labelBox.y + labelBox.height - (labelSize.y + 10),
                        labelSize.x + 10, labelSize.y + 2
                    );

                    GUI.Label(labelBox, infoString, labelStyle);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            // message
            string currentMessage;
            string context;

            if (directoryMode)
            {
                currentMessage = deleteDirectoryMessage;
                context = sceneManager.CurrentDirectoryName;
            }
            else
            {
                if (deleteScene)
                {
                    currentMessage = deleteSceneMessage;
                    context = sceneManager.CurrentScene.FileInfo.Name;
                }
                else
                {
                    currentMessage = sceneManager.CurrentScene.FileInfo.Name;
                    context = currentMessage;
                }
            }

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = Utility.GetPix(12)
            };

            GUILayout.FlexibleSpace();

            GUILayout.Label(string.Format(currentMessage, context), messageStyle);

            GUILayout.FlexibleSpace();

            // Buttons

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12)
            };

            GUILayoutOption buttonHeight = GUILayout.Height(Utility.GetPix(20));

            GUILayout.BeginHorizontal();

            if (directoryMode || deleteScene)
            {
                GUILayout.FlexibleSpace();
                okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.Width(100));
            }
            else
            {
                deleteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                overwriteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));

                GUILayout.FlexibleSpace();

                okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.Width(100));
            }

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        public void ShowDirectoryDialogue()
        {
            okButton.Label = directoryDeleteCommit;
            directoryMode = true;
            Modal.Show(this);
        }

        public void ShowSceneDialogue()
        {
            directoryMode = false;
            okButton.Label = sceneLoadCommit;
            Modal.Show(this);
        }

        private void Commit()
        {
            if (directoryMode)
            {
                sceneManager.DeleteDirectory();
                Modal.Close();
            }
            else
            {
                if (deleteScene)
                {
                    sceneManager.DeleteScene();
                    deleteScene = false;
                }
                else sceneManager.LoadScene();

                Modal.Close();
            }
        }

        private void Cancel()
        {
            if (directoryMode) Modal.Close();
            else
            {
                if (deleteScene) deleteScene = false;
                else Modal.Close();
            }
            okButton.Label = sceneLoadCommit;
        }
    }
}
