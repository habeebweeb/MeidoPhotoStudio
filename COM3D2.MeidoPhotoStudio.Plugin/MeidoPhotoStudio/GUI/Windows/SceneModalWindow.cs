using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class SceneModalWindow : BaseWindow
    {
        private static Texture2D infoHighlight = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
        private SceneManager sceneManager;
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

        private Button okButton;
        private Button cancelButton;
        private Button deleteButton;
        private Button overwriteButton;
        private string deleteDirectoryMessage;
        private string deleteSceneMessage;
        private string directoryDeleteCommit;
        private string sceneDeleteCommit;
        private string sceneLoadCommit;
        private string infoKankyo;
        private string infoMaidSingular;
        private string infoMaidPlural;
        private bool directoryMode = false;
        private bool deleteScene = false;

        public SceneModalWindow(SceneManager sceneManager)
        {
            ReloadTranslation();

            this.sceneManager = sceneManager;

            windowRect.x = MiddlePosition.x;
            windowRect.y = MiddlePosition.y;
            this.okButton = new Button(sceneLoadCommit);
            this.okButton.ControlEvent += (s, a) => Commit();

            this.cancelButton = new Button("Cancel");
            this.cancelButton.ControlEvent += (s, a) => Cancel();

            this.deleteButton = new Button("Delete");
            this.deleteButton.ControlEvent += (s, a) =>
            {
                okButton.Label = sceneDeleteCommit;
                deleteScene = true;
            };

            this.overwriteButton = new Button("Overwrite");
            this.overwriteButton.ControlEvent += (s, a) =>
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
            sceneLoadCommit = Translation.Get("sceneManagerModal", "fileLoadCommit"); ;
            infoKankyo = Translation.Get("sceneManagerModal", "infoKankyo");
            infoMaidSingular = Translation.Get("sceneManagerModal", "infoMaidSingular"); ;
            infoMaidPlural = Translation.Get("sceneManagerModal", "infoMaidPlural"); ;
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
                    (WindowRect.width - 20f) / (float)thumb.width, (WindowRect.height - 110f) / (float)thumb.height
                );
                float width = Mathf.Min(thumb.width, (float)thumb.width * scale);
                float height = Mathf.Min(thumb.height, (float)thumb.height * scale);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                MiscGUI.DrawTexture(thumb, GUILayout.Width(width), GUILayout.Height(height));

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = Utility.GetPix(12);
                labelStyle.alignment = TextAnchor.MiddleCenter;
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
            string currentMessage = string.Empty;
            string context = string.Empty;

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

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label);
            messageStyle.alignment = TextAnchor.MiddleCenter;
            messageStyle.fontSize = Utility.GetPix(12);

            GUILayout.FlexibleSpace();

            GUILayout.Label(string.Format(currentMessage, context), messageStyle);

            GUILayout.FlexibleSpace();

            // Buttons

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = Utility.GetPix(12);

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
