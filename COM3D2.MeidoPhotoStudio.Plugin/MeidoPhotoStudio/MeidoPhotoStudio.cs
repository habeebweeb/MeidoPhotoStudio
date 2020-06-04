using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityInjector;
using UnityInjector.Attributes;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    [PluginName("Meido Photo Studio"), PluginVersion("0.0.0")]
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
                    if (!initialized)
                    {
                        Initialize();
                        windowManager.Visible = true;
                    }
                    else
                    {
                        ReturnToMenu();
                    }
                }

                if (isActive)
                {
                    meidoManager.Update();
                    windowManager.Update();
                }
            }
        }
        private void OnGUI()
        {
            if (isActive)
            {
                windowManager.OnGUI();
            }
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            currentScene = (Constants.Scene)scene.buildIndex;

            // if (currentScene == Constants.Scene.Daily)
            // {
            //     if (initialized)
            //     {

            //     }
            // }
            // else
            // {
            //     if (initialized)
            //     {
            //         initialized = false;
            //     }
            // }
        }
        private void ReturnToMenu()
        {
            if (meidoManager.IsBusy) return;
            meidoManager.DeactivateMeidos();
            environmentManager.Deactivate();
            messageWindowManager.Deactivate();

            isActive = false;
            initialized = false;
            windowManager.Visible = false;
            GameMain.Instance.SoundMgr.PlayBGM("bgm009.ogg", 1f, true);
            GameObject go = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            go.SetActive(true);
            bool isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") == 3;

            if (isNight)
            {
                GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot_Night");
            }
            else
            {
                GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot");
            }

            GameMain.Instance.MainCamera.Reset(CameraMain.CameraType.Target, true);
            GameMain.Instance.MainCamera.SetTargetPos(new Vector3(0.5609447f, 1.380762f, -1.382336f), true);
            GameMain.Instance.MainCamera.SetDistance(1.6f, true);
            GameMain.Instance.MainCamera.SetAroundAngle(new Vector2(245.5691f, 6.273283f), true);
        }

        private void Initialize()
        {
            initialized = true;
            meidoManager = new MeidoManager();
            environmentManager = new EnvironmentManager();
            messageWindowManager = new MessageWindowManager();
            windowManager = new WindowManager(meidoManager, environmentManager, messageWindowManager);

            environmentManager.Initialize();

            isActive = true;

            #region maid stuff
            // if (maid)
            // {
            //     maid.StopKuchipakuPattern();
            //     maid.body0.trsLookTarget = GameMain.Instance.MainCamera.transform;

            //     if (maid.Visible && maid.body0.isLoadedBody)
            //     {
            //         maid.CrossFade("pose_taiki_f.anm", false, true, false, 0f);
            //         maid.SetAutoTwistAll(true);
            //         maid.body0.MuneYureL(1f);
            //         maid.body0.MuneYureR(1f);
            //         maid.body0.jbMuneL.enabled = true;
            //         maid.body0.jbMuneR.enabled = true;
            //     }

            //     maid.body0.SetMask(TBody.SlotID.wear, true);
            //     maid.body0.SetMask(TBody.SlotID.skirt, true);
            //     maid.body0.SetMask(TBody.SlotID.bra, true);
            //     maid.body0.SetMask(TBody.SlotID.panz, true);
            //     maid.body0.SetMask(TBody.SlotID.mizugi, true);
            //     maid.body0.SetMask(TBody.SlotID.onepiece, true);
            //     if (maid.body0.isLoadedBody)
            //     {
            //         for (int i = 0; i < maid.body0.goSlot.Count; i++)
            //         {
            //             List<THair1> fieldValue = Utility.GetFieldValue<TBoneHair_, List<THair1>>(maid.body0.goSlot[i].bonehair, "hair1list");
            //             for (int j = 0; j < fieldValue.Count; ++j)
            //             {
            //                 fieldValue[j].SoftG = new Vector3(0.0f, -3f / 1000f, 0.0f);
            //             }
            //         }
            //     }
            // }
            #endregion

            GameObject dailyPanel = GameObject.Find("UI Root").transform.Find("DailyPanel").gameObject;
            dailyPanel.SetActive(false);
        }
    }
}
