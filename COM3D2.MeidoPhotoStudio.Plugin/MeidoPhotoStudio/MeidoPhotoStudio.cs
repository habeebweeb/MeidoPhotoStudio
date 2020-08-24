using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Ionic.Zlib;
using BepInEx;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MeidoPhotoStudio : BaseUnityPlugin
    {
        private static CameraMain mainCamera = GameMain.Instance.MainCamera;
        private static event EventHandler<ScreenshotEventArgs> ScreenshotEvent;
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio";
        public const string pluginName = "MeidoPhotoStudio";
        public const string pluginVersion = "0.0.0";
        public const int sceneVersion = 1000;
        public static string pluginString = $"{pluginName} {pluginVersion}";
        private WindowManager windowManager;
        private MeidoManager meidoManager;
        private EnvironmentManager environmentManager;
        private MessageWindowManager messageWindowManager;
        private LightManager lightManager;
        private PropManager propManager;
        private EffectManager effectManager;
        private Constants.Scene currentScene;
        private bool initialized = false;
        private bool active = false;
        private bool uiActive = false;

        private void Awake()
        {
            ScreenshotEvent += OnScreenshotEvent;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            Constants.Initialize();
            Translation.Initialize(Configuration.CurrentLanguage);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Serialize(bool quickSave)
        {
            string sceneName = quickSave ? "mpstempscene" : $"mpsscene{System.DateTime.Now:yyyyMMddHHmmss}.scene";
            string scenePath = Path.Combine(Constants.scenesPath, sceneName);
            File.WriteAllBytes(scenePath, Serialize());
        }

        public byte[] Serialize()
        {
            if (meidoManager.Busy) return null;

            MemoryStream memoryStream;

            using (memoryStream = new MemoryStream())
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            using (BinaryWriter binaryWriter = new BinaryWriter(deflateStream, System.Text.Encoding.UTF8))
            {
                binaryWriter.Write("MPS_SCENE");
                binaryWriter.Write(sceneVersion);

                messageWindowManager.Serialize(binaryWriter);
                effectManager.Serialize(binaryWriter);
                environmentManager.Serialize(binaryWriter);
                lightManager.Serialize(binaryWriter);
                // meidomanager has to be serialized before propmanager because reattached props will be in the wrong
                // place after a maid's pose is deserialized.
                meidoManager.Serialize(binaryWriter);
                propManager.Serialize(binaryWriter);

                binaryWriter.Write("END");
            }

            return memoryStream.ToArray();
        }

        public void Deserialize()
        {
            string path = Path.Combine(Constants.scenesPath, "mpstempscene");
            Deserialize(path);
        }

        public void Deserialize(string filePath)
        {
            if (meidoManager.Busy) return;

            byte[] data = DeflateStream.UncompressBuffer(File.ReadAllBytes(filePath));

            using (MemoryStream memoryStream = new MemoryStream(data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream, System.Text.Encoding.UTF8))
            {
                try
                {
                    if (binaryReader.ReadString() != "MPS_SCENE") return;

                    if (binaryReader.ReadInt32() > sceneVersion)
                    {
                        Utility.LogWarning($"'{filePath}' is made in a newer version of {pluginName}");
                        return;
                    }

                    string previousHeader = string.Empty;
                    string header;

                    while ((header = binaryReader.ReadString()) != "END")
                    {
                        switch (header)
                        {
                            case MessageWindowManager.header:
                                messageWindowManager.Deserialize(binaryReader);
                                break;
                            case EnvironmentManager.header:
                                environmentManager.Deserialize(binaryReader);
                                break;
                            case MeidoManager.header:
                                meidoManager.Deserialize(binaryReader);
                                break;
                            case PropManager.header:
                                propManager.Deserialize(binaryReader);
                                break;
                            case LightManager.header:
                                lightManager.Deserialize(binaryReader);
                                break;
                            case EffectManager.header:
                                effectManager.Deserialize(binaryReader);
                                break;
                            default: throw new System.Exception($"Unknown header '{header}'. Last {previousHeader}");
                        }
                        previousHeader = header;
                    }
                }
                catch (System.Exception e)
                {
                    Utility.LogError($"Failed to deserialize scene '{filePath}' because {e.Message}");
                    return;
                }
            }
        }

        public static void TakeScreenshot(ScreenshotEventArgs args) => ScreenshotEvent?.Invoke(null, args);

        public static void TakeScreenshot(string path = "", int superSize = -1, bool hideMaids = false)
        {
            TakeScreenshot(new ScreenshotEventArgs() { Path = path, SuperSize = superSize, HideMaids = hideMaids });
        }

        private void OnScreenshotEvent(object sender, ScreenshotEventArgs args)
        {
            StartCoroutine(Screenshot(args));
        }

        private void Update()
        {
            if (currentScene == Constants.Scene.Daily)
            {
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    if (active) Deactivate();
                    else Activate();
                }

                if (active)
                {
                    if (Utility.GetModKey(Utility.ModKey.Control))
                    {
                        if (Input.GetKeyDown(KeyCode.S)) Serialize(true);
                        else if (Input.GetKeyDown(KeyCode.A)) Deserialize();
                    }
                    else if (!Input.GetKey(KeyCode.Q) && Input.GetKeyDown(KeyCode.S))
                    {
                        TakeScreenshot();
                    }

                    meidoManager.Update();
                    environmentManager.Update();
                    windowManager.Update();
                    effectManager.Update();
                }
            }
        }

        private IEnumerator Screenshot(ScreenshotEventArgs args)
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

            // TODO: Hide cubes for bg, maid, lights etc.

            List<Meido> activeMeidoList = this.meidoManager.ActiveMeidoList;
            bool[] isIK = new bool[activeMeidoList.Count];
            bool[] isVisible = new bool[activeMeidoList.Count];
            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                isIK[i] = meido.IK;
                isVisible[i] = meido.Maid.Visible;
                if (meido.IK) meido.IK = false;
                if (args.HideMaids) meido.Maid.Visible = false;
            }

            GizmoRender.UIVisible = false;

            yield return new WaitForEndOfFrame();

            // Take Screenshot
            int[] defaultSuperSize = new[] { 1, 2, 4 };
            int selectedSuperSize = args.SuperSize < 1
                ? defaultSuperSize[(int)GameMain.Instance.CMSystem.ScreenShotSuperSize]
                : args.SuperSize;

            string path = string.IsNullOrEmpty(args.Path)
                ? Utility.ScreenshotFilename()
                : args.Path;

            Application.CaptureScreenshot(path, selectedSuperSize);
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
                if (isIK[i]) meido.IK = true;
                if (args.HideMaids && isVisible[i]) meido.Maid.Visible = true;
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
            if (active) Deactivate();
            ResetCalcNearClip();
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

            SetNearClipPlane();

            uiActive = true;
            active = true;

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
            if (meidoManager.Busy) return;

            ResetCalcNearClip();

            uiActive = false;
            active = false;

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

        private void SetNearClipPlane()
        {
            mainCamera.StopAllCoroutines();
            mainCamera.m_bCalcNearClip = false;
            mainCamera.camera.nearClipPlane = 0.01f;
        }

        private void ResetCalcNearClip()
        {
            if (mainCamera.m_bCalcNearClip) return;
            mainCamera.StopAllCoroutines();
            mainCamera.m_bCalcNearClip = true;
            mainCamera.Start();
        }
    }

    public class ScreenshotEventArgs : EventArgs
    {
        public string Path { get; set; } = string.Empty;
        public int SuperSize { get; set; } = -1;
        public bool HideMaids { get; set; } = false;
    }
}
