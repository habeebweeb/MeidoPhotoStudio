namespace MeidoPhotoStudio.Plugin;

public class SceneManagerTitleBarPane : BasePane
{
    private static readonly string[] SortModes = new[] { "sortName", "sortCreated", "sortModified" };

    private readonly SceneManager sceneManager;
    private readonly Button kankyoToggle;
    private readonly Button refreshButton;
    private readonly Dropdown sortDropdown;
    private readonly Toggle descendingToggle;
    private readonly Button closeButton;

    private string sortLabel;

    public SceneManagerTitleBarPane(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        kankyoToggle = new(Translation.Get("sceneManager", "kankyoToggle"));
        kankyoToggle.ControlEvent += (_, _) =>
            sceneManager.ToggleKankyoMode();

        refreshButton = new(Translation.Get("sceneManager", "refreshButton"));
        refreshButton.ControlEvent += (_, _) =>
            sceneManager.Refresh();

        sortDropdown = new(Translation.GetArray("sceneManager", SortModes), (int)sceneManager.CurrentSortMode);
        sortDropdown.SelectionChange += (_, _) =>
        {
            var sortMode = (SceneManager.SortMode)sortDropdown.SelectedItemIndex;

            if (sceneManager.CurrentSortMode == sortMode)
                return;

            sceneManager.SortScenes(sortMode);
        };

        descendingToggle = new(Translation.Get("sceneManager", "descendingToggle"), sceneManager.SortDescending);
        descendingToggle.ControlEvent += (_, _) =>
        {
            sceneManager.SortDescending = descendingToggle.Value;
            sceneManager.SortScenes(sceneManager.CurrentSortMode);
        };

        closeButton = new("X");
        closeButton.ControlEvent += (_, _) =>
            CloseChange?.Invoke(this, EventArgs.Empty);

        sortLabel = Translation.Get("sceneManager", "sortLabel");
    }

    public event EventHandler CloseChange;

    public override void Draw()
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Utility.GetPix(12),
        };

        var buttonHeight = GUILayout.Height(Utility.GetPix(20));

        GUILayout.BeginHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(Utility.GetPix(SceneManagerDirectoryPane.ListWidth)));

        var originalColour = GUI.backgroundColor;

        if (sceneManager.KankyoMode)
            GUI.backgroundColor = Color.green;

        kankyoToggle.Draw(buttonStyle, buttonHeight);
        GUI.backgroundColor = originalColour;

        GUILayout.FlexibleSpace();

        refreshButton.Draw(buttonStyle, buttonHeight);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Space(Utility.GetPix(15));

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = buttonStyle.fontSize,
        };

        GUILayout.Label(sortLabel, labelStyle);

        var dropdownStyle = new GUIStyle(DropdownHelper.DefaultDropdownStyle)
        {
            fontSize = buttonStyle.fontSize,
        };

        sortDropdown.Draw(buttonStyle, dropdownStyle, buttonHeight, GUILayout.Width(Utility.GetPix(100)));

        var toggleStyle = new GUIStyle(GUI.skin.toggle)
        {
            fontSize = buttonStyle.fontSize,
        };

        descendingToggle.Draw(toggleStyle);

        GUILayout.FlexibleSpace();

        closeButton.Draw();

        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        kankyoToggle.Label = Translation.Get("sceneManager", "kankyoToggle");
        refreshButton.Label = Translation.Get("sceneManager", "refreshButton");
        sortDropdown.SetDropdownItems(Translation.GetArray("sceneManager", SortModes));
        descendingToggle.Label = Translation.Get("sceneManager", "descendingToggle");
        sortLabel = Translation.Get("sceneManager", "sortLabel");
    }
}
