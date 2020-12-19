using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    using UInput = Input;
    public class CameraManager : IManager
    {
        public const string header = "CAMERA";
        private static readonly CameraMain mainCamera = CameraUtility.MainCamera;
        private static readonly UltimateOrbitCamera ultimateOrbitCamera = CameraUtility.UOCamera;
        private float defaultCameraMoveSpeed;
        private float defaultCameraZoomSpeed;
        private const float cameraFastMoveSpeed = 0.1f;
        private const float cameraFastZoomSpeed = 3f;
        private Camera subCamera;
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

        public void Activate()
        {
            ultimateOrbitCamera.enabled = true;

            defaultCameraMoveSpeed = ultimateOrbitCamera.moveSpeed;
            defaultCameraZoomSpeed = ultimateOrbitCamera.zoomSpeed;

            if (!MeidoPhotoStudio.EditMode) ResetCamera();

            currentCameraIndex = 0;

            tempCameraInfo.Reset();

            for (var i = 0; i < CameraCount; i++) cameraInfos[i].Reset();

            mainCamera.ForceCalcNearClip();

            var subCamGo = new GameObject("subcam");
            subCamera = subCamGo.AddComponent<Camera>();
            subCamera.CopyFrom(mainCamera.camera);
            subCamera.clearFlags = CameraClearFlags.Depth;
            subCamera.cullingMask = 1 << 8;
            subCamera.depth = 1f;
            subCamera.transform.parent = mainCamera.transform;
        }

        public void Deactivate()
        {
            UnityEngine.Object.Destroy(subCamera.gameObject);
            mainCamera.camera.backgroundColor = Color.black;

            ultimateOrbitCamera.moveSpeed = defaultCameraMoveSpeed;
            ultimateOrbitCamera.zoomSpeed = defaultCameraZoomSpeed;

            if (MeidoPhotoStudio.EditMode) return;

            mainCamera.Reset(CameraMain.CameraType.Target, true);
            mainCamera.SetTargetPos(new Vector3(0.5609447f, 1.380762f, -1.382336f));
            mainCamera.SetDistance(1.6f);
            mainCamera.SetAroundAngle(new Vector2(245.5691f, 6.273283f));

            mainCamera.ResetCalcNearClip();
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

            subCamera.fieldOfView = mainCamera.camera.fieldOfView;

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
