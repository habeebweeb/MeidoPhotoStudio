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
        public void ChangeBackground(string assetName)
        {
            GameMain.Instance.BgMgr.ChangeBg(assetName);
            if (assetName == "KaraokeRoom")
            {
                bg.transform.position = bgObject.transform.position;
                bg.transform.localPosition = new Vector3(1f, 0f, 4f);
                bg.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
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
            UltimateOrbitCamera UOCamera = Utility.GetFieldValue<CameraMain, UltimateOrbitCamera>(GameMain.Instance.MainCamera, "m_UOCamera");
            UOCamera.enabled = true;

            GameMain.Instance.MainLight.Reset();
            GameMain.Instance.CharacterMgr.ResetCharaPosAll();

            CameraMain cameraMain = GameMain.Instance.MainCamera;
            cameraMain.Reset(CameraMain.CameraType.Target, true);
            cameraMain.SetTargetPos(new Vector3(0f, 0.9f, 0f), true);
            cameraMain.SetDistance(3f, true);
        }

        public void Deactivate()
        {
            GameObject.Destroy(cameraObject);
            GameObject.Destroy(subCamera);
        }
    }
}
