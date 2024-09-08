using MeidoPhotoStudio.Plugin.Core.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

using WindowSize = (float Width, float Height);

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SceneManagementModal : BaseWindow
{
    private const int FontSize = 13;
    private const float PaddingSize = 10f;

    private readonly NoneMode noneMode;
    private readonly ManageSceneMode manageSceneMode;
    private readonly DeleteCategoryMode deleteCategoryMode;

    public SceneManagementModal(
        SceneRepository sceneRepository,
        ScreenshotService screenshotService,
        SceneSchemaBuilder sceneSchemaBuilder,
        ISceneSerializer sceneSerializer,
        SceneLoader sceneLoader)
    {
        noneMode = new(this);
        manageSceneMode = new(this, sceneSerializer, sceneSchemaBuilder, screenshotService, sceneLoader, sceneRepository);
        deleteCategoryMode = new(this, sceneRepository);

        CurrentMode = noneMode;
    }

    private Mode CurrentMode { get; set; }

    public override void Draw()
    {
        GUILayout.BeginArea(new(PaddingSize, PaddingSize, WindowRect.width - PaddingSize * 2, WindowRect.height - PaddingSize * 2));

        CurrentMode.Draw();

        GUILayout.EndArea();
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        CurrentMode.OnScreenDimensionsChanged();
    }

    public void ManageScene(SceneModel scene) =>
        manageSceneMode.ManageScene(scene);

    public void DeleteCategory(string category) =>
        deleteCategoryMode.DeleteCategory(category);

    protected override void ReloadTranslation()
    {
        manageSceneMode.OnReloadTranslation();
        deleteCategoryMode.OnReloadTranslation();
    }

    private abstract class Mode(SceneManagementModal sceneManagementModal)
    {
        protected readonly LazyStyle messageStyle = new(
            FontSize,
            () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
            });

        protected readonly SceneManagementModal sceneManagementModal = sceneManagementModal;

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

        public abstract void OnReloadTranslation();

        public abstract void OnScreenDimensionsChanged();

        protected static Rect MiddlePosition(float width, float height) =>
            new(Screen.width / 2f - width / 2f, Screen.height / 2f - height / 2f, width, height);

        protected static int ScaledMinimum(float value) =>
            Mathf.Min(Utility.GetPix(Mathf.RoundToInt(value)), (int)value);

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

        public override void OnReloadTranslation()
        {
        }

        public override void OnScreenDimensionsChanged()
        {
        }
    }

    private class ManageSceneMode : Mode
    {
        private static readonly Texture2D InfoHighlight = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.8f));

        private readonly ISceneSerializer sceneSerializer;
        private readonly SceneSchemaBuilder sceneSchemaBuilder;
        private readonly ScreenshotService screenshotService;
        private readonly SceneLoader sceneLoader;
        private readonly SceneRepository sceneRepository;
        private readonly DeleteSceneMode deleteSceneMode;
        private readonly ErrorMode errorMode;
        private readonly WindowSize manageSceneWindowSize = (540, 415);
        private readonly WindowSize loadOptionsWindowSize = (800, 415);

        private readonly LazyStyle infoLabelStyle = new(
            FontSize,
            () => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { background = InfoHighlight },
            });

        private readonly LazyStyle paddedToggleStyle = new(
            FontSize,
            () => new(GUI.skin.toggle)
            {
                margin = { left = 10 },
            });

        private readonly LazyStyle thumbnailStyle = new(
            0,
            () => new(GUI.skin.box)
            {
                margin = new(0, 0, 0, 0),
                border = new(0, 0, 0, 0),
                normal = { background = MpsGui.White },
                stretchWidth = false,
                stretchHeight = false,
            });

        private readonly Button loadButton;
        private readonly Button cancelButton;
        private readonly Button deleteButton;
        private readonly Button overwriteButton;
        private readonly Label sceneFilenameLabel;
        private readonly Toggle loadOptionsToggle;
        private readonly Toggle characterLoadOptionToggle;
        private readonly Toggle characterIDLoadOptionToggle;
        private readonly Toggle messageWindowLoadOptionToggle;
        private readonly Toggle lightsLoadOptionToggle;
        private readonly Toggle effectsLoadOptionToggle;
        private readonly Toggle bloomLoadOptionToggle;
        private readonly Toggle depthOfFieldLoadOptionToggle;
        private readonly Toggle vignetteLoadOptionToggle;
        private readonly Toggle fogLoadOptionToggle;
        private readonly Toggle sepiaToneLoadOptionToggle;
        private readonly Toggle blurLoadOptionToggle;
        private readonly Toggle backgroundLoadOptionToggle;
        private readonly Toggle propsLoadOptionToggle;
        private readonly Toggle cameraLoadOptionToggle;

        private Vector2 loadOptionsScrollPosition;
        private SceneSchema managingSceneSchema;
        private SceneModel managingScene;
        private GUIContent characterCount;

        public ManageSceneMode(
            SceneManagementModal sceneManagementModal,
            ISceneSerializer sceneSerializer,
            SceneSchemaBuilder sceneSchemaBuilder,
            ScreenshotService screenshotService,
            SceneLoader sceneLoader,
            SceneRepository sceneRepository)
            : base(sceneManagementModal)
        {
            this.sceneSerializer = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
            this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
            this.screenshotService = screenshotService
                ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));

            this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));

            sceneFilenameLabel = new(string.Empty);

            loadButton = new(Translation.Get("sceneManagerModal", "fileLoadCommit"));
            loadButton.ControlEvent += OnLoadButtonPushed;

            cancelButton = new(Translation.Get("sceneManagerModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonPushed;

            deleteButton = new(Translation.Get("sceneManagerModal", "deleteButton"));
            deleteButton.ControlEvent += OnDeleteButtonPushed;

            overwriteButton = new(Translation.Get("sceneManagerModal", "overwriteButton"));
            overwriteButton.ControlEvent += OnOverwriteButtonPushed;

            loadOptionsToggle = new(Translation.Get("sceneManagerModal", "loadOptionsToggle"), false);
            loadOptionsToggle.ControlEvent += OnLoadOptionsToggleChanged;

            characterLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadCharactersToggle"), true);
            characterIDLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadCharactersByIDToggle"));

            messageWindowLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadMessageToggle"), true);

            cameraLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadCameraToggle"), true);

            lightsLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadLightsToggle"), true);

            effectsLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadEffectsToggle"), true);
            bloomLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadBloomToggle"), true);
            depthOfFieldLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadDepthOfFieldToggle"), true);
            vignetteLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadVignetteToggle"), true);
            fogLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadFogToggle"), true);
            sepiaToneLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadSepiaToneToggle"), true);
            blurLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadBlurToggle"), true);

            backgroundLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadBackgroundToggle"), true);

            propsLoadOptionToggle = new(Translation.Get("sceneManagerModalLoadOptions", "loadPropsToggle"), true);

            deleteSceneMode = new(sceneManagementModal, this, sceneRepository);
            errorMode = new(sceneManagementModal);
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
                    cancelButton.Draw(GUILayout.MinWidth(Utility.GetPix(110)));

                    GUILayout.EndHorizontal();
                }
            }

            void DrawLoadOptions()
            {
                GUILayout.BeginVertical();

                loadOptionsScrollPosition = GUILayout.BeginScrollView(loadOptionsScrollPosition);

                GUI.enabled = managingSceneSchema.Character is not null;

                characterLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                if (characterLoadOptionToggle.Value)
                {
                    GUI.enabled = managingSceneSchema.Character?.Version >= 2;

                    characterIDLoadOptionToggle.Draw(paddedToggleStyle);
                }

                GUI.enabled = managingSceneSchema.MessageWindow is not null;

                messageWindowLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                GUI.enabled = managingSceneSchema.Camera is not null;

                cameraLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                GUI.enabled = managingSceneSchema.Lights is not null;

                lightsLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                GUI.enabled = managingSceneSchema.Effects is not null;

                effectsLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                if (effectsLoadOptionToggle.Value)
                {
                    bloomLoadOptionToggle.Draw(paddedToggleStyle);
                    depthOfFieldLoadOptionToggle.Draw(paddedToggleStyle);
                    vignetteLoadOptionToggle.Draw(paddedToggleStyle);
                    fogLoadOptionToggle.Draw(paddedToggleStyle);
                    sepiaToneLoadOptionToggle.Draw(paddedToggleStyle);
                    blurLoadOptionToggle.Draw(paddedToggleStyle);
                }

                GUI.enabled = managingSceneSchema.Background is not null;

                backgroundLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                GUI.enabled = managingSceneSchema.Props is not null;

                propsLoadOptionToggle.Draw();
                MpsGui.WhiteLine();

                GUI.enabled = true;

                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }
        }

        public override void OnReloadTranslation()
        {
            loadButton.Label = Translation.Get("sceneManagerModal", "fileLoadCommit");
            cancelButton.Label = Translation.Get("sceneManagerModal", "cancelButton");
            deleteButton.Label = Translation.Get("sceneManagerModal", "deleteButton");
            overwriteButton.Label = Translation.Get("sceneManagerModal", "overwriteButton");
            loadOptionsToggle.Label = Translation.Get("sceneManagerModal", "loadOptionsToggle");
            characterLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadCharactersToggle");
            characterIDLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadCharactersByIDToggle");
            messageWindowLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadMessageToggle");
            cameraLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadCameraToggle");
            lightsLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadLightsToggle");
            effectsLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadEffectsToggle");
            bloomLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadBloomToggle");
            depthOfFieldLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadDepthOfFieldToggle");
            vignetteLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadVignetteToggle");
            fogLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadFogToggle");
            sepiaToneLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadSepiaToneToggle");
            blurLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadBlurToggle");
            backgroundLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadBackgroundToggle");
            propsLoadOptionToggle.Label = Translation.Get("sceneManagerModalLoadOptions", "loadPropsToggle");

            deleteSceneMode.OnReloadTranslation();
            errorMode.OnReloadTranslation();
        }

        public override void OnScreenDimensionsChanged()
        {
            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = Mathf.Min(Utility.GetPix(width), width),
                height = Mathf.Min(Utility.GetPix(height), height),
            };
        }

        public void ManageScene(SceneModel scene)
        {
            _ = scene ?? throw new ArgumentNullException(nameof(scene));

            SceneSchema schema;

            try
            {
                using var fileStream = File.OpenRead(scene.Filename);

                Utility.SeekPngEnd(fileStream);

                schema = sceneSerializer.DeserializeScene(fileStream);

                if (schema is null)
                {
                    errorMode.ShowError(
                        string.Format(Translation.Get("sceneManagerModal", "sceneLoadErrorMessage"), scene.Name));

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

            this.characterCount = new(string.Format(
                characterCount is 1
                    ? Translation.Get("sceneManagerModal", "infoMaidSingular")
                    : Translation.Get("sceneManagerModal", "infoMaidPlural"),
                characterCount));

            CurrentMode = this;

            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = MiddlePosition(ScaledMinimum(width), ScaledMinimum(height));
        }

        protected override void OnModeEnter()
        {
            var (width, height) = loadOptionsToggle.Value ? loadOptionsWindowSize : manageSceneWindowSize;

            WindowRect = WindowRect with
            {
                width = Mathf.Min(Utility.GetPix(width), width),
                height = Mathf.Min(Utility.GetPix(height), height),
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
            var labelSize = infoLabelStyle.Style.CalcSize(characterCount);

            var labelRect = new Rect(
                thumbnailRect.x + 10,
                thumbnailRect.yMax - (labelSize.y + 10),
                labelSize.x + 10,
                labelSize.y + 2);

            GUI.Label(labelRect, characterCount, infoLabelStyle);

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private void OnLoadButtonPushed(object sender, EventArgs e)
        {
            sceneLoader.LoadScene(managingSceneSchema, new()
            {
                Characters = new()
                {
                    Load = characterLoadOptionToggle.Value,
                    ByID = characterIDLoadOptionToggle.Value,
                },
                Message = messageWindowLoadOptionToggle.Value,
                Camera = cameraLoadOptionToggle.Value,
                Lights = lightsLoadOptionToggle.Value,
                Effects = new()
                {
                    Load = effectsLoadOptionToggle.Value,
                    Bloom = bloomLoadOptionToggle.Value,
                    DepthOfField = depthOfFieldLoadOptionToggle.Value,
                    Vignette = vignetteLoadOptionToggle.Value,
                    Fog = fogLoadOptionToggle.Value,
                    SepiaTone = sepiaToneLoadOptionToggle.Value,
                    Blur = blurLoadOptionToggle.Value,
                },
                Background = backgroundLoadOptionToggle.Value,
                Props = propsLoadOptionToggle.Value,
            });

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
            private readonly Label messageLabel;
            private readonly Button deleteButton;
            private readonly Button cancelButton;

            public DeleteSceneMode(
                SceneManagementModal sceneManagementModal,
                ManageSceneMode manageSceneMode,
                SceneRepository sceneRepository)
                : base(sceneManagementModal)
            {
                this.manageSceneMode = manageSceneMode;
                this.sceneRepository = sceneRepository;

                messageLabel = new(string.Empty);

                deleteButton = new(Translation.Get("sceneManagerModal", "deleteFileCommit"));
                deleteButton.ControlEvent += OnDeleteButtonPushed;

                cancelButton = new(Translation.Get("sceneManagerModal", "cancelButton"));
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

                cancelButton.Draw(GUILayout.MinWidth(Utility.GetPix(110)));

                GUILayout.EndHorizontal();
            }

            public override void OnReloadTranslation()
            {
                deleteButton.Label = Translation.Get("sceneManagerModal", "cancelButton");
                cancelButton.Label = Translation.Get("sceneManagerModal", "cancelButton");
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
                    Translation.Get("sceneManagerModal", "deleteFileConfirm"), manageSceneMode.managingScene.Name);

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

            public ErrorMode(SceneManagementModal sceneManagementModal)
                : base(sceneManagementModal)
            {
                okButton = new(Translation.Get("sceneManagerModal", "okButton"));
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

                okButton.Draw(GUILayout.MinWidth(Utility.GetPix(110)));

                GUILayout.EndHorizontal();
            }

            public override void OnScreenDimensionsChanged() =>
                WindowRect = WindowRect with
                {
                    width = ScaledMinimum(windowSize.Width),
                    height = ScaledMinimum(windowSize.Height),
                };

            public override void OnReloadTranslation() =>
                okButton.Label = Translation.Get("sceneManagerModal", "okButton");

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
    }

    private class DeleteCategoryMode : Mode
    {
        private readonly WindowSize windowSize = (450, 200);
        private readonly SceneRepository sceneRepository;
        private readonly Label messageLabel;
        private readonly Button deleteButton;
        private readonly Button cancelButton;

        private string managingCategory;

        public DeleteCategoryMode(SceneManagementModal sceneManagementModal, SceneRepository sceneRepository)
            : base(sceneManagementModal)
        {
            this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));

            messageLabel = new(string.Empty);

            cancelButton = new(Translation.Get("sceneManagerModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonPushed;

            deleteButton = new(Translation.Get("sceneManagerModal", "deleteButton"));
            deleteButton.ControlEvent += OnDeleteButtonPushed;
        }

        public override void Draw()
        {
            messageLabel.Draw(messageStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            deleteButton.Draw(GUILayout.ExpandWidth(false));

            cancelButton.Draw(GUILayout.MinWidth(Utility.GetPix(110)));

            GUILayout.EndHorizontal();
        }

        public override void OnReloadTranslation() =>
            messageLabel.Text = string.Format(Translation.Get("sceneManagerModal", "deleteDirectoryConfirm"), managingCategory);

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
            messageLabel.Text = string.Format(Translation.Get("sceneManagerModal", "deleteDirectoryConfirm"), managingCategory);

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
