using MeidoPhotoStudio.Plugin.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin.Core;

public class PluginCore : MonoBehaviour
{
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
    private CustomMaidSceneService customMaidSceneService;
    private bool initialized;
    private bool active;
    private bool uiActive;

    static PluginCore()
    {
        Input.Register(MpsKey.Screenshot, KeyCode.S, "Take screenshot");
        Input.Register(MpsKey.Activate, KeyCode.F6, "Activate/deactivate MeidoPhotoStudio");
    }

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
        if (!customMaidSceneService.ValidScene)
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

        meidoManager = new(customMaidSceneService);
        environmentManager = new(customMaidSceneService);
        messageWindowManager = new();
        messageWindowManager.Activate();
        lightManager = new();
        propManager = new(meidoManager);
        cameraManager = new(customMaidSceneService);

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
                cameraManager,
                lightManager,
                effectManager,
                environmentManager,
                propManager));

        meidoManager.BeginCallMeidos += (_, _) =>
            uiActive = false;

        meidoManager.EndCallMeidos += (_, _) =>
            uiActive = true;

        var maidSwitcherPane = new MaidSwitcherPane(meidoManager, customMaidSceneService);
        var sceneWindow = new SceneWindow(sceneManager);

        windowManager = new()
        {
            [Constants.Window.Main] = new MainWindow(meidoManager, propManager, lightManager, customMaidSceneService)
            {
                [Constants.Window.Call] = new CallWindowPane(meidoManager, customMaidSceneService),
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
            cameraManager.Deactivate();
            propManager.Deactivate();
            lightManager.Deactivate();
            effectManager.Deactivate();
            messageWindowManager.Deactivate();
            windowManager.Deactivate();
            screenshotService.enabled = false;
            Input.Deactivate();

            Modal.Close();

            Configuration.Config.Save();

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
}
