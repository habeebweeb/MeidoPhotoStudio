using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class EnvironmentManager
    {
        private GameObject cameraObject;
        private Camera subCamera;
        private GameObject bgObject;
        private Transform bg;
        private CameraInfo cameraInfo;
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

        public void Initialize()
        {
            bgObject = GameObject.Find("__GameMain__/BG");
            bg = bgObject.transform;

            cameraObject = new GameObject("subCamera");
            subCamera = cameraObject.AddComponent<Camera>();
            subCamera.CopyFrom(Camera.main);
            cameraObject.SetActive(true);
            subCamera.clearFlags = CameraClearFlags.Depth;
            subCamera.cullingMask = 256;
            subCamera.depth = 1f;
            subCamera.transform.parent = GameMain.Instance.MainCamera.transform;

            bgObject.SetActive(true);
            GameMain.Instance.BgMgr.ChangeBg("Theater");

            GameMain.Instance.MainCamera.GetComponent<Camera>().backgroundColor = new Color(0.0f, 0.0f, 0.0f);
            UltimateOrbitCamera UOCamera =
                Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(GameMain.Instance.MainCamera, "m_UOCamera");
            UOCamera.enabled = true;

            GameMain.Instance.MainLight.Reset();
            GameMain.Instance.CharacterMgr.ResetCharaPosAll();

            ResetCamera();
            SaveCameraInfo();
        }

        public void Deactivate()
        {
            GameObject.Destroy(cameraObject);
            GameObject.Destroy(subCamera);
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
    }

    public struct CameraInfo
    {
        public Vector3 TargetPos { get; private set; }
        public Vector3 Pos { get; private set; }
        public Vector3 Angle { get; private set; }
        public float Distance { get; private set; }
        public CameraInfo(CameraMain camera)
        {
            this.TargetPos = camera.GetTargetPos();
            this.Pos = camera.GetPos();
            this.Angle = camera.transform.eulerAngles;
            this.Distance = camera.GetDistance();
        }
    }
}
