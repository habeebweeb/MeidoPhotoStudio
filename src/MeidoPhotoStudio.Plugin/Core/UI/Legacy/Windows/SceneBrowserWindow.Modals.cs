using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Collections;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using WindowSize = (float Width, float Height);

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Modal windows that belong to the scene browser.</summary>
public partial class SceneBrowserWindow
{
    private class SceneManagementModal : BaseWindow
    {
        private static readonly WindowSize ManageSceneWindowSize = (540, 415);
        private static readonly WindowSize LoadOptionsWindowSize = (800, 415);

        private readonly Translation translation;
        private readonly SceneRepository sceneRepository;
        private readonly ScreenshotService screenshotService;
        private readonly SceneSchemaBuilder sceneSchemaBuilder;
        private readonly SceneLoader sceneLoader;

        private readonly LazyStyle messageStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        private readonly LazyStyle sceneInfoLabelStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { background = UIUtility.CreateTexture(2, 2, new(0f, 0f, 0f, 0.8f)) },
            });

        private readonly LazyStyle thumbnailStyle = new(
            0,
            static () => new(GUI.skin.box)
            {
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                normal = { background = Texture2D.whiteTexture },
                stretchWidth = false,
                stretchHeight = false,
            });

        private readonly Button loadSceneButton;
        private readonly Button cancelManagementButton;
        private readonly Button deleteSceneButton;
        private readonly Button overwriteSceneButton;
        private readonly Label sceneFilenameLabel;
        private readonly Toggle loadOptionsToggle;
        private readonly GUIContent characterCountContent = new();
        private readonly Label deleteMessageLabel;
        private readonly Button deleteConfirmButton;
        private readonly Button cancelDeleteButton;
        private readonly LoadOptionsPane loadOptionsPane;

        private bool deletingScene = false;
        private SceneSchema managingSceneSchema;
        private SceneModel managingScene;

        public SceneManagementModal(
            Translation translation,
            SceneRepository sceneRepository,
            ScreenshotService screenshotService,
            SceneSchemaBuilder sceneSchemaBuilder,
            SceneLoader sceneLoader,
            LoadOptionsService loadOptionsService)
        {
            this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
            this.screenshotService = screenshotService ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));
            this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _ = loadOptionsService ?? throw new ArgumentNullException(nameof(loadOptionsService));

            loadOptionsPane = new(translation, loadOptionsService);

            sceneFilenameLabel = new(string.Empty);

            loadSceneButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "fileLoadCommit"));
            loadSceneButton.ControlEvent += OnLoadSceneButtonPushed;

            cancelManagementButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
            cancelManagementButton.ControlEvent += OnCancelManagementButtonPushed;

            deleteSceneButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteButton"));
            deleteSceneButton.ControlEvent += OnDeleteSceneButtonPushed;

            overwriteSceneButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "overwriteButton"));
            overwriteSceneButton.ControlEvent += OnOverwriteSceneButtonPushed;

            loadOptionsToggle = new(new LocalizableGUIContent(translation, "sceneManagerModal", "loadOptionsToggle"));
            loadOptionsToggle.ControlEvent += OnLoadOptionsToggleChanged;

            deleteMessageLabel = new(string.Empty);

            deleteConfirmButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteFileCommit"));
            deleteConfirmButton.ControlEvent += OnDeleteConfirmButtonPushed;

            cancelDeleteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
            cancelDeleteButton.ControlEvent += OnCancelDeleteButtonPushed;
        }

        public override void Draw()
        {
            const float PaddingSize = 10f;

            GUILayout.BeginArea(new(PaddingSize, PaddingSize, WindowRect.width - PaddingSize * 2, WindowRect.height - PaddingSize * 2));

            if (deletingScene)
                DrawDeleteScene();
            else
                DrawManageScene();

            GUILayout.EndArea();

            void DrawManageScene()
            {
                GUILayout.BeginHorizontal();

                if (loadOptionsToggle.Value)
                {
                    var maxWidth = UIUtility.ScaledMinimum(ManageSceneWindowSize.Width);

                    GUILayout.BeginVertical(GUILayout.MaxWidth(maxWidth - 20));
                }
                else
                {
                    GUILayout.BeginVertical();
                }

                DrawThumbnail();

                GUILayout.FlexibleSpace();

                sceneFilenameLabel.Draw(messageStyle);

                GUILayout.FlexibleSpace();

                DrawButtons();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                loadOptionsToggle.Draw();

                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                if (loadOptionsToggle.Value)
                    loadOptionsPane.Draw();

                GUILayout.EndHorizontal();

                void DrawButtons()
                {
                    GUILayout.BeginHorizontal();

                    deleteSceneButton.Draw(GUILayout.ExpandWidth(false));
                    overwriteSceneButton.Draw(GUILayout.ExpandWidth(false));

                    GUILayout.FlexibleSpace();

                    loadSceneButton.Draw(GUILayout.ExpandWidth(false));
                    cancelManagementButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                    GUILayout.EndHorizontal();
                }
            }

            void DrawDeleteScene()
            {
                DrawThumbnail();

                GUILayout.FlexibleSpace();

                deleteMessageLabel.Draw(messageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                deleteConfirmButton.Draw(GUILayout.ExpandWidth(false));

                cancelDeleteButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                GUILayout.EndHorizontal();
            }

            void DrawThumbnail()
            {
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                var thumbnail = managingScene.Thumbnail;

                var (windowWidth, windowHeight) = ManageSceneWindowSize;

                var scaleWidth = (UIUtility.ScaledMinimum(windowWidth) - PaddingSize * 2) / thumbnail.width;
                var scaleHeight = UIUtility.ScaledMinimum(windowHeight) / thumbnail.height;

                var scale = Mathf.Min(scaleWidth, scaleHeight);

                var thumbnailWidth = Mathf.Min(thumbnail.width, thumbnail.width * scale);
                var thumbnailHeight = Mathf.Min(thumbnail.height, thumbnail.height * scale);

                GUILayout.Box(
                    thumbnail,
                    thumbnailStyle,
                    GUILayout.MaxWidth(thumbnailWidth),
                    GUILayout.MaxHeight(thumbnailHeight));

                var thumbnailRect = GUILayoutUtility.GetLastRect();
                var labelSize = sceneInfoLabelStyle.Style.CalcSize(characterCountContent);

                var labelRect = new Rect(
                    thumbnailRect.x + 10,
                    thumbnailRect.yMax - (labelSize.y + 10),
                    labelSize.x + 10,
                    labelSize.y + 2);

                GUI.Label(labelRect, characterCountContent, sceneInfoLabelStyle);

                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
        }

        public void ManageScene(SceneModel scene, SceneSchema schema)
        {
            managingScene = scene ?? throw new ArgumentNullException(nameof(scene));
            managingSceneSchema = schema ?? throw new ArgumentNullException(nameof(schema));

            deletingScene = false;

            loadOptionsPane.UpdateLoadOptionValidity(managingSceneSchema);
            sceneFilenameLabel.Text = scene.Name;

            var characterCount = managingSceneSchema?.Character?.Characters.Count ?? 0;

            characterCountContent.text = string.Format(
                characterCount is 1
                    ? translation["sceneManagerModal", "infoMaidSingular"]
                    : translation["sceneManagerModal", "infoMaidPlural"],
                characterCount);

            var (width, height) = loadOptionsToggle.Value ? LoadOptionsWindowSize : ManageSceneWindowSize;

            WindowRect = UIUtility.MiddlePosition(UIUtility.ScaledMinimum(width), UIUtility.ScaledMinimum(height));

            Modal.Show(this);
        }

        public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
        {
            base.OnScreenDimensionsChanged(newScreenDimensions);

            var (width, height) = deletingScene || !loadOptionsToggle.Value
                ? ManageSceneWindowSize
                : LoadOptionsWindowSize;

            WindowRect = WindowRect with
            {
                width = UIUtility.ScaledMinimum(width),
                height = UIUtility.ScaledMinimum(height),
            };
        }

        private void OnLoadSceneButtonPushed(object sender, EventArgs e)
        {
            sceneLoader.LoadScene(managingSceneSchema, loadOptionsPane.LoadOptions);

            CloseModal();
        }

        private void OnCancelManagementButtonPushed(object sender, EventArgs e) =>
            CloseModal();

        private void OnDeleteSceneButtonPushed(object sender, EventArgs e)
        {
            if (managingScene is null)
                return;

            deletingScene = true;
            deleteMessageLabel.Text =
                string.Format(translation["sceneManagerModal", "deleteFileConfirm"], managingScene.Name);
        }

        private void OnOverwriteSceneButtonPushed(object sender, EventArgs e)
        {
            if (managingScene is null)
                return;

            var sceneSchema = sceneSchemaBuilder.Build();

            screenshotService.TakeScreenshotToTexture(
                screenshot =>
                {
                    sceneRepository.Overwrite(sceneSchema, screenshot, managingScene);

                    CloseModal();
                },
                new());
        }

        private void OnLoadOptionsToggleChanged(object sender, EventArgs e)
        {
            var (width, height) = loadOptionsToggle.Value ? LoadOptionsWindowSize : ManageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = UIUtility.ScaledMinimum(width),
                height = UIUtility.ScaledMinimum(height),
            };
        }

        private void OnCancelDeleteButtonPushed(object sender, EventArgs e) =>
            deletingScene = false;

        private void OnDeleteConfirmButtonPushed(object sender, EventArgs e)
        {
            if (!deletingScene)
                return;

            sceneRepository.Delete(managingScene);

            CloseModal();
        }

        private void CloseModal()
        {
            deletingScene = false;
            Modal.Close();
        }

        private class LoadOptionsPane
        {
            private readonly Translation translation;
            private readonly KeyedTree<string, bool> loadOptionValidityTree;
            private readonly KeyedTree<string, Toggle> loadOptionsToggles;
            private readonly Dictionary<int, LazyStyle> loadOptionToggleStyles = [];

            private Vector2 loadOptionsScrollPosition;

            public LoadOptionsPane(Translation translation, LoadOptionsService loadOptionsService)
            {
                this.translation = translation ?? throw new ArgumentNullException(nameof(translation));

                LoadOptions = loadOptionsService.CreateLoadOptions();
                LoadOptions.AddedOption += OnLoadOptionAdded;
                LoadOptions.RemovedOption += OnLoadOptionRemoved;

                loadOptionValidityTree = new(
                    new(
                        "characters",
                        true,
                        new KeyedTree<string, bool>.Node("byID", true)),
                    new("message", true),
                    new("camera", true),
                    new("lights", true),
                    new(
                        "effects",
                        true,
                        new("bloom", true),
                        new("depthOfField", true),
                        new("vignette", true),
                        new("fog", true),
                        new("sepiaTone", true),
                        new("blur", true)),
                    new("background", true),
                    new("props", true));

                loadOptionsToggles = new([.. LoadOptions.Select(CreateLoadOptionToggles)]);
            }

            public ILoadOptions LoadOptions { get; }

            // TODO: Add a Valid property in LoadOption?
            public void UpdateLoadOptionValidity(SceneSchema scene)
            {
                _ = scene ?? throw new ArgumentNullException(nameof(scene));

                loadOptionValidityTree["characters"].Value = scene.Character is not null;
                loadOptionValidityTree["characters"]["byID"].Value = scene.Character?.Version >= 2;
                loadOptionValidityTree["message"].Value = scene.MessageWindow is not null;
                loadOptionValidityTree["camera"].Value = scene.Camera is not null;
                loadOptionValidityTree["lights"].Value = scene.Lights is not null;
                loadOptionValidityTree["effects"].Value = scene.Effects is not null;
                loadOptionValidityTree["effects"]["bloom"].Value = true;
                loadOptionValidityTree["effects"]["depthOfField"].Value = true;
                loadOptionValidityTree["effects"]["vignette"].Value = true;
                loadOptionValidityTree["effects"]["fog"].Value = true;
                loadOptionValidityTree["effects"]["sepiaTone"].Value = true;
                loadOptionValidityTree["effects"]["blur"].Value = true;
                loadOptionValidityTree["background"].Value = scene.Background is not null;
                loadOptionValidityTree["props"].Value = scene.Props is not null;
            }

            public void Draw()
            {
                GUILayout.BeginVertical();

                loadOptionsScrollPosition = GUILayout.BeginScrollView(loadOptionsScrollPosition);

                foreach (var loadOption in LoadOptions)
                {
                    loadOptionValidityTree.TryGetNode(loadOption.Tag, out var validityNode);
                    loadOptionsToggles.TryGetNode(loadOption.Tag, out var loadOptionToggle);
                    DrawLoadOption(loadOption, loadOptionToggle, validityNode);

                    UIUtility.DrawBlackLine();
                }

                GUI.enabled = true;

                GUILayout.EndScrollView();

                GUILayout.EndVertical();

                void DrawLoadOption(LoadOption loadOption, KeyedTree<string, Toggle>.Node toggleNode, KeyedTree<string, bool>.Node validityNode, int depth = 0)
                {
                    GUI.enabled = validityNode?.Value ?? true;

                    if (depth is 0)
                    {
                        toggleNode.Value.Draw();
                    }
                    else
                    {
                        if (!loadOptionToggleStyles.TryGetValue(depth, out var style))
                            loadOptionToggleStyles[depth] = style = new(
                                StyleSheet.TextSize,
                                () => new(GUI.skin.toggle)
                                {
                                    margin = { left = UIUtility.Scaled(15 * depth) },
                                },
                                style => style.margin.left = UIUtility.Scaled(15 * depth));

                        toggleNode.Value.Draw(style);
                    }

                    if (!loadOption.Enabled)
                        return;

                    foreach (var child in loadOption)
                    {
                        KeyedTree<string, bool>.Node loadOptionValidity = null;
                        validityNode?.TryGetNode(child.Tag, out loadOptionValidity);
                        toggleNode.TryGetNode(child.Tag, out var loadOptionToggle);

                        DrawLoadOption(child, loadOptionToggle, loadOptionValidity, depth + 1);
                    }
                }
            }

            private void OnLoadOptionAdded(object sender, LoadOptionsChangedEventArgs e)
            {
                var loadOption = e.LoadOption;

                loadOptionsToggles[loadOption.Tag] = CreateLoadOptionToggles(loadOption);
            }

            private void OnLoadOptionRemoved(object sender, LoadOptionsChangedEventArgs e) =>
                loadOptionsToggles.RemoveNode(e.LoadOption.Tag);

            private KeyedTree<string, Toggle>.Node CreateLoadOptionToggles(LoadOption option)
            {
                const string LoadOptionTableKey = "baseLoadOptions";

                var content = translation.ContainsTranslation(LoadOptionTableKey, option.Path)
                    ? new LocalizableGUIContent(translation, LoadOptionTableKey, option.Path)
                    : new GUIContent(option.Tag);

                var toggle = new Toggle(content, option.Enabled);

                toggle.ControlEvent += (_, _) =>
                    option.Enabled = toggle.Value;

                return new(option.Tag, toggle, [.. option.Select(CreateLoadOptionToggles)]);
            }
        }
    }

    private class CategoryManagementModal : BaseWindow
    {
        private static readonly WindowSize WindowSize = (450, 200);

        private readonly Translation translation;
        private readonly SceneRepository sceneRepository;
        private readonly Label messageLabel;
        private readonly Button deleteButton;
        private readonly Button cancelButton;
        private readonly Button createCategoryButton;
        private readonly Header categoryNameHeader;
        private readonly TextField categoryNameTextfield;

        private readonly LazyStyle messageStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        private bool addingCategory;
        private string managingCategory = string.Empty;

        public CategoryManagementModal(Translation translation, SceneRepository sceneRepository)
        {
            this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));

            messageLabel = new(string.Empty);

            cancelButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonPushed;

            deleteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteButton"));
            deleteButton.ControlEvent += OnDeleteButtonPushed;

            categoryNameHeader = new(new LocalizableGUIContent(translation, "sceneManagerModal", "addDirectoryHeader"));

            categoryNameTextfield = new()
            {
                PlaceholderContent = new LocalizableGUIContent(translation, "sceneManagerModal", "newDirectoryNamePlaceholder"),
            };

            createCategoryButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "addDirectoryButton"));
            createCategoryButton.ControlEvent += OnCreateCategoryButtonPushed;
        }

        public override void Draw()
        {
            const float PaddingSize = 10f;

            GUILayout.BeginArea(new(PaddingSize, PaddingSize, WindowRect.width - PaddingSize * 2, WindowRect.height - PaddingSize * 2));

            if (addingCategory)
                DrawAddCategory();
            else
                DrawDeleteCategory();

            GUILayout.EndArea();

            void DrawDeleteCategory()
            {
                messageLabel.Draw(messageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                deleteButton.Draw(GUILayout.ExpandWidth(false));

                cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                GUILayout.EndHorizontal();
            }

            void DrawAddCategory()
            {
                GUILayout.FlexibleSpace();

                categoryNameHeader.Draw();

                GUILayout.Space(UIUtility.Scaled(5));

                categoryNameTextfield.Draw(GUILayout.Height(UIUtility.Scaled(StyleSheet.TextSize) + 12));

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                createCategoryButton.Draw(GUILayout.ExpandWidth(false));
                cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                GUILayout.EndHorizontal();
            }
        }

        public void DeleteCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

            addingCategory = false;
            managingCategory = category;

            messageLabel.Text =
                string.Format(translation["sceneManagerModal", "deleteDirectoryConfirm"], managingCategory);

            WindowRect = UIUtility.MiddlePosition(UIUtility.ScaledMinimum(WindowSize.Width), UIUtility.ScaledMinimum(WindowSize.Height));

            Modal.Show(this);
        }

        public void AddCategory()
        {
            addingCategory = true;

            categoryNameTextfield.Value = GetUniqueCategoryName();

            WindowRect = UIUtility.MiddlePosition(UIUtility.ScaledMinimum(WindowSize.Width), UIUtility.ScaledMinimum(WindowSize.Height));

            Modal.Show(this);
        }

        public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
        {
            base.OnScreenDimensionsChanged(newScreenDimensions);

            WindowRect = WindowRect with
            {
                width = UIUtility.ScaledMinimum(WindowSize.Width),
                height = UIUtility.ScaledMinimum(WindowSize.Height),
            };
        }

        private void OnCancelButtonPushed(object sender, EventArgs e) =>
            CloseModal();

        private void OnDeleteButtonPushed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(managingCategory))
                return;

            sceneRepository.DeleteCategory(managingCategory);

            CloseModal();
        }

        private void OnCreateCategoryButtonPushed(object sender, EventArgs e)
        {
            sceneRepository.AddCategory(GetUniqueCategoryName(categoryNameTextfield.Value));

            CloseModal();
        }

        private void CloseModal()
        {
            addingCategory = false;
            Modal.Close();
        }

        private string GetUniqueCategoryName(string startingName = "")
        {
            const string defaultName = "Scenes";

            if (string.IsNullOrEmpty(startingName))
                startingName = defaultName;

            var newCategoryName = startingName;
            var categorySet = new HashSet<string>(sceneRepository.Categories);
            var index = 1;

            while (categorySet.Contains(newCategoryName))
                newCategoryName = $"{startingName} ({index++})";

            return newCategoryName;
        }
    }

    private class ErrorModal : BaseWindow
    {
        private static readonly WindowSize WindowSize = (450, 200);

        private readonly Button okButton;
        private readonly Label errorLabel;
        private readonly LazyStyle messageStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        public ErrorModal(Translation translation)
        {
            _ = translation ?? throw new ArgumentNullException(nameof(translation));

            okButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "okButton"));
            okButton.ControlEvent += OnOKButtonPushed;

            errorLabel = new(string.Empty);
        }

        public override void Draw()
        {
            const float PaddingSize = 10f;

            GUILayout.BeginArea(new(PaddingSize, PaddingSize, WindowRect.width - PaddingSize * 2, WindowRect.height - PaddingSize * 2));

            GUILayout.BeginVertical();

            GUILayout.FlexibleSpace();

            errorLabel.Draw(messageStyle);

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            okButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        public void ShowError(string message)
        {
            errorLabel.Text = message;

            WindowRect = UIUtility.MiddlePosition(UIUtility.ScaledMinimum(WindowSize.Width), UIUtility.ScaledMinimum(WindowSize.Height));

            Modal.Show(this);
        }

        private void OnOKButtonPushed(object sender, EventArgs e) =>
            Modal.Close();
    }
}
