using System;
using System.IO;
using System.Text;

using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency("org.bepinex.plugins.unityinjectorloader", BepInDependency.DependencyFlags.SoftDependency)]
public class MeidoPhotoStudio : BaseUnityPlugin
{
    public const string PluginName = "MeidoPhotoStudio";
    public const string PluginVersion = "1.0.0";
    public const string PluginSubVersion = "beta.5.1";
    public const short SceneVersion = 3;
    public const int KankyoMagic = -765;

    public static readonly byte[] SceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");
    public static readonly string PluginString = $"{PluginName} {PluginVersion}";

    private const string PluginGuid = "com.habeebweeb.com3d2.meidophotostudio";

    private static Constants.Scene currentScene;

    private HarmonyLib.Harmony harmony;
    private WindowManager windowManager;
    private SceneManager sceneManager;
    private MeidoManager meidoManager;
    private EnvironmentManager environmentManager;
    private MessageWindowManager messageWindowManager;
    private LightManager lightManager;
    private PropManager propManager;
    private EffectManager effectManager;
    private CameraManager cameraManager;
    private ScreenshotService screenshotService;
    private bool initialized;
    private bool active;

    static MeidoPhotoStudio()
    {
        Input.Register(MpsKey.Screenshot, KeyCode.S, "Take screenshot");
        Input.Register(MpsKey.Activate, KeyCode.F6, "Activate/deactivate MeidoPhotoStudio");

        if (!string.IsNullOrEmpty(PluginSubVersion))
            PluginString += $"-{PluginSubVersion}";
    }

    public static bool EditMode =>
        currentScene is Constants.Scene.Edit;

    public static MeidoPhotoStudio Instance { get; private set; }

    public bool UIActive { get; set; }

    public byte[] SaveScene(bool environment = false)
    {
        if (meidoManager.Busy)
            return null;

        try
        {
            using var memoryStream = new MemoryStream();
            using var headerWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

            headerWriter.Write(SceneHeader);

            new SceneMetadata
            {
                Version = SceneVersion,
                Environment = environment,
                MaidCount = environment ? KankyoMagic : meidoManager.ActiveMeidoList.Count,
                MMConverted = false,
            }.WriteMetadata(headerWriter);

            using var compressionStream = memoryStream.GetCompressionStream();
            using var dataWriter = new BinaryWriter(compressionStream, Encoding.UTF8);

            if (!environment)
            {
                Serialization.Get<MeidoManager>().Serialize(meidoManager, dataWriter);
                Serialization.Get<MessageWindowManager>().Serialize(messageWindowManager, dataWriter);
                Serialization.Get<CameraManager>().Serialize(cameraManager, dataWriter);
            }

            Serialization.Get<LightManager>().Serialize(lightManager, dataWriter);
            Serialization.Get<EffectManager>().Serialize(effectManager, dataWriter);
            Serialization.Get<EnvironmentManager>().Serialize(environmentManager, dataWriter);
            Serialization.Get<PropManager>().Serialize(propManager, dataWriter);

            dataWriter.Write("END");

            compressionStream.Close();

            var data = memoryStream.ToArray();

            return data;
        }
        catch (Exception e)
        {
            Utility.LogError($"Failed to save scene because {e.Message}\n{e.StackTrace}");

            return null;
        }
    }

