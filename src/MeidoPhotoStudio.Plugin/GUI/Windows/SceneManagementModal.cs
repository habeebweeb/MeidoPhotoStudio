using MeidoPhotoStudio.Database.Scenes;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Serialization;

namespace MeidoPhotoStudio.Plugin;

public class SceneManagementModal : BaseWindow
{
    private static readonly Texture2D InfoHighlight = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.8f));

    private readonly SceneRepository sceneRepository;
    private readonly ScreenshotService screenshotService;
    private readonly SceneSchemaBuilder sceneSchemaBuilder;
    private readonly ISceneSerializer sceneSerializer;
    private readonly SceneLoader sceneLoader;
    private readonly Button okButton;
    private readonly Button cancelButton;
    private readonly Button deleteButton;
    private readonly Button overwriteButton;
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

    private Mode currentMode = Mode.None;
    private SceneModel managingScene;
    private string managingCategory = string.Empty;
    private SceneSchema managingSceneSchema;
    private string message = string.Empty;
    private string infoString = string.Empty;

    public SceneManagementModal(
        SceneRepository sceneRepository,
        ScreenshotService screenshotService,
        SceneSchemaBuilder sceneSchemaBuilder,
        ISceneSerializer sceneSerializer,
        SceneLoader sceneLoader)
    {
        this.sceneRepository = sceneRepository ?? throw new ArgumentNullException(nameof(sceneRepository));
        this.screenshotService = screenshotService
            ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));
        this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
        this.sceneSerializer = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));

        okButton = new(Translation.Get("sceneManagerModal", "fileLoadCommit"));
        okButton.ControlEvent += OnCommitButtonPushed;

        cancelButton = new(Translation.Get("sceneManagerModal", "cancelButton"));
        cancelButton.ControlEvent += OnCancelButtonPushed;

        deleteButton = new(Translation.Get("sceneManagerModal", "deleteButton"));
        deleteButton.ControlEvent += OnDeleteButtonPushed;

        overwriteButton = new(Translation.Get("sceneManagerModal", "overwriteButton"));
        overwriteButton.ControlEvent += OnOverwriteButtonPushed;

        loadOptionsToggle = new(Translation.Get("sceneManagerModal", "loadOptionsToggle"), false);

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

        WindowRect = WindowRect with
        {
            x = MiddlePosition.x,
            y = MiddlePosition.y,
        };
    }

    private enum Mode
    {
        None,
        ManageScene,
        DeleteScene,
        DeleteCategory,
        Error,
    }

    public override Rect WindowRect
    {
        set =>
            base.WindowRect = value with
            {
                width = CurrentMode is Mode.ManageScene && loadOptionsToggle.Value
                    ? Screen.width * 0.35f
                    : Screen.width * 0.2f,
                height = CurrentMode switch
                {
                    Mode.DeleteCategory or Mode.Error => 150f,
                    Mode.DeleteScene => Screen.height * 0.32f,
                    Mode.ManageScene or Mode.None or _ => Screen.height * 0.31f,
                },
            };
    }

    public override bool Visible =>
        CurrentMode is not Mode.None;

    private Mode CurrentMode
    {
        get => currentMode;
        set
        {
            if (currentMode == value)
                return;

            currentMode = value;

            OnModeChanged(currentMode);
        }
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width * 0.2f));

        DrawMainContent();

        GUILayout.EndVertical();

        if (CurrentMode is Mode.ManageScene && loadOptionsToggle.Value)
        {
            GUILayout.BeginVertical();

            DrawLoadOptions();

            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();

        GUILayout.EndArea();

        void DrawMainContent()
        {
            if (CurrentMode is Mode.ManageScene or Mode.DeleteScene)
                DrawThumbnail();

            DrawMessage(message);

            if (CurrentMode is Mode.DeleteCategory or Mode.DeleteScene)
                DrawDeleteButtons();
            else if (CurrentMode is Mode.Error)
                DrawErrorButtons();
            else
                DrawSceneManagementButtons();

            if (CurrentMode is Mode.ManageScene)
                DrawLoadOptionsToggle();

            static void DrawMessage(string message)
            {
                var messageStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = Utility.GetPix(12),
                };

                GUILayout.Space(10f);

                GUILayout.Label(message, messageStyle);

                GUILayout.Space(10f);
            }

            void DrawThumbnail()
            {
                if (managingScene is null)
                    return;

                var thumb = managingScene.Thumbnail;

                var windowWidth = Screen.width * 0.2f;

                var scale = Mathf.Min((windowWidth - 20f) / thumb.width, (windowRect.height - 110f) / thumb.height);
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

                var labelSize = labelStyle.CalcSize(new GUIContent(infoString));

                labelBox = new(
                    labelBox.x + 10, labelBox.y + labelBox.height - (labelSize.y + 10), labelSize.x + 10, labelSize.y + 2);

                GUI.Label(labelBox, infoString, labelStyle);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            void DrawDeleteButtons()
            {
                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = Utility.GetPix(12),
                };

                var buttonHeight = GUILayout.Height(Utility.GetPix(20));

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.MinWidth(100));

                GUILayout.EndHorizontal();
            }

            void DrawSceneManagementButtons()
            {
                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = Utility.GetPix(12),
                };

                var buttonHeight = GUILayout.Height(Utility.GetPix(20));

                GUILayout.BeginHorizontal();

                deleteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                overwriteButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));

                GUILayout.FlexibleSpace();

                okButton.Draw(buttonStyle, buttonHeight, GUILayout.ExpandWidth(false));
                cancelButton.Draw(buttonStyle, buttonHeight, GUILayout.MinWidth(100));

                GUILayout.EndHorizontal();
            }

            void DrawErrorButtons()
            {
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                cancelButton.Draw(
                    new GUIStyle(GUI.skin.button)
                    {
                        fontSize = Utility.GetPix(12),
                    },
                    GUILayout.Height(Utility.GetPix(20)),
                    GUILayout.ExpandWidth(false));

                GUILayout.EndHorizontal();
            }

            void DrawLoadOptionsToggle()
            {
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                loadOptionsToggle.Draw(new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = Utility.GetPix(12),
                });

                GUILayout.EndHorizontal();
            }
        }

        void DrawLoadOptions()
        {
            var padding = new GUIStyle(GUI.skin.toggle)
            {
                margin = { left = 10 },
            };

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUI.enabled = managingSceneSchema.Character is not null;

            characterLoadOptionToggle.Draw();
            MpsGui.WhiteLine();

            if (characterLoadOptionToggle.Value)
            {
                GUI.enabled = managingSceneSchema.Character?.Version >= 2;

                characterIDLoadOptionToggle.Draw(padding);
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
                bloomLoadOptionToggle.Draw(padding);
                depthOfFieldLoadOptionToggle.Draw(padding);
                vignetteLoadOptionToggle.Draw(padding);
                fogLoadOptionToggle.Draw(padding);
                sepiaToneLoadOptionToggle.Draw(padding);
                blurLoadOptionToggle.Draw(padding);
            }

            GUI.enabled = managingSceneSchema.Background is not null;

            backgroundLoadOptionToggle.Draw();
            MpsGui.WhiteLine();

            GUI.enabled = managingSceneSchema.Props is not null;

            propsLoadOptionToggle.Draw();
            MpsGui.WhiteLine();

            GUI.enabled = true;

            GUILayout.EndScrollView();
        }
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
                CurrentMode = Mode.Error;

                message = string.Format(Translation.Get("sceneManagerModal", "sceneLoadErrorMessage"), scene.Name);

                Modal.Show(this);

                return;
            }
        }
        catch
        {
            throw;
        }

        managingScene = scene;
        managingSceneSchema = schema;

        CurrentMode = Mode.ManageScene;

        Modal.Show(this);
    }

    public void DeleteCategory(string category)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (!sceneRepository.ContainsCategory(category))
            throw new ArgumentException($"'{category}' does not exist.", nameof(category));

        managingCategory = category;

        CurrentMode = Mode.DeleteCategory;

        Modal.Show(this);
    }

    protected override void ReloadTranslation()
    {
        okButton.Label = Translation.Get("sceneManagerModal", "fileLoadCommit");
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
    }

    private void OnModeChanged(Mode currentMode)
    {
        if (currentMode is Mode.ManageScene)
            ApplyManageSceneMode();
        else if (currentMode is Mode.DeleteScene)
            ApplyDeleteSceneMode();
        else if (currentMode is Mode.DeleteCategory)
            ApplyDeleteCategoryMode();
        else
            ResetState();

        if (currentMode is Mode.None)
            WindowRect = WindowRect with
            {
                x = MiddlePosition.x,
                y = MiddlePosition.y,
            };

        void ApplyManageSceneMode()
        {
            message = managingScene.Name;

            var characterCount = managingSceneSchema?.Character?.Characters.Count ?? 0;

            infoString = string.Format(
                characterCount is 1
                    ? Translation.Get("sceneManagerModal", "infoMaidSingular")
                    : Translation.Get("sceneManagerModal", "infoMaidPlural"),
                characterCount);

            okButton.Label = Translation.Get("sceneManagerModal", "fileLoadCommit");
        }

        void ApplyDeleteSceneMode()
        {
            message = string.Format(Translation.Get("sceneManagerModal", "deleteFileConfirm"), managingScene.Name);
            okButton.Label = Translation.Get("sceneManagerModal", "deleteFileCommit");
        }

        void ApplyDeleteCategoryMode()
        {
            message = string.Format(Translation.Get("sceneManagerModal", "deleteDirectoryConfirm"), managingCategory);
            okButton.Label = Translation.Get("sceneManagerModal", "deleteDirectoryButton");
        }

        void ResetState()
        {
            managingScene = null;
            managingSceneSchema = null;
            managingCategory = string.Empty;
            message = string.Empty;
        }
    }

    private void OnOverwriteButtonPushed(object sender, EventArgs e)
    {
        if (managingScene is null)
            return;

        var sceneSchema = sceneSchemaBuilder.Build();

        screenshotService.TakeScreenshotToTexture(
            (screenshot) =>
            {
                sceneRepository.Overwrite(sceneSchema, screenshot, managingScene);

                CurrentMode = Mode.None;

                Modal.Close();
            },
            new());
    }

    private void OnDeleteButtonPushed(object sender, EventArgs e)
    {
        if (CurrentMode is not Mode.ManageScene)
            return;

        CurrentMode = Mode.DeleteScene;
    }

    private void OnCommitButtonPushed(object sender, EventArgs e)
    {
        if (CurrentMode is Mode.DeleteCategory)
            DeleteCategory(managingCategory);
        else if (CurrentMode is Mode.DeleteScene)
            DeleteScene(managingScene);
        else if (CurrentMode is Mode.ManageScene)
            LoadScene(managingSceneSchema);

        CurrentMode = Mode.None;

        Modal.Close();

        void LoadScene(SceneSchema scene) =>
            sceneLoader.LoadScene(scene, new()
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

        void DeleteScene(SceneModel scene)
        {
            if (scene is null)
                return;

            sceneRepository.Delete(scene);
        }

        void DeleteCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return;

            sceneRepository.DeleteCategory(category);
        }
    }

    private void OnCancelButtonPushed(object sender, EventArgs e)
    {
        if (CurrentMode is Mode.DeleteScene)
        {
            CurrentMode = Mode.ManageScene;
        }
        else
        {
            CurrentMode = Mode.None;

            Modal.Close();
        }
    }
}
