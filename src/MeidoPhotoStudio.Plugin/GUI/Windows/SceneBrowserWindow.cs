using MeidoPhotoStudio.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class SceneBrowserWindow : BaseWindow
{
    private const float ThumbnailScale = 0.4f;
    private const float ResizeHandleSize = 15f;
    private const int CategoryListWidth = 200;

    private static readonly Texture2D CategorySelectedTexture = Utility.MakeTex(2, 2, new(0.5f, 0.5f, 0.5f, 0.4f));
    private static readonly Vector2 ThumbnailDimensions = new(480f, 270f);

    private readonly SceneRepository sceneRepository;
    private readonly SceneManagementModal sceneManagementModal;
    private readonly SceneSchemaBuilder sceneSchemaBuilder;
    private readonly SceneBrowserConfiguration configuration;
    private readonly ScreenshotService screenshotService;
    private readonly TextField categoryNameTextfield;
    private readonly TextField sceneNameTextfield;
    private readonly Button refreshScenesButton;
    private readonly Button addCategoryButton;
    private readonly Button saveSceneButton;
    private readonly Dropdown<SortingMode> sortingModesDropdown;
    private readonly Toggle descendingToggle;

    private bool resizing;
    private Rect resizeHandleRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private string currentCategory = string.Empty;
    private IEnumerable<SceneModel> currentCategoryScenes;
    private IEnumerable<string> currentCategories;
    private Vector2 scenesScrollPosition;
    private Vector2 categoryScrollPosition;
    private bool hasCategories;
    private bool hasScenes;
    private string sortLabel = string.Empty;
    private string noScenesLabel = string.Empty;
    private string noCategoriesLabel = string.Empty;

    public SceneBrowserWindow(
        SceneRepository sceneRepository,
        SceneManagementModal sceneManagementModal,
        SceneSchemaBuilder sceneSchemaBuilder,
        ScreenshotService screenshotService,
        SceneBrowserConfiguration configuration)
    {
        this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
        this.sceneManagementModal = sceneManagementModal ?? throw new ArgumentNullException(nameof(sceneManagementModal));
        this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
        this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        this.screenshotService = screenshotService
            ? screenshotService
            : throw new ArgumentNullException(nameof(screenshotService));

        this.sceneRepository.AddedScene += OnScenesChanged;
        this.sceneRepository.RemovedScene += OnScenesChanged;
        this.sceneRepository.AddedCategory += OnCategoriesChanged;
        this.sceneRepository.RemovedCategory += OnCategoriesChanged;
        this.sceneRepository.Refreshing += OnRefreshing;
        this.sceneRepository.Refreshed += OnRefreshed;

        categoryNameTextfield = new();
        categoryNameTextfield.ControlEvent += OnAddCategoryButtonPushed;

        sceneNameTextfield = new();
        sceneNameTextfield.ControlEvent += OnSaveSceneButtonPushed;

        refreshScenesButton = new(Translation.Get("sceneManager", "refreshButton"));
        refreshScenesButton.ControlEvent += OnRefreshScenesButtonPushed;

        addCategoryButton = new(Translation.Get("sceneManager", "createDirectoryButton"));
        addCategoryButton.ControlEvent += OnAddCategoryButtonPushed;

        saveSceneButton = new(Translation.Get("sceneManager", "saveSceneButton"));
        saveSceneButton.ControlEvent += OnSaveSceneButtonPushed;

        sortLabel = Translation.Get("sceneManager", "sortLabel");
        noScenesLabel = Translation.Get("sceneManager", "noScenesLabel");
        noCategoriesLabel = Translation.Get("sceneManager", "noDirectoriesLabel");

        var sortingModeTranslationkeys = new Dictionary<SortingMode, string>()
        {
            [SortingMode.Name] = "sortName",
            [SortingMode.DateCreated] = "sortCreated",
            [SortingMode.DateModified] = "sortModified",
        };

        var sortingModes = new SortingMode[] { SortingMode.Name, SortingMode.DateCreated, SortingMode.DateModified };

        sortingModesDropdown = new(
            sortingModes,
            Array.IndexOf(sortingModes, this.configuration.SortingMode),
            formatter: (sortingMode, _) => Translation.Get("sceneManager", sortingModeTranslationkeys[sortingMode]));

        sortingModesDropdown.SelectionChanged += OnSortOptionChanged;

        descendingToggle = new(Translation.Get("sceneManager", "descendingToggle"), this.configuration.SortDescending);
        descendingToggle.ControlEvent += OnDescendingToggleChanged;

        PopulateWindow();

        void PopulateWindow()
        {
            currentCategory = sceneRepository.RootCategoryName;

            currentCategoryScenes = GetScenes(currentCategory);
            currentCategories = GetCategories();

            WindowRect = new Rect(
                Screen.width * 0.5f - Screen.width * 0.65f / 2f,
                Screen.height * 0.5f - Screen.height * 0.75f / 2f,
                Screen.width * 0.65f,
                Screen.height * 0.75f);

            hasCategories = this.sceneRepository.Categories.Any();
            hasScenes = currentCategoryScenes.Any();
        }
    }

    public enum SortingMode
    {
        Name,
        DateCreated,
        DateModified,
    }

    private bool Descending =>
        descendingToggle.Value;

    public override void GUIFunc(int id)
    {
        HandleResize();
        Draw();

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandleRect = resizeHandleRect with
            {
                x = windowRect.width - ResizeHandleSize,
                y = windowRect.height - ResizeHandleSize,
            };

            if (resizing && !UnityEngine.Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && UnityEngine.Input.GetMouseButtonDown(0) && resizeHandleRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var mousePosition = Event.current.mousePosition;

                var (windowWidth, windowHeight) = mousePosition;
                var minimumWidth = Utility.GetPix((int)(CategoryListWidth + ThumbnailDimensions.x * ThumbnailScale + 38));
                var minimumHeight = Utility.GetPix((int)(ThumbnailDimensions.y * ThumbnailScale + 40));

                WindowRect = windowRect with
                {
                    width = Mathf.Max(minimumWidth, windowWidth + ResizeHandleSize / 2f),
                    height = Mathf.Max(minimumHeight, windowHeight + ResizeHandleSize / 2f),
                };
            }
        }
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

        DrawTitleBar();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(Utility.GetPix(CategoryListWidth)));

        DrawCategories();

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        DrawScenes();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        DrawFooter();

        GUILayout.EndArea();

        GUI.Box(resizeHandleRect, GUIContent.none);

        void DrawTitleBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Utility.GetPix(CategoryListWidth)));

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12),
            };

            refreshScenesButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            GUILayout.Label(sortLabel, new GUIStyle(GUI.skin.label) { fontSize = Utility.GetPix(12) });

            sortingModesDropdown.Draw(buttonStyle);

            descendingToggle.Draw(new GUIStyle(GUI.skin.toggle)
            {
                fontSize = Utility.GetPix(12),
            });

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("X", buttonStyle))
                Visible = !Visible;

            GUILayout.EndHorizontal();
        }

        void DrawCategories()
        {
            if (!hasCategories)
            {
                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Utility.GetPix(11),
                    alignment = TextAnchor.MiddleCenter,
                };

                GUILayout.Label(noCategoriesLabel, labelStyle);

                return;
            }

            categoryScrollPosition = GUILayout.BeginScrollView(categoryScrollPosition);

            var categoryStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12),
                alignment = TextAnchor.MiddleLeft,
                margin = new(0, 0, 0, 0),
            };

            var categorySelectedStyle = new GUIStyle(categoryStyle);

            categorySelectedStyle.normal.textColor = Color.white;
            categorySelectedStyle.normal.background = CategorySelectedTexture;
            categorySelectedStyle.hover.background = CategorySelectedTexture;

            var deleteButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12),
                margin = new(0, 0, 0, 0),
            };

            var buttonLayoutOption = GUILayout.ExpandWidth(false);

            foreach (var category in currentCategories)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(
                    category,
                    string.Equals(category, currentCategory, StringComparison.Ordinal) ? categorySelectedStyle : categoryStyle))
                    ChangeCategory(category);

                if (!string.Equals(category, sceneRepository.RootCategoryName, StringComparison.Ordinal)
                    && GUILayout.Button("X", deleteButtonStyle, buttonLayoutOption))
                    DeleteCategory(category);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        void DrawScenes()
        {
            if (!hasScenes)
            {
                var labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = Utility.GetPix(12),
                    alignment = TextAnchor.MiddleCenter,
                };

                GUILayout.Label(noScenesLabel, labelStyle);

                return;
            }

            scenesScrollPosition = GUILayout.BeginScrollView(scenesScrollPosition);

            var scaledThumbnailDimensions = ThumbnailDimensions * ThumbnailScale;
            var (thumbnailWidth, thumbnailHeight) = (Utility.GetPix((int)scaledThumbnailDimensions.x), Utility.GetPix((int)scaledThumbnailDimensions.y));
            var sceneGridWidth = WindowRect.width - Utility.GetPix(CategoryListWidth) - 20f;

            var columns = Mathf.Max(1, (int)(sceneGridWidth / (thumbnailWidth + 5f)));

            var thumbnailLayoutOptions = new[]
            {
                GUILayout.Height(thumbnailHeight),
                GUILayout.Width(thumbnailWidth),
            };

            var thumbnailStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
            };

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();

            foreach (var chunk in currentCategoryScenes.Chunk(columns))
            {
                GUILayout.BeginHorizontal();

                foreach (var scene in chunk)
                    if (GUILayout.Button(scene.Thumbnail, thumbnailStyle, thumbnailLayoutOptions))
                        sceneManagementModal.ManageScene(scene);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            GUILayout.EndScrollView();
        }

        void DrawFooter()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(Utility.GetPix(CategoryListWidth)));

            var textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = Utility.GetPix(12),
            };

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = Utility.GetPix(12),
            };

            categoryNameTextfield.Draw(textFieldStyle);

            addCategoryButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            sceneNameTextfield.Draw(textFieldStyle);

            saveSceneButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        refreshScenesButton.Label = Translation.Get("sceneManager", "refreshButton");
        addCategoryButton.Label = Translation.Get("sceneManager", "createDirectoryButton");
        saveSceneButton.Label = Translation.Get("sceneManager", "saveSceneButton");
        sortLabel = Translation.Get("sceneManager", "sortLabel");
        noScenesLabel = Translation.Get("sceneManager", "noScenesLabel");
        noCategoriesLabel = Translation.Get("sceneManager", "noDirectoriesLabel");
        descendingToggle.Label = Translation.Get("sceneManager", "descendingToggle");
        sortingModesDropdown.Reformat();
    }

    private static IEnumerable<SceneModel> SortScenes(IEnumerable<SceneModel> scenes, SortingMode sortingMode, bool descending) =>
        sortingMode switch
        {
            SortingMode.Name => scenes.OrderBy(scene => scene.Name, new WindowsLogicalStringComparer(), descending),
            SortingMode.DateCreated => scenes.OrderBy(scene => File.GetCreationTime(scene.Filename), descending),
            SortingMode.DateModified => scenes.OrderBy(scene => File.GetLastWriteTime(scene.Filename), descending),
            _ => throw new NotImplementedException($"'{sortingMode}' is not implemented"),
        };

    private void OnRefreshScenesButtonPushed(object sender, EventArgs e) =>
        sceneRepository.Refresh();

    private void OnScenesChanged(object sender, SceneChangeEventArgs e)
    {
        if (!string.Equals(e.Scene.Category, currentCategory, StringComparison.Ordinal))
            return;

        currentCategoryScenes = GetScenes(currentCategory);
        hasScenes = currentCategoryScenes.Any();
    }

    private void OnCategoriesChanged(object sender, CategoryChangeEventArgs e)
    {
        currentCategories = GetCategories();
        hasCategories = sceneRepository.Categories.Any();

        if (string.Equals(currentCategory, e.Category) && !sceneRepository.ContainsCategory(e.Category))
            ChangeCategory(sceneRepository.RootCategoryName);
    }

    private void OnAddCategoryButtonPushed(object sender, EventArgs e)
    {
        var name = categoryNameTextfield.Value;
        var categoryName = string.IsNullOrEmpty(name) ? "scenes" : name;

        categoryNameTextfield.Value = string.Empty;

        sceneRepository.AddCategory(categoryName);
    }

    private void OnSaveSceneButtonPushed(object sender, EventArgs e)
    {
        var name = sceneNameTextfield.Value;
        var sceneName = string.IsNullOrEmpty(name) ? $"mpsscene{DateTime.Now:yyyyMMddHHmmss}" : name;

        sceneNameTextfield.Value = string.Empty;

        screenshotService.TakeScreenshotToTexture(SaveScene, new());

        void SaveScene(Texture2D screenshot) =>
            sceneRepository.Add(sceneSchemaBuilder.Build(), screenshot, currentCategory, sceneName);
    }

    private void OnSortOptionChanged(object sender, DropdownEventArgs<SortingMode> e)
    {
        configuration.SortingMode = sortingModesDropdown.SelectedItem;

        currentCategoryScenes = SortScenes(
            currentCategoryScenes, sortingModesDropdown.SelectedItem, Descending).ToArray();
    }

    private void OnDescendingToggleChanged(object sender, EventArgs e)
    {
        configuration.SortDescending = Descending;

        currentCategoryScenes = SortScenes(
            currentCategoryScenes, sortingModesDropdown.SelectedItem, Descending).ToArray();
    }

    private void ChangeCategory(string category)
    {
        if (string.Equals(category, currentCategory, StringComparison.Ordinal))
            return;

        foreach (var scene in currentCategoryScenes)
            scene?.DestroyThumnail();

        currentCategory = category;
        currentCategories = GetCategories();
        currentCategoryScenes = GetScenes(currentCategory);
        hasScenes = currentCategories.Any();
    }

    private void OnRefreshing(object sender, EventArgs e)
    {
        currentCategories = [];
        currentCategoryScenes = [];
        hasCategories = false;
        hasScenes = false;
    }

    private void OnRefreshed(object sender, EventArgs e)
    {
        currentCategories = GetCategories();
        hasCategories = currentCategories.Any();

        if (!sceneRepository.ContainsCategory(currentCategory))
        {
            ChangeCategory(sceneRepository.RootCategoryName);
        }
        else
        {
            currentCategoryScenes = GetScenes(currentCategory);
            hasScenes = currentCategoryScenes.Any();
        }
    }

    private void DeleteCategory(string category) =>
        sceneManagementModal.DeleteCategory(category);

    private SceneModel[] GetScenes(string category) =>
        string.IsNullOrEmpty(category) || !sceneRepository.ContainsCategory(category)
            ? []
            : SortScenes(sceneRepository[category], sortingModesDropdown.SelectedItem, Descending).ToArray();

    private string[] GetCategories() =>
        [.. sceneRepository.Categories
            .OrderBy(category => !string.Equals(category, sceneRepository.RootCategoryName, StringComparison.Ordinal))
            .ThenBy(category => category, new WindowsLogicalStringComparer())];
}
