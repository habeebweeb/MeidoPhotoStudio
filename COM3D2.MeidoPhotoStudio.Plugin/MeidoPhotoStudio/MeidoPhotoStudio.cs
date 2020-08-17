using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MeidoPhotoStudio : BaseUnityPlugin
    {
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio";
        public const string pluginName = "MeidoPhotoStudio";
        public const string pluginVersion = "0.0.0";
        public static string pluginString;
        private WindowManager windowManager;
        private MeidoManager meidoManager;
        private EnvironmentManager environmentManager;
        private MessageWindowManager messageWindowManager;
        private LightManager lightManager;
        private PropManager propManager;
        private EffectManager effectManager;
        private Constants.Scene currentScene;
        private bool initialized = false;
        private bool isActive = false;
        private bool uiActive = false;

        static MeidoPhotoStudio() => pluginString = $"{pluginName} {pluginVersion}";

        private void Awake() => DontDestroyOnLoad(this);

        private void Start()
        {
            Constants.Initialize();
            Translation.Initialize("en");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (currentScene == Constants.Scene.Daily)
            {
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    if (isActive) Deactivate();
                    else Activate();
                }

                if (isActive)
                {
                    bool qFlag = Input.GetKey(KeyCode.Q);
                    if (!qFlag && Input.GetKeyDown(KeyCode.S))
                    {
                        StartCoroutine(TakeScreenShot());
                    }

                    meidoManager.Update();
                    environmentManager.Update();
                    windowManager.Update();
                    effectManager.Update();
                }
            }
        }

        private IEnumerator TakeScreenShot()
        {
            // Hide UI and dragpoints
            GameObject editUI = GameObject.Find("/UI Root/Camera");
            GameObject fpsViewer =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/FpsCounter", false);
            GameObject sysDialog =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/SystemDialog", false);
            GameObject sysShortcut =
                UTY.GetChildObject(GameMain.Instance.gameObject, "SystemUI Root/SystemShortcut", false);
            editUI.SetActive(false);
            fpsViewer.SetActive(false);
            sysDialog.SetActive(false);
            sysShortcut.SetActive(false);
            uiActive = false;

            List<Meido> activeMeidoList = this.meidoManager.ActiveMeidoList;
            bool[] isIK = new bool[activeMeidoList.Count];
            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                isIK[i] = meido.IsIK;
                if (meido.IsIK) meido.IsIK = false;
            }

            GizmoRender.UIVisible = false;

            yield return new WaitForEndOfFrame();

            // Take Screenshot
            int[] superSize = new[] { 1, 2, 4 };
            int selectedSuperSize = superSize[(int)GameMain.Instance.CMSystem.ScreenShotSuperSize];

            Application.CaptureScreenshot(Utility.ScreenshotFilename(), selectedSuperSize);
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);

            yield return new WaitForEndOfFrame();

            // Show UI and dragpoints
            uiActive = true;
            editUI.SetActive(true);
            fpsViewer.SetActive(GameMain.Instance.CMSystem.ViewFps);
            sysDialog.SetActive(true);
            sysShortcut.SetActive(true);

            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                if (isIK[i]) meido.IsIK = true;
            }

            GizmoRender.UIVisible = true;
        }

        private void OnGUI()
        {
            if (uiActive)
            {
                windowManager.DrawWindows();

                if (DropdownHelper.Visible) DropdownHelper.HandleDropdown();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            currentScene = (Constants.Scene)scene.buildIndex;
        }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;

            meidoManager = new MeidoManager();
            environmentManager = new EnvironmentManager(meidoManager);
            messageWindowManager = new MessageWindowManager();
            lightManager = new LightManager();
            propManager = new PropManager(meidoManager);
            effectManager = new EffectManager();

            MaidSwitcherPane maidSwitcherPane = new MaidSwitcherPane(meidoManager);

            windowManager = new WindowManager()
            {
                [Constants.Window.Main] = new MainWindow(meidoManager)
                {
                    [Constants.Window.Call] = new CallWindowPane(meidoManager),
                    [Constants.Window.Pose] = new PoseWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.Face] = new FaceWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.BG] = new BGWindowPane(environmentManager, lightManager, effectManager),
                    [Constants.Window.BG2] = new BG2WindowPane(meidoManager, propManager)
                },
                [Constants.Window.Message] = new MessageWindow(messageWindowManager)
            };

            meidoManager.BeginCallMeidos += (s, a) => uiActive = false;
            meidoManager.EndCallMeidos += (s, a) => uiActive = true;
        }

        private void Activate()
        {
            if (!initialized) Initialize();
            uiActive = true;
            isActive = true;

            meidoManager.Activate();
            environmentManager.Activate();
            propManager.Activate();
            lightManager.Activate();
            effectManager.Activate();
            windowManager.Activate();
            messageWindowManager.Activate();

            GameObject dailyPanel = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            dailyPanel.SetActive(false);
        }

        private void Deactivate()
        {
            if (meidoManager.IsBusy) return;

            uiActive = false;
            isActive = false;

            meidoManager.Deactivate();
            environmentManager.Deactivate();
            propManager.Deactivate();
            lightManager.Deactivate();
            effectManager.Deactivate();
            messageWindowManager.Deactivate();
            windowManager.Deactivate();

            GameObject dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;
            dailyPanel?.SetActive(true);
        }
    }
}
