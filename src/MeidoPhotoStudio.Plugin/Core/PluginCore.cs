using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using MeidoPhotoStudio.Plugin.Service;
using MeidoPhotoStudio.Plugin.Service.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeidoPhotoStudio.Plugin.Core;

/// <summary>Core plugin.</summary>
public partial class PluginCore : MonoBehaviour
{
    private WindowManager windowManager;
    private SceneManager sceneManager;
    private MeidoManager meidoManager;
    private EnvironmentManager environmentManager;
    private MessageWindowManager messageWindowManager;
    private PropManager propManager;
    private SubCameraController subCameraController;
    private EffectManager effectManager;
    private CameraController cameraController;
    private CameraSpeedController cameraSpeedController;
    private CameraSaveSlotController cameraSaveSlotController;
    private ScreenshotService screenshotService;
    private CustomMaidSceneService customMaidSceneService;
    private InputPollingService inputPollingService;
    private bool initialized;
    private InputConfiguration inputConfiguration;
    private InputRemapper inputRemapper;
    private bool active;
    private bool uiActive;
    private LightRepository lightRepository;

    public bool UIActive
    {
        get => uiActive;
        set
        {
            var newValue = value;

            if (!active)
                newValue = false;

            uiActive = newValue;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        customMaidSceneService = new();

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDestroy()
    {
        if (active)
            Deactivate(true);

        CameraUtility.MainCamera.ResetCalcNearClip();

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;

        Destroy(GameObject.Find("[MPS DragPoint Parent]"));
        Destroy(GameObject.Find("[MPS Drag Handle Parent]"));
        Destroy(GameObject.Find("[MPS Light Parent]"));
        Destroy(Utility.MousePositionGameObject);
        WfCameraMoveSupportUtility.Destroy();

        Constants.Destroy();
    }

    private void Start()
    {
        Constants.Initialize();
        Translation.Initialize(Translation.CurrentLanguage);

        inputConfiguration = new InputConfiguration(MeidoPhotoStudio.Plugin.Configuration.Config);

        inputPollingService = gameObject.AddComponent<InputPollingService>();
        inputPollingService.AddInputHandler(new InputHandler(this, inputConfiguration));
        gameObject.AddComponent<DragHandle.ClickHandler>();
    }

    private void Update()
    {
        if (!customMaidSceneService.ValidScene)
            return;

        if (!active)
            return;

        windowManager.Update();
    }

    private void OnGUI()
    {
        if (!uiActive)
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

        screenshotService = gameObject.AddComponent<ScreenshotService>();
        screenshotService.PluginCore = this;

        inputRemapper = gameObject.AddComponent<InputRemapper>();
        inputRemapper.InputPollingService = inputPollingService;

        AddPluginActiveInputHandler(new ScreenshotService.InputHandler(screenshotService, inputConfiguration));

        var generalDragPointInputService = new GeneralDragPointInputService(inputConfiguration);

        AddPluginActiveInputHandler(generalDragPointInputService);

        var dragPointMeidoInputService = new DragPointMeidoInputService(
            new DragPointFingerInputService(inputConfiguration),
            new DragPointHeadInputService(inputConfiguration),
            new DragPointLimbInputService(inputConfiguration),
            new DragPointMuneInputService(inputConfiguration),
            new DragPointPelvisInputService(inputConfiguration),
            new DragPointSpineInputService(inputConfiguration),
            new DragPointHipInputService(inputConfiguration),
            new DragPointThighInputService(inputConfiguration),
            new DragPointTorsoInputService(inputConfiguration));

        AddPluginActiveInputHandler(dragPointMeidoInputService);

        meidoManager = new(customMaidSceneService, generalDragPointInputService, dragPointMeidoInputService);

        AddPluginActiveInputHandler(new MeidoManager.InputHandler(meidoManager, inputConfiguration));

        environmentManager = new(customMaidSceneService, generalDragPointInputService);

        messageWindowManager = new();
        messageWindowManager.Activate();

        lightRepository = new LightRepository();

        var lightSelectionController = new LightSelectionController(lightRepository);

        var lightDragHandleRepository = new LightDragHandleRepository(
            generalDragPointInputService, lightRepository, lightSelectionController);

        propManager = new(meidoManager, generalDragPointInputService);

        cameraController = new(customMaidSceneService);
        cameraController.Activate();

        cameraSpeedController = new();
        cameraSaveSlotController = new(cameraController);
        cameraSaveSlotController.Activate();

        subCameraController = gameObject.AddComponent<SubCameraController>();

        AddPluginActiveInputHandler(
            new CameraInputHandler(
                cameraController, cameraSpeedController, cameraSaveSlotController, inputConfiguration));

        effectManager = new();
        effectManager.AddManager<BloomEffectManager>();
        effectManager.AddManager<DepthOfFieldEffectManager>();
        effectManager.AddManager<FogEffectManager>();
        effectManager.AddManager<VignetteEffectManager>();
        effectManager.AddManager<SepiaToneEffectManager>();
        effectManager.AddManager<BlurEffectManager>();

        sceneManager = new(
            screenshotService,
            new(
                meidoManager,
                messageWindowManager,
                cameraSaveSlotController,
                lightRepository,
                effectManager,
                environmentManager,
                propManager));

        AddPluginActiveInputHandler(new SceneManager.InputHandler(sceneManager, inputConfiguration));

        var sceneWindow = new SceneWindow(sceneManager);

        AddPluginActiveInputHandler(new SceneWindow.InputHandler(sceneWindow, inputConfiguration));

        var messageWindow = new MessageWindow(messageWindowManager);

        AddPluginActiveInputHandler(new MessageWindow.InputHandler(messageWindow, inputConfiguration));

        var maidSwitcherPane = new MaidSwitcherPane(meidoManager, customMaidSceneService);
        var mainWindow = new MainWindow(
            meidoManager,
            propManager,
            lightSelectionController,
            customMaidSceneService,
            inputRemapper)
        {
            [Constants.Window.Call] = new CallWindowPane(meidoManager, customMaidSceneService),
            [Constants.Window.Pose] = new PoseWindowPane(meidoManager, maidSwitcherPane),
            [Constants.Window.Face] = new FaceWindowPane(meidoManager, maidSwitcherPane),
            [Constants.Window.BG] = new BGWindowPane()
                {
                    new SceneManagementPane(sceneWindow),
                    new BackgroundSelectorPane(environmentManager),
                    new DragPointPane(),
                    new CameraPane(cameraController, cameraSaveSlotController),
                    new LightsPane(lightRepository, lightSelectionController),
                    new EffectsPane()
                    {
                        ["bloom"] = new BloomPane(effectManager),
                        ["dof"] = new DepthOfFieldPane(effectManager),
                        ["vignette"] = new VignettePane(effectManager),
                        ["fog"] = new FogPane(effectManager),
                    },
                    new OtherEffectsPane(effectManager),
                },
            [Constants.Window.BG2] = new BG2WindowPane(meidoManager, propManager),
            [Constants.Window.Settings] = new SettingsWindowPane(inputConfiguration, inputRemapper),
        };

        AddPluginActiveInputHandler(new MainWindow.InputHandler(mainWindow, inputConfiguration));

        windowManager = new()
        {
            [Constants.Window.Main] = mainWindow,
            [Constants.Window.Message] = messageWindow,
            [Constants.Window.Save] = sceneWindow,
        };

        meidoManager.BeginCallMeidos += (_, _) =>
            uiActive = false;

        meidoManager.EndCallMeidos += (_, _) =>
            uiActive = true;

        void AddPluginActiveInputHandler<T>(T inputHandler)
            where T : IInputHandler =>
            inputPollingService.AddInputHandler(new PluginActiveInputHandler<T>(this, inputHandler));
    }

    private void Activate()
    {
        if (!GameMain.Instance.SysDlg.IsDecided)
            return;

        if (!initialized)
        {
            Initialize();

            lightRepository.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());
        }
        else
        {
            meidoManager.Activate();
            environmentManager.Activate();
            cameraController.Activate();
            propManager.Activate();
            effectManager.Activate();
            messageWindowManager.Activate();
            windowManager.Activate();
            subCameraController.Activate();
            cameraSaveSlotController.Activate();

            lightRepository.AddLight(GameMain.Instance.MainLight.GetComponent<Light>());

            screenshotService.enabled = true;
        }

        uiActive = true;
        active = true;

        if (!customMaidSceneService.EditScene)
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

        uiActive = false;
        active = false;

        if (force)
        {
            Exit();

            return;
        }

        sysDialog.Show(
            string.Format(Translation.Get("systemMessage", "exitConfirm"), Plugin.PluginName),
            SystemDialog.TYPE.OK_CANCEL,
            Exit,
            Resume);

        void Resume()
        {
            sysDialog.Close();
            uiActive = true;
            active = true;
        }

        void Exit()
        {
            sysDialog.Close();

            meidoManager.Deactivate();
            environmentManager.Deactivate();
            cameraController.Deactivate();
            propManager.Deactivate();
            effectManager.Deactivate();
            messageWindowManager.Deactivate();
            windowManager.Deactivate();
            cameraSpeedController.Deactivate();
            subCameraController.Deactivate();
            screenshotService.enabled = false;

            lightRepository.RemoveAllLights();

            Modal.Close();

            MeidoPhotoStudio.Plugin.Configuration.Config.Save();

            if (customMaidSceneService.EditScene)
                return;

            // TODO: Rework this to not use null propagation (UNT008)
            var dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;

            // NOTE: using is (not) for null checks on UnityEngine.Object does not work
            if (dailyPanel)
                dailyPanel.SetActive(true);
        }
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (active)
            Deactivate(true);

        CameraUtility.MainCamera.ResetCalcNearClip();
    }

    private void ToggleActive()
    {
        if (active)
            Deactivate();
        else
            Activate();
    }
}
