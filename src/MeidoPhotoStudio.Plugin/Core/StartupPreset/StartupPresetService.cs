using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.SceneManagement;
using MeidoPhotoStudio.Plugin.Core.Schema;
using MeidoPhotoStudio.Plugin.Core.Schema.Camera;
using MeidoPhotoStudio.Plugin.Core.Schema.Light;
using MeidoPhotoStudio.Plugin.Core.Serialization;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.StartupPreset;

public class StartupPresetService : IActivateable
{
    private static readonly SceneSchema DefaultSchema;

    private readonly string startupPresetDirectory;
    private readonly CharacterService characterService;
    private readonly ScreenshotService screenshotService;
    private readonly SceneSchemaBuilder sceneSchemaBuilder;
    private readonly ISceneSerializer sceneSerializer;
    private readonly SceneLoader sceneLoader;

    private SceneSchema customPresetSchema;

    static StartupPresetService()
    {
        DefaultSchema = new()
        {
            Character = null,
            MessageWindow = null,
            Camera = new()
            {
                CurrentCameraSlot = 0,
                CameraInfo = new([..
                    Enumerable.Repeat(new CameraInfo(new(0f, 0.9f, 0f), Quaternion.Euler(10f, 180f, 0f), 3f, 35f), 5)
                    .Select(static info => new CameraInfoSchema()
                    {
                        TargetPosition = info.TargetPos,
                        Rotation = info.Angle,
                        Distance = info.Distance,
                        FOV = info.FOV,
                    })]),
            },
            Lights = new()
            {
                Lights = new([CreateLightSchema()]),
            },
            Effects = new()
            {
                Bloom = new()
                {
                    Active = true,
                    BloomValue = 0,
                    BlurIterations = 3,
                    BloomThresholdColour = Color.white,
                    BloomHDR = false,
                },
                DepthOfField = new()
                {
                    Active = false,
                    FocalLength = 10f,
                    FocalSize = 0.05f,
                    Aperture = 11.5f,
                    MaxBlurSize = 2f,
                    VisualizeFocus = false,
                },
                Vignette = new()
                {
                    Active = false,
                    Intensity = -3.98f,
                    Blur = 0.82f,
                    BlurSpread = 4.19f,
                    ChromaticAberration = -0.75f,
                },
                Fog = new()
                {
                    Active = false,
                    Distance = 30f,
                    Density = 1f,
                    HeightScale = 20f,
                    Height = 0f,
                    FogColour = new(0.5f, 0.5f, 0.5f),
                },
                Blur = new()
                {
                    Active = false,
                    BlurSize = 3f,
                    BlurIterations = 2,
                    Downsample = 1,
                },
                SepiaTone = new()
                {
                    Active = false,
                },
            },
            Background = new()
            {
                Background = new()
                {
                    ID = "Theater",
                    Category = Database.Background.BackgroundCategory.COM3D2,
                    AssetName = "Theater",
                },
                Transform = new(),
            },
            Props = null,
            Extension = null,
        };

        static LightSchema CreateLightSchema()
        {
            return new()
            {
                DirectionalProperties = CreateLightPropertiesSchema(),
                SpotProperties = CreateLightPropertiesSchema(),
                PointProperties = CreateLightPropertiesSchema(),
                Position = new(0f, 1.9f, 0.4f),
                Type = LightType.Directional,
                ColourMode = false,
                Enabled = true,
            };

            static LightPropertiesSchema CreateLightPropertiesSchema() =>
                new()
                {
                    Rotation = Quaternion.Euler(40f, 180f, 18f),
                    Intensity = 0.95f,
                    Range = 10f,
                    SpotAngle = 30f,
                    ShadowStrength = 0.098f,
                    Colour = Color.white,
                };
        }
    }

