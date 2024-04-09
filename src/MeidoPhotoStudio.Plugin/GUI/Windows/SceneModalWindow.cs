namespace MeidoPhotoStudio.Plugin;

public class SceneModalWindow : BaseWindow
{
    private static readonly Texture2D InfoHighlight = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.8f));

    private readonly SceneManager sceneManager;
    private readonly Button okButton;
    private readonly Button cancelButton;
    private readonly Button deleteButton;
    private readonly Button overwriteButton;

    private bool visible;
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
        this.sceneManager = sceneManager;

        windowRect.x = MiddlePosition.x;
        windowRect.y = MiddlePosition.y;

        okButton = new(sceneLoadCommit);
        okButton.ControlEvent += (_, _) =>
            Commit();

        cancelButton = new(Translation.Get("sceneManagerModal", "cancelButton"));
        cancelButton.ControlEvent += (_, _) =>
            Cancel();

        deleteButton = new(Translation.Get("sceneManagerModal", "deleteButton"));
        deleteButton.ControlEvent += (_, _) =>
        {
            okButton.Label = sceneDeleteCommit;
            deleteScene = true;
        };

        overwriteButton = new(Translation.Get("sceneManagerModal", "overwriteButton"));
        overwriteButton.ControlEvent += (_, _) =>
        {
            sceneManager.OverwriteScene();
            Visible = false;
        };

        ReloadTranslation();
    }

    public override Rect WindowRect
    {
        set
        {
            value.width = Mathf.Clamp(Screen.width * 0.3f, 360f, 500f);
            value.height = directoryMode ? 150f : Mathf.Clamp(Screen.height * 0.4f, 240f, 380f);

            base.WindowRect = value;
        }
    }

    public override bool Visible
    {
        get => visible;
        set
        {
            visible = value;

            if (!value)
                return;

            WindowRect = WindowRect;
            windowRect.x = MiddlePosition.x;
            windowRect.y = MiddlePosition.y;
        }
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

        // thumbnail
        if (!directoryMode)
        {
            var scene = sceneManager.CurrentScene;
            var thumb = scene.Thumbnail;

            var scale = Mathf.Min((WindowRect.width - 20f) / thumb.width, (WindowRect.height - 110f) / thumb.height);
            var width = Mathf.Min(thumb.width, thumb.width * scale);
            var height = Mathf.Min(thumb.height, thumb.height * scale);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            MpsGui.DrawTexture(thumb, GUILayout.Width(width), GUILayout.Height(height));

            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Utility.GetPix(12),
                alignment = TextAnchor.MiddleCenter,
            };

            labelStyle.normal.background = InfoHighlight;

            var labelBox = GUILayoutUtility.GetLastRect();

            var infoString = scene.Environment
                ? infoKankyo
                : string.Format(scene.NumberOfMaids is 1 ? infoMaidSingular : infoMaidPlural, scene.NumberOfMaids);

            var labelSize = labelStyle.CalcSize(new GUIContent(infoString));

            labelBox = new(
                labelBox.x + 10, labelBox.y + labelBox.height - (labelSize.y + 10), labelSize.x + 10, labelSize.y + 2);

            GUI.Label(labelBox, infoString, labelStyle);

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

        var messageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Utility.GetPix(12),
        };

        GUILayout.FlexibleSpace();

        GUILayout.Label(string.Format(currentMessage, context), messageStyle);

        GUILayout.FlexibleSpace();

        // Buttons
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Utility.GetPix(12),
        };

        var buttonHeight = GUILayout.Height(Utility.GetPix(20));

        GUILayout.BeginHorizontal();

        if (directoryMode || deleteScene)
        {
            GUILayout.FlexibleSpace();
            okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
            cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.MinWidth(100));
        }
        else
        {
            deleteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
            overwriteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));

            GUILayout.FlexibleSpace();

            okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
            cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.MinWidth(100));
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
        cancelButton.Label = Translation.Get("sceneManagerModal", "cancelButton");
        deleteButton.Label = Translation.Get("sceneManagerModal", "deleteButton");
        overwriteButton.Label = Translation.Get("sceneManagerModal", "overwriteButton");
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
            else
            {
                sceneManager.LoadScene(sceneManager.CurrentScene);
            }

            Modal.Close();
        }
    }

    private void Cancel()
    {
        if (directoryMode)
        {
            Modal.Close();
        }
        else
        {
            if (deleteScene)
                deleteScene = false;
            else
                Modal.Close();
        }

        okButton.Label = sceneLoadCommit;
    }
}
