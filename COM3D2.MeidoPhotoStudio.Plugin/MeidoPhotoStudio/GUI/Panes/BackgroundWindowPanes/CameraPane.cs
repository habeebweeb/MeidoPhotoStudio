using System.Linq;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class CameraPane : BasePane
    {
        private readonly EnvironmentManager environmentManager;
        private readonly SelectionGrid cameraGrid;
        private readonly Slider zRotationSlider;
        private readonly Slider fovSlider;
        private string header;

        public CameraPane(EnvironmentManager environmentManager)
        {
            this.environmentManager = environmentManager;
            this.environmentManager.CameraChange += (s, a) => UpdatePane();

            Camera camera = CameraUtility.MainCamera.camera;
            Vector3 eulerAngles = camera.transform.eulerAngles;

            cameraRotation = eulerAngles;

            zRotationSlider = new Slider(Translation.Get("cameraPane", "zRotation"), 0f, 360f, eulerAngles.z);
            zRotationSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                Vector3 newRotation = camera.transform.eulerAngles;
                newRotation.z = zRotationSlider.Value;
                camera.transform.rotation = Quaternion.Euler(newRotation);
            };
            fovSlider = new Slider(Translation.Get("cameraPane", "fov"), 20f, 150f, camera.fieldOfView);
            fovSlider.ControlEvent += (s, a) =>
            {
                if (updating) return;
                camera.fieldOfView = fovSlider.Value;
            };
            cameraGrid = new SelectionGrid(
                Enumerable.Range(1, environmentManager.CameraCount).Select(x => x.ToString()).ToArray()
            );
            cameraGrid.ControlEvent += (s, a) =>
            {
                if (updating) return;
                environmentManager.CurrentCameraIndex = cameraGrid.SelectedItemIndex;
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

            cameraGrid.SelectedItemIndex = environmentManager.CurrentCameraIndex;

            updating = false;
        }
    }
}
