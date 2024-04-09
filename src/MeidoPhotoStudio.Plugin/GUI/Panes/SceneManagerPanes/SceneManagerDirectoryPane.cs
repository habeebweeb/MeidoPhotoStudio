namespace MeidoPhotoStudio.Plugin;

public class SceneManagerDirectoryPane : BasePane
{
    public static readonly int ListWidth = 200;

    private readonly SceneManager sceneManager;
    private readonly SceneModalWindow sceneModalWindow;
    private readonly Button createDirectoryButton;
    private readonly Button deleteDirectoryButton;
    private readonly TextField directoryTextField;
    private readonly Button cancelButton;
    private readonly Texture2D selectedTexture = Utility.MakeTex(2, 2, new(0.5f, 0.5f, 0.5f, 0.4f));

    private Vector2 listScrollPos;
    private bool createDirectoryMode;

    public SceneManagerDirectoryPane(SceneManager sceneManager, SceneModalWindow sceneModalWindow)
    {
        this.sceneManager = sceneManager;
        this.sceneModalWindow = sceneModalWindow;

        createDirectoryButton = new(Translation.Get("sceneManager", "createDirectoryButton"));
        createDirectoryButton.ControlEvent += (_, _) =>
            createDirectoryMode = true;

        deleteDirectoryButton = new(Translation.Get("sceneManager", "deleteDirectoryButton"));
        deleteDirectoryButton.ControlEvent += (_, _) =>
            this.sceneModalWindow.ShowDirectoryDialogue();

        directoryTextField = new();
        directoryTextField.ControlEvent += (_, _) =>
        {
            sceneManager.AddDirectory(directoryTextField.Value);
            createDirectoryMode = false;
            directoryTextField.Value = string.Empty;
        };

        cancelButton = new("X");
        cancelButton.ControlEvent += (_, _) =>
            createDirectoryMode = false;
    }

    public override void Draw()
    {
        var directoryStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Utility.GetPix(12),
            alignment = TextAnchor.MiddleLeft,
            margin = new(0, 0, 0, 0),
        };

        var directorySelectedStyle = new GUIStyle(directoryStyle);

        directorySelectedStyle.normal.textColor = Color.white;
        directorySelectedStyle.normal.background = selectedTexture;
        directorySelectedStyle.hover.background = selectedTexture;

        GUILayout.BeginVertical(GUILayout.Width(Utility.GetPix(ListWidth)));

        listScrollPos = GUILayout.BeginScrollView(listScrollPos);

        for (var i = 0; i < sceneManager.CurrentDirectoryList.Count; i++)
        {
            var style = i == sceneManager.CurrentDirectoryIndex ? directorySelectedStyle : directoryStyle;
            var directoryName = sceneManager.CurrentDirectoryList[i];

            if (GUILayout.Button(directoryName, style, GUILayout.Height(Utility.GetPix(20))))
                sceneManager.SelectDirectory(i);
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();

        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Utility.GetPix(12),
        };

        var buttonHeight = GUILayout.Height(Utility.GetPix(20));

        if (createDirectoryMode)
        {
            directoryTextField.Draw(buttonHeight, GUILayout.Width(Utility.GetPix(ListWidth - 30)));
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

    protected override void ReloadTranslation()
    {
        createDirectoryButton.Label = Translation.Get("sceneManager", "createDirectoryButton");
        deleteDirectoryButton.Label = Translation.Get("sceneManager", "deleteDirectoryButton");
    }
}
