using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SceneBrowserWindow : BaseWindow
{
    private const float ThumbnailScale = 0.4f;
    private const float ResizeHandleSize = 15f;
    private const int CategoryListWidth = 200;
    private const int FontSize = 13;

    private static readonly Texture2D CategorySelectedTexture = Utility.MakeTex(2, 2, new(0.5f, 0.5f, 0.5f, 0.4f));
    private static readonly Vector2 ThumbnailDimensions = new(480f, 270f);

    private readonly SceneRepository sceneRepository;
    private readonly SceneManagementModal sceneManagementModal;
    private readonly SceneSchemaBuilder sceneSchemaBuilder;
    private readonly SceneBrowserConfiguration configuration;
    private readonly ScreenshotService screenshotService;
    private readonly LazyStyle labelStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private readonly LazyStyle categoryStyle = new(
        FontSize,
        () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new(0, 0, 0, 0),
        });

    private readonly LazyStyle selectedCategoryStyle = new(
        FontSize,
        () => new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new(0, 0, 0, 0),
            normal =
                {
                    textColor = Color.white,
                    background = CategorySelectedTexture,
                },
            hover = { background = CategorySelectedTexture },
        });

    private readonly LazyStyle deleteButtonStyle = new(
        FontSize,
        () => new(GUI.skin.button)
        {
            margin = new(0, 0, 0, 0),
        });

    private readonly LazyStyle thumbnailStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 0, 0),
        });

    private readonly TextField categoryNameTextfield;
    private readonly TextField sceneNameTextfield;
    private readonly Button refreshScenesButton;
    private readonly Button addCategoryButton;
    private readonly Button saveSceneButton;
    private readonly Button closeButton;
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

        closeButton = new("X");
        closeButton.ControlEvent += OnCloseButtonPushed;

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
                var minimumWidth = Utility.GetPix(CategoryListWidth + ThumbnailDimensions.x * ThumbnailScale + 38);
                var minimumHeight = Utility.GetPix(ThumbnailDimensions.y * ThumbnailScale + 40);

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

        var categoryWidth = GUILayout.Width(Utility.GetPix(CategoryListWidth));

        DrawTitleBar();

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(categoryWidth);

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

            GUILayout.BeginHorizontal(categoryWidth);

            refreshScenesButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            GUILayout.Label(sortLabel, labelStyle);

            sortingModesDropdown.Draw();

            descendingToggle.Draw();

            GUILayout.FlexibleSpace();

            closeButton.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawCategories()
        {
            if (!hasCategories)
            {
                GUILayout.Label(noCategoriesLabel, labelStyle);

                return;
            }

            categoryScrollPosition = GUILayout.BeginScrollView(categoryScrollPosition);

            var buttonLayoutOption = GUILayout.ExpandWidth(false);

            foreach (var category in currentCategories)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(
                    category,
                    string.Equals(category, currentCategory, StringComparison.Ordinal) ? selectedCategoryStyle : categoryStyle))
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

            GUILayout.BeginHorizontal(categoryWidth);

            categoryNameTextfield.Draw();

            addCategoryButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            sceneNameTextfield.Draw();

            saveSceneButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        sceneManagementModal.OnScreenDimensionsChanged(newScreenDimensions);

        var minimumWidth = Utility.GetPix(CategoryListWidth + ThumbnailDimensions.x * ThumbnailScale + 20);
        var minimumHeight = Utility.GetPix(ThumbnailDimensions.y * ThumbnailScale + 40);

        WindowRect = windowRect with
        {
            width = Mathf.Max(minimumWidth, windowRect.width),
            height = Mathf.Max(minimumHeight, windowRect.height),
        };
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

    private void OnCloseButtonPushed(object sender, EventArgs e) =>
        Visible = false;

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
