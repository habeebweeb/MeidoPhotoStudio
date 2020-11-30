using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    using UInput = Input;
    public class EnvironmentManager : IManager, ISerializable
    {
        private static readonly BgMgr bgMgr = GameMain.Instance.BgMgr;
        private static readonly CameraMain mainCamera = CameraUtility.MainCamera;
        private static readonly UltimateOrbitCamera ultimateOrbitCamera = CameraUtility.UOCamera;
        public const string header = "ENVIRONMENT";
        public const string defaultBg = "Theater";
        private const string myRoomPrefix = "マイルーム:";
        private static bool cubeActive;
        public static bool CubeActive
        {
            get => cubeActive;
            set
            {
                if (value == cubeActive) return;
                cubeActive = value;
                CubeActiveChange?.Invoke(null, EventArgs.Empty);
            }
        }
        private static bool cubeSmall;
        public static bool CubeSmall
        {
            get => cubeSmall;
            set
            {
                if (value == cubeSmall) return;
                cubeSmall = value;
                CubeSmallChange?.Invoke(null, EventArgs.Empty);
            }
        }
        private static event EventHandler CubeActiveChange;
        private static event EventHandler CubeSmallChange;
        private GameObject cameraObject;
        private Camera subCamera;
        private GameObject bgObject;
        private Transform bg;
        private DragPointBG bgDragPoint;
        private string currentBGAsset = defaultBg;
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
        private const float cameraFastZoomSpeed = 3f;
        private CameraInfo tempCameraInfo;
        private int currentCameraIndex;
        private const KeyCode AlphaOne = KeyCode.Alpha1;
        public int CameraCount => cameraInfos.Length;
        public EventHandler CameraChange;

        public int CurrentCameraIndex
        {
            get => currentCameraIndex;
            set
            {
                cameraInfos[currentCameraIndex] = mainCamera.GetInfo();
                currentCameraIndex = value;
                LoadCameraInfo(cameraInfos[currentCameraIndex]);
            }
        }
        private CameraInfo[] cameraInfos;

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

        public void Serialize(BinaryWriter binaryWriter) => Serialize(binaryWriter, false);

        public void Serialize(BinaryWriter binaryWriter, bool kankyo)
        {
            binaryWriter.Write(header);
            binaryWriter.Write(currentBGAsset);
            binaryWriter.WriteVector3(bg.position);
            binaryWriter.WriteQuaternion(bg.rotation);
            binaryWriter.WriteVector3(bg.localScale);

            binaryWriter.Write(kankyo);

            binaryWriter.WriteVector3(mainCamera.GetTargetPos());
            binaryWriter.Write(mainCamera.GetDistance());
            binaryWriter.WriteQuaternion(mainCamera.transform.rotation);

            CameraUtility.StopAll();
        }

        public void Deserialize(BinaryReader binaryReader)
        {
            var bgAsset = binaryReader.ReadString();
            var isCreative = Utility.IsGuidString(bgAsset);
            List<string> bgList = isCreative
                ? Constants.MyRoomCustomBGList.ConvertAll(kvp => kvp.Key)
                : Constants.BGList;

            var assetIndex = bgList.FindIndex(
                asset => asset.Equals(bgAsset, StringComparison.InvariantCultureIgnoreCase)
            );
            if (assetIndex < 0)
            {
                Utility.LogWarning($"Could not load BG '{bgAsset}'");
                isCreative = false;
                bgAsset = defaultBg;
            }
            else bgAsset = bgList[assetIndex];

            ChangeBackground(bgAsset, isCreative);
            bg.position = binaryReader.ReadVector3();
            bg.rotation = binaryReader.ReadQuaternion();
            bg.localScale = binaryReader.ReadVector3();

            var kankyo = binaryReader.ReadBoolean();

            Vector3 cameraPosition = binaryReader.ReadVector3();
            var cameraDistance = binaryReader.ReadSingle();
            Quaternion cameraRotation = binaryReader.ReadQuaternion();

            if (kankyo) return;

            mainCamera.SetTargetPos(cameraPosition);
            mainCamera.SetDistance(cameraDistance);
            mainCamera.transform.rotation = cameraRotation;
            CameraUtility.StopAll();
        }

        public void Activate()
        {
            BgMgrPatcher.ChangeBgBegin += OnChangeBegin;
            BgMgrPatcher.ChangeBgEnd += OnChangeEnd;

            bgObject = bgMgr.Parent;

            cameraObject = new GameObject("subCamera");
            subCamera = cameraObject.AddComponent<Camera>();
            subCamera.CopyFrom(mainCamera.camera);
            subCamera.clearFlags = CameraClearFlags.Depth;
            subCamera.cullingMask = 256;
            subCamera.depth = 1f;
            subCamera.transform.parent = mainCamera.transform;

            cameraObject.SetActive(true);

            bgObject.SetActive(true);

            ultimateOrbitCamera.enabled = true;

            defaultCameraMoveSpeed = ultimateOrbitCamera.moveSpeed;
            defaultCameraZoomSpeed = ultimateOrbitCamera.zoomSpeed;

            if (!MeidoPhotoStudio.EditMode)
            {
                ResetCamera();
                ChangeBackground(defaultBg);
            }
            else UpdateBG();

            SaveTempCamera();

            CameraInfo initalInfo = mainCamera.GetInfo();

            cameraInfos = new CameraInfo[5];

            for (var i = 0; i < CameraCount; i++) cameraInfos[i] = initalInfo;

            CubeSmallChange += OnCubeSmall;
            CubeActiveChange += OnCubeActive;
        }

        public void Deactivate()
        {
            BgMgrPatcher.ChangeBgBegin -= OnChangeBegin;
            BgMgrPatcher.ChangeBgEnd -= OnChangeEnd;

            DestroyDragPoint();
            Object.Destroy(cameraObject);
            Object.Destroy(subCamera);

            BGVisible = true;
            mainCamera.camera.backgroundColor = Color.black;

            ultimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;

            if (MeidoPhotoStudio.EditMode) bgMgr.ChangeBg(defaultBg);
            else
            {
                var isNight = GameMain.Instance.CharacterMgr.status.GetFlag("時間帯") == 3;

                bgMgr.ChangeBg(isNight ? "ShinShitsumu_ChairRot_Night" : "ShinShitsumu_ChairRot");

                mainCamera.Reset(CameraMain.CameraType.Target, true);
                mainCamera.SetTargetPos(new Vector3(0.5609447f, 1.380762f, -1.382336f));
                mainCamera.SetDistance(1.6f);
                mainCamera.SetAroundAngle(new Vector2(245.5691f, 6.273283f));
            }

            if (bgMgr.BgObject) bgMgr.BgObject.transform.localScale = Vector3.one;

            CubeSmallChange -= OnCubeSmall;
            CubeActiveChange -= OnCubeActive;
        }

        public void Update()
        {
            if (Input.GetKey(MpsKey.CameraLayer))
            {
                if (Input.GetKeyDown(MpsKey.CameraSave)) SaveTempCamera();
                else if (Input.GetKeyDown(MpsKey.CameraLoad)) LoadCameraInfo(tempCameraInfo);
                else if (Input.GetKeyDown(MpsKey.CameraReset)) ResetCamera();

                for (var i = 0; i < CameraCount; i++)
                {
                    if (i != CurrentCameraIndex && UInput.GetKeyDown(AlphaOne + i)) CurrentCameraIndex = i;
                }
            }

            var shift = Input.Shift;
            ultimateOrbitCamera.moveSpeed = shift ? cameraFastMoveSpeed : defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = shift ? cameraFastZoomSpeed : defaultCameraZoomSpeed;
        }

        public void ChangeBackground(string assetName, bool creative = false)
        {
            if (creative) bgMgr.ChangeBgMyRoom(assetName);
            else bgMgr.ChangeBg(assetName);
        }

        private void AttachDragPoint(Transform bgTransform)
        {
            bgDragPoint = DragPoint.Make<DragPointBG>(PrimitiveType.Cube, Vector3.one * 0.12f);
            bgDragPoint.Initialize(() => bgTransform.position, () => Vector3.zero);
            bgDragPoint.Set(bgTransform);
            bgDragPoint.AddGizmo();
            bgDragPoint.ConstantScale = true;
            bgDragPoint.gameObject.SetActive(CubeActive);
        }

        private void OnChangeBegin(object sender, EventArgs args) => DestroyDragPoint();

        private void OnChangeEnd(object sender, EventArgs args) => UpdateBG();

        private void UpdateBG()
        {
            if (!bgMgr.BgObject) return;

            currentBGAsset = bgMgr.GetBGName();
            if (currentBGAsset.StartsWith(myRoomPrefix))
                currentBGAsset = currentBGAsset.Replace(myRoomPrefix, string.Empty);
            bg = bgMgr.BgObject.transform;
            AttachDragPoint(bg);
        }

        private void DestroyDragPoint()
        {
            if (bgDragPoint) Object.Destroy(bgDragPoint.gameObject);
        }

        private void SaveTempCamera() => tempCameraInfo = mainCamera.GetInfo(true);

        public void LoadCameraInfo(CameraInfo info)
        {
            mainCamera.ApplyInfo(info, true);
            CameraChange?.Invoke(this, EventArgs.Empty);
        }

        private void ResetCamera()
        {
            mainCamera.Reset(CameraMain.CameraType.Target, true);
            mainCamera.SetTargetPos(new Vector3(0f, 0.9f, 0f));
            mainCamera.SetDistance(3f);
            CameraChange?.Invoke(this, EventArgs.Empty);
        }

        private void OnCubeSmall(object sender, EventArgs args)
        {
            bgDragPoint.DragPointScale = CubeSmall ? DragPointGeneral.smallCube : 1f;
        }

        private void OnCubeActive(object sender, EventArgs args)
        {
            bgDragPoint.gameObject.SetActive(CubeActive);
        }
    }

    public readonly struct CameraInfo
    {
        public Vector3 TargetPos { get; }
        public Quaternion Angle { get; }
        public float Distance { get; }
        public float FOV { get; }

        public CameraInfo(CameraMain camera)
        {
            TargetPos = camera.GetTargetPos();
            Angle = camera.transform.rotation;
            Distance = camera.GetDistance();
            FOV = camera.camera.fieldOfView;
        }
    }
}
