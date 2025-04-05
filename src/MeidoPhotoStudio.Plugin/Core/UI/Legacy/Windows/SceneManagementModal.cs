using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using WindowSize = (float Width, float Height);

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SceneManagementModal : BaseWindow
{
    private const float PaddingSize = 10f;

    private readonly NoneMode noneMode;
    private readonly ManageSceneMode manageSceneMode;
    private readonly DeleteCategoryMode deleteCategoryMode;

    public SceneManagementModal(
        Translation translation,
        SceneRepository sceneRepository,
        ScreenshotService screenshotService,
        SceneSchemaBuilder sceneSchemaBuilder,
        ISceneSerializer sceneSerializer,
        SceneLoader sceneLoader)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        _ = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
        _ = screenshotService ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));
        _ = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
        _ = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
        _ = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));

        noneMode = new(this);
        manageSceneMode = new(this, translation, sceneSerializer, sceneSchemaBuilder, screenshotService, sceneLoader, sceneRepository);
        deleteCategoryMode = new(this, translation, sceneRepository);

        CurrentMode = noneMode;
    }

    private Mode CurrentMode { get; set; }

    public override void Draw()
    {
        GUILayout.BeginArea(new(PaddingSize, PaddingSize, WindowRect.width - PaddingSize * 2, WindowRect.height - PaddingSize * 2));

        CurrentMode.Draw();

        GUILayout.EndArea();
    }

    public void ManageScene(SceneModel scene) =>
        manageSceneMode.ManageScene(scene);

    public void DeleteCategory(string category) =>
        deleteCategoryMode.DeleteCategory(category);

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        CurrentMode.OnScreenDimensionsChanged();
    }

    private abstract class Mode(SceneManagementModal sceneManagementModal)
    {
        protected readonly LazyStyle messageStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        protected readonly SceneManagementModal sceneManagementModal =
            sceneManagementModal ?? throw new ArgumentNullException(nameof(sceneManagementModal));

        protected Rect WindowRect
        {
            get => sceneManagementModal.WindowRect;
            set => sceneManagementModal.WindowRect = value;
        }

        protected Mode CurrentMode
        {
            get => sceneManagementModal.CurrentMode;
            set
            {
                sceneManagementModal.CurrentMode = value;

                CurrentMode.OnModeEnter();

                Modal.Show(sceneManagementModal);
            }
        }

        public abstract void Draw();

        public abstract void OnScreenDimensionsChanged();

        protected static Rect MiddlePosition(float width, float height) =>
            new(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);

        protected static int ScaledMinimum(float value) =>
            Mathf.Min(UIUtility.Scaled(Mathf.RoundToInt(value)), (int)value);

        protected virtual void OnModeEnter()
        {
        }

        protected void CloseModal()
        {
            Modal.Close();

            sceneManagementModal.CurrentMode = sceneManagementModal.noneMode;
        }
    }

    private sealed class NoneMode(SceneManagementModal sceneManagementModal) : Mode(sceneManagementModal)
    {
        public override void Draw() =>
            GUILayout.Label("You're not supposed to see this");

        public override void OnScreenDimensionsChanged()
        {
        }
    }

    private class ManageSceneMode : Mode
    {
        private static readonly Texture2D InfoHighlight = UIUtility.CreateTexture(2, 2, new(0f, 0f, 0f, 0.8f));

        private readonly Translation translation;
        private readonly ISceneSerializer sceneSerializer;
        private readonly SceneSchemaBuilder sceneSchemaBuilder;
        private readonly ScreenshotService screenshotService;
        private readonly SceneLoader sceneLoader;
        private readonly SceneRepository sceneRepository;
        private readonly DeleteSceneMode deleteSceneMode;
        private readonly ErrorMode errorMode;
        private readonly WindowSize manageSceneWindowSize = (540, 415);
        private readonly WindowSize loadOptionsWindowSize = (800, 415);
        private readonly LoadOptions loadOptions;

        private readonly LazyStyle infoLabelStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { background = InfoHighlight },
            });

        private readonly LazyStyle paddedToggleStyle = new(
            StyleSheet.TextSize,
            static () => new(GUI.skin.toggle)
            {
                margin = { left = UIUtility.Scaled(15) },
            },
            static style => style.margin.left = UIUtility.Scaled(15));

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

        private readonly Button loadButton;
        private readonly Button cancelButton;
        private readonly Button deleteButton;
        private readonly Button overwriteButton;
        private readonly Label sceneFilenameLabel;
        private readonly KeyedTree<Toggle> loadOptionsToggles;
        private readonly Toggle loadOptionsToggle;
        private readonly GUIContent characterCountContent = new();

        private Vector2 loadOptionsScrollPosition;
        private SceneSchema managingSceneSchema;
        private SceneModel managingScene;

        public ManageSceneMode(
            SceneManagementModal sceneManagementModal,
            Translation translation,
            ISceneSerializer sceneSerializer,
            SceneSchemaBuilder sceneSchemaBuilder,
            ScreenshotService screenshotService,
            SceneLoader sceneLoader,
            SceneRepository sceneRepository)
            : base(sceneManagementModal)
        {
            this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
            this.sceneSerializer = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
            this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
            this.screenshotService = screenshotService
                ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));

            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));

            loadOptions = LoadOptions.All;

            sceneFilenameLabel = new(string.Empty);

            loadButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "fileLoadCommit"));
            loadButton.ControlEvent += OnLoadButtonPushed;

            cancelButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonPushed;

            deleteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteButton"));
            deleteButton.ControlEvent += OnDeleteButtonPushed;

            overwriteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "overwriteButton"));
            overwriteButton.ControlEvent += OnOverwriteButtonPushed;

            loadOptionsToggle = new(new LocalizableGUIContent(translation, "sceneManagerModal", "loadOptionsToggle"));
            loadOptionsToggle.ControlEvent += OnLoadOptionsToggleChanged;

            loadOptionsToggles = new(
                CreateLoadOptionToggle(
                    "characters",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadCharactersToggle"),
                    null,
                    CreateLoadOptionToggle(
                        "byID",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadCharactersByIDToggle"),
                        loadOptions["characters"])),
                CreateLoadOptionToggle(
                    "message",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadMessageToggle")),
                CreateLoadOptionToggle(
                    "camera",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadCameraToggle")),
                CreateLoadOptionToggle(
                    "lights",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadLightsToggle")),
                CreateLoadOptionToggle(
                    "effects",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadEffectsToggle"),
                    null,
                    CreateLoadOptionToggle(
                        "bloom",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadBloomToggle"),
                        loadOptions["effects"]),
                    CreateLoadOptionToggle(
                        "depthOfField",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadDepthOfFieldToggle"),
                        loadOptions["effects"]),
                    CreateLoadOptionToggle(
                        "vignette",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadVignetteToggle"),
                        loadOptions["effects"]),
                    CreateLoadOptionToggle(
                        "fog",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadFogToggle"),
                        loadOptions["effects"]),
                    CreateLoadOptionToggle(
                        "sepiaTone",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadSepiaToneToggle"),
                        loadOptions["effects"]),
                    CreateLoadOptionToggle(
                        "blur",
                        new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadBlurToggle"),
                        loadOptions["effects"])),
                CreateLoadOptionToggle(
                    "background",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadBackgroundToggle")),
                CreateLoadOptionToggle(
                    "props",
                    new LocalizableGUIContent(translation, "sceneManagerModalLoadOptions", "loadPropsToggle")));

            deleteSceneMode = new(sceneManagementModal, this, translation, sceneRepository);
            errorMode = new(sceneManagementModal, translation);

            KeyedTree<Toggle>.Node CreateLoadOptionToggle(
                string key,
                GUIContent content,
                LoadOption option = null,
                params KeyedTree<Toggle>.Node[] children)
            {
                var toggle = new Toggle(content, option is null ? loadOptions[key].Enabled : option[key].Enabled);

                toggle.ControlEvent += (_, _) =>
                    (option is null ? loadOptions[key] : option[key]).Enabled = toggle.Value;

                return new(key, toggle, children ?? []);
            }
        }

        public override void Draw()
        {
            GUILayout.BeginHorizontal();

            DrawManageScene();

            if (loadOptionsToggle.Value)
                DrawLoadOptions();

            GUILayout.EndHorizontal();

            void DrawManageScene()
            {
                if (loadOptionsToggle.Value)
                {
                    var maxWidth = ScaledMinimum(manageSceneWindowSize.Width);

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

                void DrawButtons()
                {
                    GUILayout.BeginHorizontal();

                    deleteButton.Draw(GUILayout.ExpandWidth(false));
                    overwriteButton.Draw(GUILayout.ExpandWidth(false));

                    GUILayout.FlexibleSpace();

                    loadButton.Draw(GUILayout.ExpandWidth(false));
                    cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                    GUILayout.EndHorizontal();
                }
            }

            void DrawLoadOptions()
            {
                GUILayout.BeginVertical();

                loadOptionsScrollPosition = GUILayout.BeginScrollView(loadOptionsScrollPosition);

                GUI.enabled = managingSceneSchema.Character is not null;

                loadOptionsToggles["characters"].Value.Draw();

                if (loadOptions["characters"].Enabled)
                {
                    GUI.enabled = managingSceneSchema.Character?.Version >= 2;

                    loadOptionsToggles["characters"]["byID"].Value.Draw(paddedToggleStyle);
                }

                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.MessageWindow is not null;

                loadOptionsToggles["message"].Value.Draw();
                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.Camera is not null;

                loadOptionsToggles["camera"].Value.Draw();
                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.Lights is not null;

                loadOptionsToggles["lights"].Value.Draw();
                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.Effects is not null;

                loadOptionsToggles["effects"].Value.Draw();

                if (loadOptions["effects"].Enabled)
                {
                    loadOptionsToggles["effects"]["bloom"].Value.Draw(paddedToggleStyle);
                    loadOptionsToggles["effects"]["depthOfField"].Value.Draw(paddedToggleStyle);
                    loadOptionsToggles["effects"]["vignette"].Value.Draw(paddedToggleStyle);
                    loadOptionsToggles["effects"]["fog"].Value.Draw(paddedToggleStyle);
                    loadOptionsToggles["effects"]["sepiaTone"].Value.Draw(paddedToggleStyle);
                    loadOptionsToggles["effects"]["blur"].Value.Draw(paddedToggleStyle);
                }

                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.Background is not null;

                loadOptionsToggles["background"].Value.Draw();
                UIUtility.DrawBlackLine();

                GUI.enabled = managingSceneSchema.Props is not null;

                loadOptionsToggles["props"].Value.Draw();
                UIUtility.DrawBlackLine();

                GUI.enabled = true;

                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }
        }

        public override void OnScreenDimensionsChanged()
        {
            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = Mathf.Min(UIUtility.Scaled(width), width),
                height = Mathf.Min(UIUtility.Scaled(height), height),
            };
        }

        public void ManageScene(SceneModel scene)
        {
            _ = scene ?? throw new ArgumentNullException(nameof(scene));

            SceneSchema schema;

            try
            {
                using var fileStream = File.OpenRead(scene.Filename);

                SeekToEndOfPNG(fileStream);

                schema = sceneSerializer.DeserializeScene(fileStream);

                if (schema is null)
                {
                    errorMode.ShowError(
                        string.Format(translation["sceneManagerModal", "sceneLoadErrorMessage"], scene.Name));

                    return;
                }
            }
            catch
            {
                throw;
            }

            managingScene = scene;
            managingSceneSchema = schema;

            sceneFilenameLabel.Text = scene.Name;

            var characterCount = managingSceneSchema?.Character?.Characters.Count ?? 0;

            characterCountContent.text = string.Format(
                characterCount is 1
                    ? translation["sceneManagerModal", "infoMaidSingular"]
                    : translation["sceneManagerModal", "infoMaidPlural"],
                characterCount);

            CurrentMode = this;

            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = MiddlePosition(ScaledMinimum(width), ScaledMinimum(height));

            static bool SeekToEndOfPNG(Stream stream)
            {
                var buffer = new byte[8];

                var pngHeader = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

                stream.Read(buffer, 0, 8);

                if (!buffer.SequenceEqual(pngHeader))
                    return false;

                var pngEnd = Encoding.ASCII.GetBytes("IEND");

                buffer = new byte[4];

                do
                {
                    stream.Read(buffer, 0, 4);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(buffer);

                    var length = BitConverter.ToUInt32(buffer, 0);

                    stream.Read(buffer, 0, 4);
                    stream.Seek(length + 4L, SeekOrigin.Current);
                }
                while (!buffer.SequenceEqual(pngEnd));

                return true;
            }
        }

        protected override void OnModeEnter()
        {
            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = Mathf.Min(UIUtility.Scaled(width), width),
                height = Mathf.Min(UIUtility.Scaled(height), height),
            };
        }

        private void DrawThumbnail()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            var thumbnail = managingScene.Thumbnail;

            var (windowWidth, windowHeight) = manageSceneWindowSize;

            var scaleWidth = (ScaledMinimum(windowWidth) - PaddingSize * 2) / thumbnail.width;
            var scaleHeight = ScaledMinimum(windowHeight) / thumbnail.height;

            var scale = Mathf.Min(scaleWidth, scaleHeight);

            var thumbnailWidth = Mathf.Min(thumbnail.width, thumbnail.width * scale);
            var thumbnailHeight = Mathf.Min(thumbnail.height, thumbnail.height * scale);

            GUILayout.Box(
                thumbnail,
                thumbnailStyle,
                GUILayout.MaxWidth(thumbnailWidth),
                GUILayout.MaxHeight(thumbnailHeight));

            var thumbnailRect = GUILayoutUtility.GetLastRect();
            var labelSize = infoLabelStyle.Style.CalcSize(characterCountContent);

            var labelRect = new Rect(
                thumbnailRect.x + 10,
                thumbnailRect.yMax - (labelSize.y + 10),
                labelSize.x + 10,
                labelSize.y + 2);

            GUI.Label(labelRect, characterCountContent, infoLabelStyle);

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void OnLoadButtonPushed(object sender, EventArgs e)
        {
            sceneLoader.LoadScene(managingSceneSchema, loadOptions);

            CloseModal();
        }

        private void OnCancelButtonPushed(object sender, EventArgs e) =>
            CloseModal();

        private void OnDeleteButtonPushed(object sender, EventArgs e)
        {
            if (managingScene is null)
                return;

            deleteSceneMode.DeleteScene();
        }

        private void OnOverwriteButtonPushed(object sender, EventArgs e)
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
            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = ScaledMinimum(width),
                height = ScaledMinimum(height),
            };
        }

        private class DeleteSceneMode : Mode
        {
            private readonly ManageSceneMode manageSceneMode;
            private readonly SceneRepository sceneRepository;
            private readonly Translation translation;
            private readonly Label messageLabel;
            private readonly Button deleteButton;
            private readonly Button cancelButton;

            public DeleteSceneMode(
                SceneManagementModal sceneManagementModal,
                ManageSceneMode manageSceneMode,
                Translation translation,
                SceneRepository sceneRepository)
                : base(sceneManagementModal)
            {
                this.manageSceneMode = manageSceneMode;
                this.sceneRepository = sceneRepository;
                this.translation = translation ?? throw new ArgumentNullException(nameof(translation));

                messageLabel = new(string.Empty);

                deleteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteFileCommit"));
                deleteButton.ControlEvent += OnDeleteButtonPushed;

                cancelButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
                cancelButton.ControlEvent += OnCancelButtonPushed;
            }

            public override void Draw()
            {
                manageSceneMode.DrawThumbnail();

                GUILayout.FlexibleSpace();

                messageLabel.Draw(messageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                deleteButton.Draw(GUILayout.ExpandWidth(false));

                cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                GUILayout.EndHorizontal();
            }

            public override void OnScreenDimensionsChanged()
            {
                var (width, height) = manageSceneMode.manageSceneWindowSize;

                WindowRect = WindowRect with
                {
                    width = ScaledMinimum(width),
                    height = ScaledMinimum(height),
                };
            }

            public void DeleteScene()
            {
                messageLabel.Text = string.Format(
                    translation["sceneManagerModal", "deleteFileConfirm"], manageSceneMode.managingScene.Name);

                CurrentMode = this;
            }

            protected override void OnModeEnter()
            {
                var (width, height) = manageSceneMode.manageSceneWindowSize;

                WindowRect = WindowRect with
                {
                    width = ScaledMinimum(width),
                    height = ScaledMinimum(height),
                };
            }

            private void OnDeleteButtonPushed(object sender, EventArgs e)
            {
                sceneRepository.Delete(manageSceneMode.managingScene);

                CloseModal();
            }

            private void OnCancelButtonPushed(object sender, EventArgs e) =>
                CurrentMode = manageSceneMode;
        }

        private class ErrorMode : Mode
        {
            private readonly WindowSize windowSize = (450, 200);
            private readonly Button okButton;
            private readonly Label errorLabel;

            public ErrorMode(SceneManagementModal sceneManagementModal, Translation translation)
                : base(sceneManagementModal)
            {
                _ = translation ?? throw new ArgumentNullException(nameof(translation));

                okButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "okButton"));
                okButton.ControlEvent += OnOKButtonPushed;

                errorLabel = new(string.Empty);
            }

            public override void Draw()
            {
                GUILayout.BeginVertical();

                GUILayout.FlexibleSpace();

                errorLabel.Draw(messageStyle);

                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                okButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

                GUILayout.EndHorizontal();
            }

            public override void OnScreenDimensionsChanged() =>
                WindowRect = WindowRect with
                {
                    width = ScaledMinimum(windowSize.Width),
                    height = ScaledMinimum(windowSize.Height),
                };

            public void ShowError(string message)
            {
                errorLabel.Text = message;

                CurrentMode = this;
            }

            protected override void OnModeEnter() =>
                WindowRect = MiddlePosition(ScaledMinimum(windowSize.Width), ScaledMinimum(windowSize.Height));

            private void OnOKButtonPushed(object sender, EventArgs e) =>
                CloseModal();
        }

        private class KeyedTree<T>(params KeyedTree<T>.Node[] children)
        {
            private readonly Dictionary<string, Node> nodes = children.ToDictionary(static node => node.Key, static node => node);

            public Node this[string key] =>
                nodes[key];

            public class Node(string key, T value, params Node[] children)
            {
                private readonly Dictionary<string, Node> nodes = children.ToDictionary(static node => node.Key, static node => node);

                public string Key { get; } = key;

                public T Value { get; } = value;

                public Node this[string key] =>
                    nodes[key];
            }
        }
    }

    private class DeleteCategoryMode : Mode
    {
        private readonly WindowSize windowSize = (450, 200);
        private readonly SceneRepository sceneRepository;
        private readonly Translation translation;
        private readonly LocalizableGUIContent messageContent;
        private readonly Label messageLabel;
        private readonly Button deleteButton;
        private readonly Button cancelButton;

        private string managingCategory = string.Empty;

        public DeleteCategoryMode(
            SceneManagementModal sceneManagementModal, Translation translation, SceneRepository sceneRepository)
            : base(sceneManagementModal)
        {
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
            this.translation = translation ?? throw new ArgumentNullException(nameof(translation));

            messageContent = new LocalizableGUIContent(
                translation,
                "sceneManagerModal",
                "deleteDirectoryConfirm",
                translation => string.Format(translation, managingCategory));

            messageLabel = new(messageContent);

            cancelButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonPushed;

            deleteButton = new(new LocalizableGUIContent(translation, "sceneManagerModal", "deleteButton"));
            deleteButton.ControlEvent += OnDeleteButtonPushed;
        }

        public override void Draw()
        {
            messageLabel.Draw(messageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            deleteButton.Draw(GUILayout.ExpandWidth(false));

            cancelButton.Draw(GUILayout.MinWidth(UIUtility.Scaled(110)));

            GUILayout.EndHorizontal();
        }

        public override void OnScreenDimensionsChanged() =>
            WindowRect = WindowRect with
            {
                width = ScaledMinimum(windowSize.Width),
                height = ScaledMinimum(windowSize.Height),
            };

        public void DeleteCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

            if (!sceneRepository.ContainsCategory(category))
                throw new ArgumentException($"'{category}' does not exist.", nameof(category));

            managingCategory = category;

            messageContent.Reformat();

            CurrentMode = this;
        }

        protected override void OnModeEnter() =>
            WindowRect = MiddlePosition(ScaledMinimum(windowSize.Width), ScaledMinimum(windowSize.Height));

        private void OnCancelButtonPushed(object sender, EventArgs e) =>
            CloseModal();

        private void OnDeleteButtonPushed(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(managingCategory))
                return;

            sceneRepository.DeleteCategory(managingCategory);

            CloseModal();
        }
    }
}