    public void LoadScene(byte[] buffer)
    {
        if (meidoManager.Busy)
        {
            Utility.LogMessage("Could not apply scene. Meidos are Busy");

            return;
        }

        using var memoryStream = new MemoryStream(buffer);
        using var headerReader = new BinaryReader(memoryStream, Encoding.UTF8);

        if (!Utility.BytesEqual(headerReader.ReadBytes(SceneHeader.Length), SceneHeader))
        {
            Utility.LogError("Not a MPS scene!");

            return;
        }

        var metadata = SceneMetadata.ReadMetadata(headerReader);

        if (metadata.Version > SceneVersion)
        {
            Utility.LogWarning("Cannot load scene. Scene is too new.");
            Utility.LogWarning($"Your version: {SceneVersion}, Scene version: {metadata.Version}");

            return;
        }

        using var uncompressed = memoryStream.Decompress();
        using var dataReader = new BinaryReader(uncompressed, Encoding.UTF8);

        var header = string.Empty;
        var previousHeader = string.Empty;

        try
        {
            while ((header = dataReader.ReadString()) is not "END")
            {
                switch (header)
                {
                    case MeidoManager.Header:
                        Serialization.Get<MeidoManager>().Deserialize(meidoManager, dataReader, metadata);

                        break;
                    case MessageWindowManager.Header:
                        Serialization.Get<MessageWindowManager>()
                            .Deserialize(messageWindowManager, dataReader, metadata);

                        break;
                    case CameraManager.Header:
                        Serialization.Get<CameraManager>().Deserialize(cameraManager, dataReader, metadata);

                        break;
                    case LightManager.Header:
                        Serialization.Get<LightManager>().Deserialize(lightManager, dataReader, metadata);

                        break;
                    case EffectManager.Header:
                        Serialization.Get<EffectManager>().Deserialize(effectManager, dataReader, metadata);

                        break;
                    case EnvironmentManager.Header:
                        Serialization.Get<EnvironmentManager>().Deserialize(environmentManager, dataReader, metadata);

                        break;
                    case PropManager.Header:
                        Serialization.Get<PropManager>().Deserialize(propManager, dataReader, metadata);

                        break;
                    default:
                        throw new Exception($"Unknown header '{header}'");
                }

                previousHeader = header;
            }
        }
        catch (Exception e)
        {
            Utility.LogError(
                $"Failed to deserialize scene because {e.Message}\nCurrent header: '{header}'. " +
                $"Last header: '{previousHeader}'");

            Utility.LogError(e.StackTrace);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) =>
        UpdateCurrentScene(scene);

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (active)
            Deactivate(true);

        CameraUtility.MainCamera.ResetCalcNearClip();
    }

    private void UpdateCurrentScene(Scene scene) =>
        currentScene = scene.buildIndex switch
        {
            3 => Constants.Scene.Daily,
            5 => Constants.Scene.Edit,
            _ => Constants.Scene.None,
        };

    private void Awake()
    {
        Instance = this;

        harmony = HarmonyLib.Harmony.CreateAndPatchAll(typeof(AllProcPropSeqPatcher));
        harmony.PatchAll(typeof(BgMgrPatcher));
        harmony.PatchAll(typeof(MeidoManager));

        DontDestroyOnLoad(this);

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;

        UpdateCurrentScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (active)
            Deactivate(true);

        CameraUtility.MainCamera.ResetCalcNearClip();

        harmony.UnpatchSelf();

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;

        Destroy(GameObject.Find("[MPS DragPoint Parent]"));
        WfCameraMoveSupportUtility.Destroy();

        Constants.Destroy();
    }

    private void Start()
    {
        Constants.Initialize();
        Translation.Initialize(Translation.CurrentLanguage);
    }

    private void Update()
    {
        if (currentScene is not Constants.Scene.Daily and not Constants.Scene.Edit)
            return;

        if (Input.GetKeyDown(MpsKey.Activate))
        {
            if (active)
                Deactivate();
            else
                Activate();
        }

        if (!active)
            return;

        if (!Input.Control && !Input.GetKey(MpsKey.CameraLayer) && Input.GetKeyDown(MpsKey.Screenshot))
            screenshotService.TakeScreenshotToFile();

        meidoManager.Update();
        cameraManager.Update();
        windowManager.Update();
        effectManager.Update();
        sceneManager.Update();
    }

