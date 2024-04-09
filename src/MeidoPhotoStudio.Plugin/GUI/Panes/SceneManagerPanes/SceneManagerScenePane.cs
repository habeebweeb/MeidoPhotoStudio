namespace MeidoPhotoStudio.Plugin;

public class SceneManagerScenePane : BasePane
{
    public static readonly float ThumbnailScale = 0.55f;
    private readonly SceneManager sceneManager;
    private readonly SceneModalWindow sceneModalWindow;
    private readonly Button addSceneButton;
    private Vector2 sceneScrollPos;

    public SceneManagerScenePane(SceneManager sceneManager, SceneModalWindow sceneModalWindow)
    {
        this.sceneManager = sceneManager;
        this.sceneModalWindow = sceneModalWindow;

        addSceneButton = new("+");
        addSceneButton.ControlEvent += (_, _) =>
            sceneManager.SaveScene(overwrite: false);
    }

    public override void Draw()
    {
        var sceneImageStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, 0),
        };

        var addSceneStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 60,
        };

        GUILayout.BeginVertical();

        var sceneWidth = SceneManager.SceneDimensions.x * ThumbnailScale;
        var sceneHeight = SceneManager.SceneDimensions.y * ThumbnailScale;
        var sceneGridWidth = parent.WindowRect.width - SceneManagerDirectoryPane.ListWidth;

        var sceneLayoutOptions = new[]
        {
            GUILayout.Height(sceneHeight),
            GUILayout.Width(sceneWidth),
        };

        var columns = Mathf.Max(1, (int)(sceneGridWidth / sceneWidth));
        var rows = (int)Mathf.Ceil(sceneManager.SceneList.Count + 1 / (float)columns);

        sceneScrollPos = GUILayout.BeginScrollView(sceneScrollPos);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical();

        var currentScene = -1;

        for (var i = 0; i < rows; i++)
        {
            GUILayout.BeginHorizontal();

            for (var j = 0; j < columns; j++, currentScene++)
            {
                if (currentScene is -1)
                {
                    addSceneButton.Draw(addSceneStyle, sceneLayoutOptions);
                }
                else if (currentScene < sceneManager.SceneList.Count)
                {
                    var scene = sceneManager.SceneList[currentScene];

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
