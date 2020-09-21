using System;
using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
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
        private UltimateOrbitCamera ultimateOrbitCamera;
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
                bgVisible = value;
                bgObject.SetActive(bgVisible);
            }
        }
        private float defaultCameraMoveSpeed;
        private float defaultCameraZoomSpeed;
        private const float cameraFastMoveSpeed = 0.1f;
        private const float cameraFastZoomSpeed = 2f;

        static EnvironmentManager()
        {
            Input.Register(MpsKey.CameraLayer, KeyCode.Q, "Camera control layer");
            Input.Register(MpsKey.CameraSave, KeyCode.S, "Save camera transform");
            Input.Register(MpsKey.CameraLoad, KeyCode.A, "Load camera transform");
            Input.Register(MpsKey.CameraReset, KeyCode.R, "Reset camera transform");
        }

        public EnvironmentManager()
        {
            DragPointLight.EnvironmentManager = this;
            Activate();
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter) => Serialize(binaryWriter, false);

        public void Serialize(System.IO.BinaryWriter binaryWriter, bool kankyo)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(currentBGAsset);
            binaryWriter.WriteVector3(bg.position);
            binaryWriter.WriteQuaternion(bg.rotation);
            binaryWriter.WriteVector3(bg.localScale);

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
            bool isCreative = Utility.IsGuidString(bgAsset);
            System.Collections.Generic.List<string> bgList = isCreative
                ? Constants.MyRoomCustomBGList.Select(kvp => kvp.Key).ToList()
                : Constants.BGList;

            int assetIndex = bgList.FindIndex(
                asset => asset.Equals(bgAsset, StringComparison.InvariantCultureIgnoreCase)
            );
            if (assetIndex < 0)
            {
                Utility.LogWarning($"Could not load BG '{bgAsset}'");
                isCreative = false;
                bgAsset = "Theater";
            }
            else bgAsset = bgList[assetIndex];

            ChangeBackground(bgAsset, isCreative);
            bg.position = binaryReader.ReadVector3();
            bg.rotation = binaryReader.ReadQuaternion();
            bg.localScale = binaryReader.ReadVector3();

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

            bgDragPoint = DragPoint.Make<DragPointBG>(PrimitiveType.Cube, Vector3.one * 0.12f);
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

            ultimateOrbitCamera =
                Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(GameMain.Instance.MainCamera, "m_UOCamera");
            ultimateOrbitCamera.enabled = true;

            defaultCameraMoveSpeed = ultimateOrbitCamera.moveSpeed;
            defaultCameraZoomSpeed = ultimateOrbitCamera.zoomSpeed;

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

            ultimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;

            bool isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") == 3;

            if (isNight) GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot_Night");
            else GameMain.Instance.BgMgr.ChangeBg("ShinShitsumu_ChairRot");

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
            if (Input.GetKey(MpsKey.CameraLayer))
            {
                if (Input.GetKeyDown(MpsKey.CameraSave))
                {
                    SaveCameraInfo();
                }

                if (Input.GetKeyDown(MpsKey.CameraLoad))
                {
                    LoadCameraInfo(cameraInfo);
                }

                if (Input.GetKeyDown(MpsKey.CameraReset))
                {
                    ResetCamera();
                }
            }

            bool shift = Input.Shift;
            ultimateOrbitCamera.moveSpeed = shift ? cameraFastMoveSpeed : defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = shift ? cameraFastZoomSpeed : defaultCameraZoomSpeed;
        }

        public void ChangeBackground(string assetName, bool creative = false)
        {
            currentBGAsset = assetName;
            if (creative) GameMain.Instance.BgMgr.ChangeBgMyRoom(assetName);
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
            cameraInfo = new CameraInfo(GameMain.Instance.MainCamera);
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
            Utility.SetFieldValue(uoCamera, "xVelocity", 0f);
            Utility.SetFieldValue(uoCamera, "yVelocity", 0f);
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
            bgDragPoint.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnCubeActive(object sender, EventArgs args) => bgDragPoint.gameObject.SetActive(CubeActive);
    }

    public struct CameraInfo
    {
        public Vector3 TargetPos { get; }
        public Vector3 Pos { get; }
        public Vector3 Angle { get; }
        public float Distance { get; }
        public CameraInfo(CameraMain camera)
        {
            TargetPos = camera.GetTargetPos();
            Pos = camera.GetPos();
            Angle = camera.transform.eulerAngles;
            Distance = camera.GetDistance();
        }
    }
}