    private void OnGUI()
    {
        if (!UIActive)
            return;

        windowManager.DrawWindows();

        if (DropdownHelper.Visible)
            DropdownHelper.HandleDropdown();

        if (Modal.Visible)
            Modal.Draw();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        screenshotService = new(this);
        meidoManager = new();
        environmentManager = new();
        messageWindowManager = new();
        messageWindowManager.Activate();
        lightManager = new();
        propManager = new(meidoManager);
        sceneManager = new(this, screenshotService);
        cameraManager = new();

        effectManager = new();
        effectManager.AddManager<BloomEffectManager>();
        effectManager.AddManager<DepthOfFieldEffectManager>();
        effectManager.AddManager<FogEffectManager>();
        effectManager.AddManager<VignetteEffectManager>();
        effectManager.AddManager<SepiaToneEffectManager>();
        effectManager.AddManager<BlurEffectManager>();

        meidoManager.BeginCallMeidos += (_, _) =>
            UIActive = false;

        meidoManager.EndCallMeidos += (_, _) =>
            UIActive = true;

        var maidSwitcherPane = new MaidSwitcherPane(meidoManager);
        var sceneWindow = new SceneWindow(sceneManager);

        windowManager = new()
        {
            [Constants.Window.Main] = new MainWindow(meidoManager, propManager, lightManager)
            {
                [Constants.Window.Call] = new CallWindowPane(meidoManager),
                [Constants.Window.Pose] = new PoseWindowPane(meidoManager, maidSwitcherPane),
                [Constants.Window.Face] = new FaceWindowPane(meidoManager, maidSwitcherPane),
                [Constants.Window.BG] =
                    new BGWindowPane(environmentManager, lightManager, effectManager, sceneWindow, cameraManager),
                [Constants.Window.BG2] = new BG2WindowPane(meidoManager, propManager),
                [Constants.Window.Settings] = new SettingsWindowPane(),
            },
            [Constants.Window.Message] = new MessageWindow(messageWindowManager),
            [Constants.Window.Save] = sceneWindow,
        };
    }

    private void Activate()
    {
        if (!GameMain.Instance.SysDlg.IsDecided)
            return;

        if (!initialized)
        {
            Initialize();
        }
        else
        {
            meidoManager.Activate();
            environmentManager.Activate();
            cameraManager.Activate();
            propManager.Activate();
            lightManager.Activate();
            effectManager.Activate();
            messageWindowManager.Activate();
            windowManager.Activate();
        }

        UIActive = true;
        active = true;

        if (!EditMode)
        {
            // TODO: Rework this to not use null propagation (UNT008)
            var dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;

            if (dailyPanel)
                dailyPanel.SetActive(false);
        }
    }

    private void Deactivate(bool force = false)
    {
        if (meidoManager.Busy || SceneManager.Busy)
            return;

        var sysDialog = GameMain.Instance.SysDlg;

        if (!sysDialog.IsDecided && !force)
            return;

        UIActive = false;
        active = false;

        if (force)
        {
            Exit();

            return;
        }

        sysDialog.Show(
            string.Format(Translation.Get("systemMessage", "exitConfirm"), PluginName),
            SystemDialog.TYPE.OK_CANCEL,
            Exit,
            Resume);

        void Resume()
        {
            sysDialog.Close();
            UIActive = true;
            active = true;
        }

        void Exit()
        {
            sysDialog.Close();

            meidoManager.Deactivate();
            environmentManager.Deactivate();
            cameraManager.Deactivate();
            propManager.Deactivate();
            lightManager.Deactivate();
            effectManager.Deactivate();
            messageWindowManager.Deactivate();
            windowManager.Deactivate();
            Input.Deactivate();

            Modal.Close();

            Configuration.Config.Save();

            if (EditMode)
                return;

            // TODO: Rework this to not use null propagation (UNT008)
            var dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;

            // NOTE: using is (not) for null checks on UnityEngine.Object does not work
            if (dailyPanel)
                dailyPanel.SetActive(true);
        }
    }
}
