using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    using UInput = Input;
    public class CameraManager : IManager, ISerializable
    {
        public const string header = "CAMERA";
        private static readonly CameraMain mainCamera = CameraUtility.MainCamera;
        private static readonly UltimateOrbitCamera ultimateOrbitCamera = CameraUtility.UOCamera;
        private GameObject cameraObject;
        private Camera subCamera;
        private float defaultCameraMoveSpeed;
        private float defaultCameraZoomSpeed;
        private const float cameraFastMoveSpeed = 0.1f;
        private const float cameraFastZoomSpeed = 3f;
        private CameraInfo tempCameraInfo = new CameraInfo();
        private const KeyCode AlphaOne = KeyCode.Alpha1;
        public int CameraCount => cameraInfos.Length;
        public EventHandler CameraChange;

        private int currentCameraIndex;
        public int CurrentCameraIndex
        {
            get => currentCameraIndex;
            set
            {
                cameraInfos[currentCameraIndex].UpdateInfo(mainCamera);
                currentCameraIndex = value;
                LoadCameraInfo(cameraInfos[currentCameraIndex]);
            }
        }
        private CameraInfo[] cameraInfos;

        static CameraManager()
        {
            Input.Register(MpsKey.CameraLayer, KeyCode.Q, "Camera control layer");
            Input.Register(MpsKey.CameraSave, KeyCode.S, "Save camera transform");
            Input.Register(MpsKey.CameraLoad, KeyCode.A, "Load camera transform");
            Input.Register(MpsKey.CameraReset, KeyCode.R, "Reset camera transform");
        }

        public CameraManager()
        {
            cameraInfos = new CameraInfo[5];
            for (var i = 0; i < cameraInfos.Length; i++) cameraInfos[i] = new CameraInfo();
            Activate();
        }

        public void Serialize(BinaryWriter binaryWriter) => Serialize(binaryWriter, false);

        public void Serialize(BinaryWriter binaryWriter, bool kankyo)
        {
            binaryWriter.Write(header);

            binaryWriter.Write(kankyo);

            binaryWriter.WriteVector3(mainCamera.GetTargetPos());
            binaryWriter.Write(mainCamera.GetDistance());
            binaryWriter.WriteQuaternion(mainCamera.transform.rotation);

            CameraUtility.StopAll();
        }

        public void Deserialize(BinaryReader binaryReader)
        {
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
            cameraObject = new GameObject("subCamera");
            subCamera = cameraObject.AddComponent<Camera>();
            subCamera.CopyFrom(mainCamera.camera);
            subCamera.clearFlags = CameraClearFlags.Depth;
            subCamera.cullingMask = 256;
            subCamera.depth = 1f;
            subCamera.transform.parent = mainCamera.transform;

            cameraObject.SetActive(true);

            ultimateOrbitCamera.enabled = true;

            defaultCameraMoveSpeed = ultimateOrbitCamera.moveSpeed;
            defaultCameraZoomSpeed = ultimateOrbitCamera.zoomSpeed;

            if (!MeidoPhotoStudio.EditMode) ResetCamera();

            currentCameraIndex = 0;

            tempCameraInfo.Reset();

            for (var i = 0; i < CameraCount; i++) cameraInfos[i].Reset();
        }

        public void Deactivate()
        {
            Object.Destroy(cameraObject);
            Object.Destroy(subCamera);

            mainCamera.camera.backgroundColor = Color.black;

            ultimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;

            if (MeidoPhotoStudio.EditMode) return;

            mainCamera.Reset(CameraMain.CameraType.Target, true);
            mainCamera.SetTargetPos(new Vector3(0.5609447f, 1.380762f, -1.382336f));
            mainCamera.SetDistance(1.6f);
            mainCamera.SetAroundAngle(new Vector2(245.5691f, 6.273283f));
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

        private void SaveTempCamera()
        {
            tempCameraInfo.UpdateInfo(mainCamera);
            CameraUtility.StopAll();
        }

        public void LoadCameraInfo(CameraInfo info)
        {
            info.Apply(mainCamera);
            CameraUtility.StopAll();
            CameraChange?.Invoke(this, EventArgs.Empty);
        }

        private void ResetCamera()
        {
            mainCamera.Reset(CameraMain.CameraType.Target, true);
            mainCamera.SetTargetPos(new Vector3(0f, 0.9f, 0f));
            mainCamera.SetDistance(3f);
            CameraChange?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CameraInfo
    {
        public Vector3 TargetPos { get; set; }
        public Quaternion Angle { get; set; }
        public float Distance { get; set; }
        public float FOV { get; set; }

        public CameraInfo() => Reset();

        public static CameraInfo FromCamera(CameraMain mainCamera)
        {
            var info = new CameraInfo();
            info.UpdateInfo(mainCamera);
            return info;
        }

        public void Reset()
        {
            TargetPos = new Vector3(0f, 0.9f, 0f);
            Angle = Quaternion.Euler(10f, 180f, 0f);
            Distance = 3f;
            FOV = 35f;
        }

        public void UpdateInfo(CameraMain camera)
        {
            TargetPos = camera.GetTargetPos();
            Angle = camera.transform.rotation;
            Distance = camera.GetDistance();
            FOV = camera.camera.fieldOfView;
        }

        public void Apply(CameraMain camera)
        {
            camera.SetTargetPos(TargetPos);
            camera.SetDistance(Distance);
            camera.transform.rotation = Angle;
            camera.camera.fieldOfView = FOV;
        }
    }
}
