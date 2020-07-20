using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [PluginName("COM3D2.MeidoPhotoStudio.Plugin"), PluginVersion("0.0.0")]
    public class MeidoPhotoStudio : PluginBase
    {
        private static MonoBehaviour instance;
        private WindowManager windowManager;
        private MeidoManager meidoManager;
        private EnvironmentManager environmentManager;
        private MessageWindowManager messageWindowManager;
        private Constants.Scene currentScene;
        private bool initialized = false;
        private bool isActive = false;
        private bool uiActive = false;
        private MeidoPhotoStudio()
        {
            MeidoPhotoStudio.instance = this;
        }
        private void Awake()
        {
            DontDestroyOnLoad(this);
            Translation.Initialize("en");
            Constants.Initialize();
        }
        private void Start()
        {
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

            if (currentScene == Constants.Scene.Daily)
            {
                if (!initialized) Initialize();
            }
        }

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;

            meidoManager = new MeidoManager();
            environmentManager = new EnvironmentManager(meidoManager);
            messageWindowManager = new MessageWindowManager();

            MaidSwitcherPane maidSwitcherPane = new MaidSwitcherPane(meidoManager);

            windowManager = new WindowManager()
            {
                [Constants.Window.Main] = new MainWindow(meidoManager)
                {
                    [Constants.Window.Call] = new CallWindowPane(meidoManager),
                    [Constants.Window.Pose] = new PoseWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.Face] = new FaceWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.BG] = new BGWindowPane(environmentManager),
                    [Constants.Window.BG2] = new BG2WindowPane(meidoManager, environmentManager)
                },
                [Constants.Window.Message] = new MessageWindow(messageWindowManager)
            };

            meidoManager.BeginCallMeidos += (s, a) => uiActive = false;
            meidoManager.EndCallMeidos += (s, a) => uiActive = true;
        }

        private void Activate()
        {
            uiActive = true;
            isActive = true;

            meidoManager.Activate();
            environmentManager.Activate();
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
            messageWindowManager.Deactivate();
            windowManager.Deactivate();

            // GameMain.Instance.SoundMgr.PlayBGM("bgm009.ogg", 1f, true);
            GameObject dailyPanel = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            dailyPanel.SetActive(true);
        }
    }
}
