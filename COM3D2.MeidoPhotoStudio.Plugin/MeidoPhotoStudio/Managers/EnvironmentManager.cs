using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EnvironmentManager
    {
        private static bool cubeActive;
        public static bool CubeActive
        {
            get => cubeActive;
            set
            {
                if (value != cubeActive)
                {
                    cubeActive = value;
                    CubeActiveChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static bool cubeSmall;
        public static bool CubeSmall
        {
            get => cubeSmall;
            set
            {
                if (value != cubeSmall)
                {
                    cubeSmall = value;
                    CubeSmallChange?.Invoke(null, EventArgs.Empty);
                }
            }
        }
        private static event EventHandler CubeActiveChange;
        private static event EventHandler CubeSmallChange;
        private GameObject cameraObject;
        private Camera subCamera;
        private GameObject bgObject;
        private Transform bg;
        private CameraInfo cameraInfo;
        private DragDogu bgDragPoint;
        public LightManager LightManager { get; }
        public PropManager PropManager { get; }
        public EffectManager EffectManager { get; }
        private bool bgVisible = true;
        public bool BGVisible
        {
            get => bgVisible;
            set
            {
                this.bgVisible = value;
                bgObject.SetActive(this.bgVisible);
            }
        }
        private DragType currentDragType = DragType.None;
        private DragType dragTypeOld = DragType.None;
        private enum DragType
        {
            None, Transform
        }

        public EnvironmentManager(MeidoManager meidoManager)
        {
            PropManager = new PropManager(meidoManager);
            LightManager = new LightManager();
            EffectManager = new EffectManager();
        }

        public void Activate()
        {
            bgObject = GameObject.Find("__GameMain__/BG");
            bg = bgObject.transform;

            GameObject dragPoint = BaseDrag.MakeDragPoint(
                PrimitiveType.Cube, Vector3.one * 0.12f, BaseDrag.LightBlue
            );

            bgDragPoint = dragPoint.AddComponent<DragDogu>();
            bgDragPoint.Initialize(bgObject, true);
            bgDragPoint.SetDragProp(false, false, false);

            cameraObject = new GameObject("subCamera");
            subCamera = cameraObject.AddComponent<Camera>();
            subCamera.CopyFrom(Camera.main);
            subCamera.clearFlags = CameraClearFlags.Depth;
            subCamera.cullingMask = 256;
            subCamera.depth = 1f;
            subCamera.transform.parent = GameMain.Instance.MainCamera.transform;

            cameraObject.SetActive(true);

            bgObject.SetActive(true);
            GameMain.Instance.BgMgr.ChangeBg("Theater");

            UltimateOrbitCamera UOCamera =
                Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(GameMain.Instance.MainCamera, "m_UOCamera");
            UOCamera.enabled = true;

            ResetCamera();
            SaveCameraInfo();

            PropManager.Activate();
            LightManager.Activate();
            EffectManager.Activate();

            CubeSmallChange += OnCubeSmall;
        }

        public void Deactivate()
        {
            if (bgDragPoint != null) GameObject.Destroy(bgDragPoint.gameObject);
            GameObject.Destroy(cameraObject);
            GameObject.Destroy(subCamera);

            PropManager.Deactivate();
            LightManager.Deactivate();
            EffectManager.Deactivate();

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
            CubeSmallChange -= OnCubeSmall;
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.Q))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    SaveCameraInfo();
                }

                if (Input.GetKeyDown(KeyCode.A))
                {
                    LoadCameraInfo(cameraInfo);
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    ResetCamera();
                }
            }

            if (CubeActive && (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C)))
            {
                currentDragType = DragType.Transform;
            }
            else
            {
                currentDragType = DragType.None;
            }

            if (currentDragType != dragTypeOld)
            {
                bool visible = currentDragType == DragType.Transform;
                this.bgDragPoint.SetDragProp(visible, visible, visible);
            }

            dragTypeOld = currentDragType;

            PropManager.Update();
            LightManager.Update();
            EffectManager.Update();
        }

        public void ChangeBackground(string assetName, bool creative = false)
        {
            if (creative)
            {
                GameMain.Instance.BgMgr.ChangeBgMyRoom(assetName);
            }
            else
            {
                GameMain.Instance.BgMgr.ChangeBg(assetName);
                if (assetName == "KaraokeRoom")
                {
                    bg.transform.position = bgObject.transform.position;
                    bg.transform.localPosition = new Vector3(1f, 0f, 4f);
                    bg.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
                }
            }
        }

        private void SaveCameraInfo()
        {
            this.cameraInfo = new CameraInfo(GameMain.Instance.MainCamera);
        }

        public void LoadCameraInfo(CameraInfo cameraInfo)
        {
            CameraMain camera = GameMain.Instance.MainCamera;
            camera.SetTargetPos(cameraInfo.TargetPos);
            camera.SetPos(cameraInfo.Pos);
            camera.SetDistance(cameraInfo.Distance);
            camera.transform.eulerAngles = cameraInfo.Angle;
        }

        private void ResetCamera()
        {
            CameraMain cameraMain = GameMain.Instance.MainCamera;
            cameraMain.Reset(CameraMain.CameraType.Target, true);
            cameraMain.SetTargetPos(new Vector3(0f, 0.9f, 0f), true);
            cameraMain.SetDistance(3f, true);
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            this.bgDragPoint.DragPointScale = this.bgDragPoint.BaseScale * (CubeSmall ? 0.4f : 1f);
        }
    }

    public struct CameraInfo
    {
        public Vector3 TargetPos { get; }
        public Vector3 Pos { get; }
        public Vector3 Angle { get; }
        public float Distance { get; }
        public CameraInfo(CameraMain camera)
        {
            this.TargetPos = camera.GetTargetPos();
            this.Pos = camera.GetPos();
            this.Angle = camera.transform.eulerAngles;
            this.Distance = camera.GetDistance();
        }
    }
}
