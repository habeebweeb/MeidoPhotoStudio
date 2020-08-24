using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class EnvironmentManager : IManager, ISerializable
    {
        public const string header = "ENVIRONMENT";
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
        private DragPointBG bgDragPoint;
        private string currentBGAsset = "Theater";
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

        public EnvironmentManager(MeidoManager meidoManager)
        {
            DragPointLight.environmentManager = this;
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter) => Serialize(binaryWriter, false);

        public void Serialize(System.IO.BinaryWriter binaryWriter, bool kankyo)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(currentBGAsset);
            binaryWriter.WriteVector3(this.bg.position);
            binaryWriter.WriteQuaternion(this.bg.rotation);
            binaryWriter.WriteVector3(this.bg.localScale);

            binaryWriter.Write(kankyo);

            CameraMain camera = GameMain.Instance.MainCamera;
            binaryWriter.WriteVector3(camera.GetTargetPos());
            binaryWriter.Write(camera.GetDistance());
            binaryWriter.WriteQuaternion(camera.transform.rotation);
            StopCameraSpin();
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            string bgAsset = binaryReader.ReadString();
            int assetIndex = Constants.BGList.FindIndex(bg => bg == bgAsset);
            ChangeBackground(bgAsset, assetIndex > Constants.MyRoomCustomBGIndex);
            this.bg.position = binaryReader.ReadVector3();
            this.bg.rotation = binaryReader.ReadQuaternion();
            this.bg.localScale = binaryReader.ReadVector3();

            bool kankyo = binaryReader.ReadBoolean();

            Vector3 cameraPosition = binaryReader.ReadVector3();
            float cameraDistance = binaryReader.ReadSingle();
            Quaternion cameraRotation = binaryReader.ReadQuaternion();

            if (!kankyo)
            {
                CameraMain camera = GameMain.Instance.MainCamera;
                camera.SetTargetPos(cameraPosition);
                camera.SetDistance(cameraDistance);
                camera.transform.rotation = cameraRotation;
                StopCameraSpin();
            }
        }

        public void Activate()
        {
            bgObject = GameObject.Find("__GameMain__/BG");
            bg = bgObject.transform;

            bgDragPoint = DragPoint.Make<DragPointBG>(
                PrimitiveType.Cube, Vector3.one * 0.12f, DragPoint.LightBlue
            );
            bgDragPoint.Initialize(() => bg.position, () => Vector3.zero);
            bgDragPoint.Set(bg);
            bgDragPoint.AddGizmo();
            bgDragPoint.ConstantScale = true;
            bgDragPoint.gameObject.SetActive(CubeActive);

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

            UltimateOrbitCamera uoCamera =
                Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(GameMain.Instance.MainCamera, "m_UOCamera");
            uoCamera.enabled = true;

            ResetCamera();
            SaveCameraInfo();

            CubeSmallChange += OnCubeSmall;
            CubeActiveChange += OnCubeActive;
        }

        public void Deactivate()
        {
            if (bgDragPoint != null) GameObject.Destroy(bgDragPoint.gameObject);
            GameObject.Destroy(cameraObject);
            GameObject.Destroy(subCamera);

            BGVisible = true;
            Camera mainCamera = GameMain.Instance.MainCamera.camera;
            mainCamera.backgroundColor = Color.black;

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
            bg.localScale = Vector3.one;
            CubeSmallChange -= OnCubeSmall;
            CubeActiveChange -= OnCubeActive;
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
        }

        public void ChangeBackground(string assetName, bool creative = false)
        {
            currentBGAsset = assetName;
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
            StopCameraSpin();
        }

        public void LoadCameraInfo(CameraInfo cameraInfo)
        {
            CameraMain camera = GameMain.Instance.MainCamera;
            camera.SetTargetPos(cameraInfo.TargetPos);
            camera.SetPos(cameraInfo.Pos);
            camera.SetDistance(cameraInfo.Distance);
            camera.transform.eulerAngles = cameraInfo.Angle;
            StopCameraSpin();
        }

        private void StopCameraSpin()
        {
            UltimateOrbitCamera uoCamera = Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(
                GameMain.Instance.MainCamera, "m_UOCamera"
            );
            Utility.SetFieldValue<UltimateOrbitCamera, float>(uoCamera, "xVelocity", 0f);
            Utility.SetFieldValue<UltimateOrbitCamera, float>(uoCamera, "yVelocity", 0f);
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
            this.bgDragPoint.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            this.bgDragPoint.gameObject.SetActive(CubeActive);
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
