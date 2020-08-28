using System;
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
        public const int sceneVersion = 1100;
        public const int kankyoMagic = -765;
        public static string pluginString = $"{pluginName} {pluginVersion}";
        private WindowManager windowManager;
        private SceneManager sceneManager;
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
            Translation.Initialize(Translation.CurrentLanguage);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public byte[] SerializeScene(bool kankyo = false)
        {
            if (meidoManager.Busy) return null;

            byte[] compressedData;

            using (MemoryStream memoryStream = new MemoryStream())
            using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
            using (BinaryWriter binaryWriter = new BinaryWriter(deflateStream, System.Text.Encoding.UTF8))
            {
                binaryWriter.Write("MPS_SCENE");
                binaryWriter.Write(sceneVersion);

                binaryWriter.Write(kankyo ? kankyoMagic : meidoManager.ActiveMeidoList.Count);

                effectManager.Serialize(binaryWriter);
                environmentManager.Serialize(binaryWriter, kankyo);
                lightManager.Serialize(binaryWriter);

                if (!kankyo)
                {
                    messageWindowManager.Serialize(binaryWriter);
                    // meidomanager has to be serialized before propmanager because reattached props will be in the 
                    // wrong place after a maid's pose is deserialized.
                    meidoManager.Serialize(binaryWriter);
                }

                propManager.Serialize(binaryWriter);

                binaryWriter.Write("END");

                deflateStream.Close();

                compressedData = memoryStream.ToArray();
            }

            return compressedData;
        }

        public static byte[] DecompressScene(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Utility.LogWarning($"Scene file '{filePath}' does not exist.");
                return null;
            }

            byte[] compressedData;

            using (FileStream fileStream = File.OpenRead(filePath))
            {
                if (Utility.IsPngFile(fileStream))
                {
                    if (!Utility.SeekPngEnd(fileStream))
                    {
                        Utility.LogWarning($"'{filePath}' is not a PNG file");
                        return null;
                    }

                    if (fileStream.Position == fileStream.Length)
                    {
                        Utility.LogWarning($"'{filePath}' contains no scene data");
                        return null;
                    }

                    int dataLength = (int)(fileStream.Length - fileStream.Position);

                    compressedData = new byte[dataLength];

                    fileStream.Read(compressedData, 0, dataLength);
                }
                else
                {
                    compressedData = new byte[fileStream.Length];
                    fileStream.Read(compressedData, 0, compressedData.Length);
                }
            }

            return DeflateStream.UncompressBuffer(compressedData);
        }

        public void ApplyScene(string filePath)
        {
            if (meidoManager.Busy) return;

            byte[] sceneBinary = DecompressScene(filePath);

            if (sceneBinary == null) return;

            using (MemoryStream memoryStream = new MemoryStream(sceneBinary))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream, System.Text.Encoding.UTF8))
            {
                try
                {
                    if (binaryReader.ReadString() != "MPS_SCENE")
                    {
                        Utility.LogWarning($"'{filePath}' is not a {pluginName} scene");
                        return;
                    }

                    if (binaryReader.ReadInt32() > sceneVersion)
                    {
                        Utility.LogWarning($"'{filePath}' is made in a newer version of {pluginName}");
                        return;
                    }

                    binaryReader.ReadInt32(); // Number of Maids

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
                            default: throw new Exception($"Unknown header '{header}'. Last {previousHeader}");
                        }
                        previousHeader = header;
                    }
                }
                catch (Exception e)
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
                    if (!Input.GetKey(KeyCode.Q) && !Utility.GetModKey(Utility.ModKey.Control)
                        && Input.GetKeyDown(KeyCode.S)
                    ) TakeScreenshot();

                    meidoManager.Update();
                    environmentManager.Update();
                    windowManager.Update();
                    effectManager.Update();
                    sceneManager.Update();
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

            bool[] isCubeActive = {
                MeidoDragPointManager.CubeActive,
                PropManager.CubeActive,
                LightManager.CubeActive,
                EnvironmentManager.CubeActive
            };

            MeidoDragPointManager.CubeActive = false;
            PropManager.CubeActive = false;
            LightManager.CubeActive = false;
            EnvironmentManager.CubeActive = false;

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

            MeidoDragPointManager.CubeActive = isCubeActive[0];
            PropManager.CubeActive = isCubeActive[1];
            LightManager.CubeActive = isCubeActive[2];
            EnvironmentManager.CubeActive = isCubeActive[3];

            GizmoRender.UIVisible = true;
        }

        private void OnGUI()
        {
            if (uiActive)
            {
                windowManager.DrawWindows();

                if (DropdownHelper.Visible) DropdownHelper.HandleDropdown();
                if (Modal.Visible) Modal.Draw();
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
            sceneManager = new SceneManager(this);

            MaidSwitcherPane maidSwitcherPane = new MaidSwitcherPane(meidoManager);

            SceneWindow sceneWindow = new SceneWindow(sceneManager);

            windowManager = new WindowManager()
            {
                [Constants.Window.Main] = new MainWindow(meidoManager, propManager, lightManager)
                {
                    [Constants.Window.Call] = new CallWindowPane(meidoManager),
                    [Constants.Window.Pose] = new PoseWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.Face] = new FaceWindowPane(meidoManager, maidSwitcherPane),
                    [Constants.Window.BG] = new BGWindowPane(
                        environmentManager, lightManager, effectManager, sceneWindow
                    ),
                    [Constants.Window.BG2] = new BG2WindowPane(meidoManager, propManager)
                },
                [Constants.Window.Message] = new MessageWindow(messageWindowManager),
                [Constants.Window.Save] = sceneWindow
            };

            meidoManager.BeginCallMeidos += (s, a) => uiActive = false;
            meidoManager.EndCallMeidos += (s, a) => uiActive = true;
        }

        private void Activate()
        {
            if (!GameMain.Instance.SysDlg.IsDecided) return;

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
            if (meidoManager.Busy || SceneManager.Busy) return;

            SystemDialog sysDialog = GameMain.Instance.SysDlg;

            if (sysDialog.IsDecided)
            {
                uiActive = false;
                active = false;
                string exitMessage = string.Format(Translation.Get("systemMessage", "exitConfirm"), pluginName);
                sysDialog.Show(exitMessage, SystemDialog.TYPE.OK_CANCEL,
                    f_dgOk: () =>
                    {
                        sysDialog.Close();
                        ResetCalcNearClip();

                        meidoManager.Deactivate();
                        environmentManager.Deactivate();
                        propManager.Deactivate();
                        lightManager.Deactivate();
                        effectManager.Deactivate();
                        messageWindowManager.Deactivate();
                        windowManager.Deactivate();

                        Modal.Close();

                        GameObject dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;
                        dailyPanel?.SetActive(true);
                        Configuration.Config.Save();
                    },
                    f_dgCancel: () =>
                    {
                        sysDialog.Close();
                        uiActive = true;
                        active = true;
                    }
                );
            }
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