    public StartupPresetService(
        string startupPresetDirectory,
        CharacterService characterService,
        ScreenshotService screenshotService,
        SceneSchemaBuilder sceneSchemaBuilder,
        ISceneSerializer sceneSerializer,
        SceneLoader sceneLoader,
        LoadOptionsService loadOptionsService)
    {
        if (string.IsNullOrEmpty(startupPresetDirectory))
            throw new ArgumentException($"'{nameof(startupPresetDirectory)}' cannot be null or empty.", nameof(startupPresetDirectory));

        this.startupPresetDirectory = startupPresetDirectory;
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.screenshotService = screenshotService ? screenshotService : throw new ArgumentNullException(nameof(screenshotService));
        this.sceneSchemaBuilder = sceneSchemaBuilder ?? throw new ArgumentNullException(nameof(sceneSchemaBuilder));
        this.sceneSerializer = sceneSerializer ?? throw new ArgumentNullException(nameof(sceneSerializer));
        this.sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
        _ = loadOptionsService ?? throw new ArgumentNullException(nameof(loadOptionsService));

        LoadOptions = loadOptionsService.CreateLoadOptions();
    }

    public event EventHandler UpdatedCustomStartupPreset;

    public event EventHandler RefreshedCustomStartupPreset;

    public bool Enabled { get; set; }

    public bool UseCustomPreset { get; set; }

    public ILoadOptions LoadOptions { get; }

    public string StartupPresetPath =>
        Path.Combine(startupPresetDirectory, "startup_preset.png");

    public void RefreshStartupPreset()
    {
        try
        {
            using var fileStream = File.OpenRead(StartupPresetPath);

            if (!SeekToEndOfPNG(fileStream))
                fileStream.Position = 0L;

            customPresetSchema = sceneSerializer.DeserializeScene(fileStream);

            RefreshedCustomStartupPreset?.Invoke(this, EventArgs.Empty);
        }
        catch (FileNotFoundException)
        {
        }
        catch (IOException e)
        {
            Plugin.Logger.LogWarning($"Could not open startup preset file because {e}.");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"Could not load startup preset because {e}.");
        }

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

    public void UpdateStartupPreset()
    {
        if (characterService.Busy)
        {
            Plugin.Logger.LogInfo($"Cannot update startup preset while character service is busy");

            return;
        }

        screenshotService.TakeScreenshotToTexture(OnScreenshotTaken, new(false, false, false));

        void OnScreenshotTaken(Texture2D screenshot)
        {
            customPresetSchema = sceneSchemaBuilder.Build();

            try
            {
                Directory.CreateDirectory(startupPresetDirectory);

                using var fileStream = File.OpenWrite(StartupPresetPath);

                ResizeToFit(screenshot, 480, 270);

                var encodedScreenshot = screenshot.EncodeToPNG();

                fileStream.Write(encodedScreenshot, 0, encodedScreenshot.Length);

                sceneSerializer.SerializeScene(fileStream, customPresetSchema);
            }
            finally
            {
                Object.DestroyImmediate(screenshot);
            }

            UpdatedCustomStartupPreset?.Invoke(this, EventArgs.Empty);

            static void ResizeToFit(Texture2D texture, int maxWidth, int maxHeight)
            {
                var width = texture.width;
                var height = texture.height;

                if (width == maxWidth && height == maxHeight)
                    return;

                var scale = Mathf.Min(maxWidth / (float)width, maxHeight / (float)height);

                width = Mathf.RoundToInt(width * scale);
                height = Mathf.RoundToInt(height * scale);
                TextureScale.Bilinear(texture, width, height);
            }
        }
    }

    void IActivateable.Activate()
    {
        if (!Enabled)
            return;

        if (LoadOptions.TryGetOption("characters", out var characterOption))
            characterOption.Enabled = false;

        if (LoadOptions.TryGetOption("message", out var messageOption))
            messageOption.Enabled = false;

        var preset = DefaultSchema;

        if (UseCustomPreset)
        {
            if (customPresetSchema is null)
                RefreshStartupPreset();

            preset = customPresetSchema ?? DefaultSchema;
        }

        if (preset is null)
            return;

        sceneLoader.LoadScene(preset, LoadOptions);
    }

    void IActivateable.Deactivate()
    {
    }
}
