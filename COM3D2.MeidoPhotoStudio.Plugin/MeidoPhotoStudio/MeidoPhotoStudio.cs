using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    [BepInDependency("org.bepinex.plugins.unityinjectorloader", BepInDependency.DependencyFlags.SoftDependency)]
    public class MeidoPhotoStudio : BaseUnityPlugin
    {
        public static readonly byte[] SceneHeader = Encoding.UTF8.GetBytes("MPSSCENE");
        private static event EventHandler<ScreenshotEventArgs> ScreenshotEvent;
        private const string pluginGuid = "com.habeebweeb.com3d2.meidophotostudio";
        public const string pluginName = "MeidoPhotoStudio";
        public const string pluginVersion = "1.0.0";
        public const string pluginSubVersion = "beta.3";
        public const short sceneVersion = 1;
        public const int kankyoMagic = -765;
        public static readonly string pluginString = $"{pluginName} {pluginVersion}";
        public static bool EditMode => currentScene == Constants.Scene.Edit;
        public static event EventHandler<ScreenshotEventArgs> NotifyRawScreenshot;
        private WindowManager windowManager;
        private SceneManager sceneManager;
        private MeidoManager meidoManager;
        private EnvironmentManager environmentManager;
        private MessageWindowManager messageWindowManager;
        private LightManager lightManager;
        private PropManager propManager;
        private EffectManager effectManager;
        private CameraManager cameraManager;
        private static Constants.Scene currentScene;
        private bool initialized;
        private bool active;
        private bool uiActive;

        static MeidoPhotoStudio()
        {
            Input.Register(MpsKey.Screenshot, KeyCode.S, "Take screenshot");
            Input.Register(MpsKey.Activate, KeyCode.F6, "Activate/deactivate MeidoPhotoStudio");

            if (!string.IsNullOrEmpty(pluginSubVersion)) pluginString += $"-{pluginSubVersion}";
        }

        private void Awake()
        {
            var harmony = HarmonyLib.Harmony.CreateAndPatchAll(typeof(AllProcPropSeqStartPatcher));
            harmony.PatchAll(typeof(BgMgrPatcher));
            ScreenshotEvent += OnScreenshotEvent;
            DontDestroyOnLoad(this);
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void Start()
        {
            Constants.Initialize();
            Translation.Initialize(Translation.CurrentLanguage);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            currentScene = (Constants.Scene)scene.buildIndex;
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            if (active) Deactivate(true);
            CameraUtility.MainCamera.ResetCalcNearClip();
        }

        public byte[] SaveScene(bool environment = false)
        {
            if (meidoManager.Busy) return null;

            try
            {
                using var memoryStream = new MemoryStream();
                using var headerWriter = new BinaryWriter(memoryStream, Encoding.UTF8);

                headerWriter.Write(SceneHeader);

                new SceneMetadata
                {
                    Version = sceneVersion,
                    Environment = environment,
                    MaidCount = environment ? kankyoMagic : meidoManager.ActiveMeidoList.Count
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

            using var uncompressed = memoryStream.Decompress();
            using var dataReader = new BinaryReader(uncompressed, Encoding.UTF8);

            var header = string.Empty;
            var previousHeader = string.Empty;
            try
            {
                while ((header = dataReader.ReadString()) != "END")
                {
                    switch (header)
                    {
                        case MeidoManager.header: 
                            Serialization.Get<MeidoManager>().Deserialize(meidoManager, dataReader, metadata);
                            break;
                        case MessageWindowManager.header:
                            Serialization.Get<MessageWindowManager>().Deserialize(messageWindowManager, dataReader, metadata);
                            break;
                        case CameraManager.header:
                            Serialization.Get<CameraManager>().Deserialize(cameraManager, dataReader, metadata);
                            break;
                        case LightManager.header:
                            Serialization.Get<LightManager>().Deserialize(lightManager, dataReader, metadata);
                            break;
                        case EffectManager.header:
                            Serialization.Get<EffectManager>().Deserialize(effectManager, dataReader, metadata);
                            break;
                        case EnvironmentManager.header:
                            Serialization.Get<EnvironmentManager>().Deserialize(environmentManager, dataReader, metadata);
                            break;
                        case PropManager.header:
                            Serialization.Get<PropManager>().Deserialize(propManager, dataReader, metadata);
                            break;
                        default: throw new Exception($"Unknown header '{header}'");
                    }

                    previousHeader = header;
                }
            }
            catch (Exception e)
            {
                Utility.LogError(
                    $"Failed to deserialize scene TEST because {e.Message}"
                    + $"\nCurrent header: '{header}'. Last header: '{previousHeader}'"
                );
                Utility.LogError(e.StackTrace);
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
            if (currentScene == Constants.Scene.Daily || currentScene == Constants.Scene.Edit)
            {
                if (Input.GetKeyDown(MpsKey.Activate))
                {
                    if (active) Deactivate();
                    else Activate();
                }

                if (active)
                {
                    if (!Input.Control && !Input.GetKey(MpsKey.CameraLayer) && Input.GetKeyDown(MpsKey.Screenshot))
                    {
                        TakeScreenshot();
                    }

                    meidoManager.Update();
                    cameraManager.Update();
                    windowManager.Update();
                    effectManager.Update();
                    sceneManager.Update();
                }
            }
        }

        private IEnumerator Screenshot(ScreenshotEventArgs args)
        {
            // Hide UI and dragpoints
            GameObject gameMain = GameMain.Instance.gameObject;
            GameObject editUI = UTY.GetChildObject(GameObject.Find("UI Root"), "Camera");
            GameObject fpsViewer = UTY.GetChildObject(gameMain, "SystemUI Root/FpsCounter");
            GameObject sysDialog = UTY.GetChildObject(gameMain, "SystemUI Root/SystemDialog");
            GameObject sysShortcut = UTY.GetChildObject(gameMain, "SystemUI Root/SystemShortcut");

            // CameraUtility can hide the edit UI so keep its state for later
            bool editUIWasActive = editUI.activeSelf;

            uiActive = false;
            editUI.SetActive(false);
            fpsViewer.SetActive(false);
            sysDialog.SetActive(false);
            sysShortcut.SetActive(false);

            // Hide maid dragpoints and maids
            List<Meido> activeMeidoList = meidoManager.ActiveMeidoList;
            bool[] isIK = new bool[activeMeidoList.Count];
            bool[] isVisible = new bool[activeMeidoList.Count];
            for (int i = 0; i < activeMeidoList.Count; i++)
            {
                Meido meido = activeMeidoList[i];
                isIK[i] = meido.IK;
                if (meido.IK) meido.IK = false;

                // Hide the maid if needed
                if (args.HideMaids)
                {
                    isVisible[i] = meido.Maid.Visible;
                    meido.Maid.Visible = false;
                }
            }

            // Hide other drag points
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

            // hide gizmos
            GizmoRender.UIVisible = false;

            yield return new WaitForEndOfFrame();

            Texture2D rawScreenshot = null;

            if (args.InMemory)
            {
                // Take a screenshot directly to a Texture2D for immediate processing
                RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
                RenderTexture.active = renderTexture;
                CameraUtility.MainCamera.camera.targetTexture = renderTexture;
                CameraUtility.MainCamera.camera.Render();

                rawScreenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                rawScreenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0, false);
                rawScreenshot.Apply();

                CameraUtility.MainCamera.camera.targetTexture = null;
                RenderTexture.active = null;
                DestroyImmediate(renderTexture);
            }
            else
            {
                // Take Screenshot
                int[] defaultSuperSize = new[] { 1, 2, 4 };
                int selectedSuperSize = args.SuperSize < 1
                    ? defaultSuperSize[(int)GameMain.Instance.CMSystem.ScreenShotSuperSize]
                    : args.SuperSize;

                string path = string.IsNullOrEmpty(args.Path)
                    ? Utility.ScreenshotFilename()
                    : args.Path;

                Application.CaptureScreenshot(path, selectedSuperSize);
            }
            GameMain.Instance.SoundMgr.PlaySe("se022.ogg", false);

            yield return new WaitForEndOfFrame();

            // Show UI and dragpoints
            uiActive = true;
            editUI.SetActive(editUIWasActive);
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

            if (args.InMemory && rawScreenshot)
                NotifyRawScreenshot?.Invoke(null, new ScreenshotEventArgs() { Screenshot = rawScreenshot });
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

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;

            meidoManager = new MeidoManager();
            environmentManager = new EnvironmentManager();
            messageWindowManager = new MessageWindowManager();
            lightManager = new LightManager();
            propManager = new PropManager(meidoManager);
            sceneManager = new SceneManager(this);
            cameraManager = new CameraManager();

            effectManager = new EffectManager();
            effectManager.AddManager<BloomEffectManager>();
            effectManager.AddManager<DepthOfFieldEffectManager>();
            effectManager.AddManager<FogEffectManager>();
            effectManager.AddManager<VignetteEffectManager>();
            effectManager.AddManager<SepiaToneEffectManger>();
            effectManager.AddManager<BlurEffectManager>();

            meidoManager.BeginCallMeidos += (s, a) => uiActive = false;
            meidoManager.EndCallMeidos += (s, a) => uiActive = true;

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
                        environmentManager, lightManager, effectManager, sceneWindow, cameraManager
                    ),
                    [Constants.Window.BG2] = new BG2WindowPane(meidoManager, propManager),
                    [Constants.Window.Settings] = new SettingsWindowPane()
                },
                [Constants.Window.Message] = new MessageWindow(messageWindowManager),
                [Constants.Window.Save] = sceneWindow
            };
        }

        private void Activate()
        {
            if (!GameMain.Instance.SysDlg.IsDecided) return;

            if (!initialized) Initialize();
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

            uiActive = true;
            active = true;

            if (!EditMode)
            {
                GameObject dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;
                if (dailyPanel) dailyPanel.SetActive(false);
            }
            else meidoManager.CallMeidos();
        }

        private void Deactivate(bool force = false)
        {
            if (meidoManager.Busy || SceneManager.Busy) return;

            SystemDialog sysDialog = GameMain.Instance.SysDlg;

            void exit()
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

                if (!EditMode)
                {
                    GameObject dailyPanel = GameObject.Find("UI Root")?.transform.Find("DailyPanel")?.gameObject;
                    dailyPanel?.SetActive(true);
                }

                Configuration.Config.Save();
            }

            if (sysDialog.IsDecided || EditMode || force)
            {
                uiActive = false;
                active = false;

                if (EditMode || force) exit();
                else
                {
                    string exitMessage = string.Format(Translation.Get("systemMessage", "exitConfirm"), pluginName);
                    sysDialog.Show(exitMessage, SystemDialog.TYPE.OK_CANCEL,
                        f_dgOk: exit,
                        f_dgCancel: () =>
                        {
                            sysDialog.Close();
                            uiActive = true;
                            active = true;
                        }
                    );
                }
            }
        }
    }

    public class ScreenshotEventArgs : EventArgs
    {
        public string Path { get; set; } = string.Empty;
        public int SuperSize { get; set; } = -1;
        public bool HideMaids { get; set; }
        public bool InMemory { get; set; } = false;
        public Texture2D Screenshot { get; set; }
    }
}
