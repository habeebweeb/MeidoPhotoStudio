﻿using System.Linq;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    public class CameraPane : BasePane
    {
        private readonly CameraManager cameraManager;
        private readonly SelectionGrid cameraGrid;
        private readonly Slider zRotationSlider;
        private readonly Slider fovSlider;
        private string header;

        public CameraPane(CameraManager cameraManager)
        {
            this.cameraManager = cameraManager;
            this.cameraManager.CameraChange += (s, a) => UpdatePane();

            Camera camera = CameraUtility.MainCamera.camera;
            Vector3 eulerAngles = camera.transform.eulerAngles;

            zRotationSlider = new Slider(Translation.Get("cameraPane", "zRotation"), 0f, 360f, eulerAngles.z)
            {
                HasReset = true, HasTextField = true
            };
            zRotationSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                Vector3 newRotation = camera.transform.eulerAngles;
                newRotation.z = zRotationSlider.Value;
                camera.transform.rotation = Quaternion.Euler(newRotation);
            };

            var fieldOfView = camera.fieldOfView;
            fovSlider = new Slider(Translation.Get("cameraPane", "fov"), 20f, 150f, fieldOfView, fieldOfView)
            {
                HasReset = true, HasTextField = true
            };
            fovSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                camera.fieldOfView = fovSlider.Value;
            };
            cameraGrid = new SelectionGrid(
                Enumerable.Range(1, cameraManager.CameraCount).Select(x => x.ToString()).ToArray()
            );
            cameraGrid.ControlEvent += (s, a) =>
            {
                if (updating) return;
                cameraManager.CurrentCameraIndex = cameraGrid.SelectedItemIndex;
            };

            header = Translation.Get("cameraPane", "header");
        }

        protected override void ReloadTranslation()
        {
            zRotationSlider.Label = Translation.Get("cameraPane", "zRotation");
            fovSlider.Label = Translation.Get("cameraPane", "fov");
            header = Translation.Get("cameraPane", "header");
        }

        public override void Draw()
        {
            MpsGui.Header(header);
            MpsGui.WhiteLine();
            cameraGrid.Draw();
            zRotationSlider.Draw();
            fovSlider.Draw();
        }

        public override void UpdatePane()
        {
            updating = true;

            Camera camera = CameraUtility.MainCamera.camera;

            zRotationSlider.Value = camera.transform.eulerAngles.z;
            fovSlider.Value = camera.fieldOfView;

            cameraGrid.SelectedItemIndex = cameraManager.CurrentCameraIndex;

            updating = false;
        }
    }
}
