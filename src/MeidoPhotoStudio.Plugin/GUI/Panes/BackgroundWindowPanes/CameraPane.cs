using MeidoPhotoStudio.Plugin.Core.Camera;

namespace MeidoPhotoStudio.Plugin;

public class CameraPane : BasePane
{
    private readonly CameraController cameraManager;
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly SelectionGrid cameraGrid;
    private readonly Slider zRotationSlider;
    private readonly Slider fovSlider;
    private readonly PaneHeader paneHeader;

    public CameraPane(CameraController cameraManager, CameraSaveSlotController cameraSaveSlotController)
    {
        this.cameraManager = cameraManager;
        this.cameraSaveSlotController = cameraSaveSlotController;
        this.cameraManager.CameraChange += (_, _) =>
            UpdatePane();

        var camera = GameMain.Instance.MainCamera.camera;
        var eulerAngles = camera.transform.eulerAngles;

        zRotationSlider = new(Translation.Get("cameraPane", "zRotation"), 0f, 360f, eulerAngles.z)
        {
            HasReset = true,
            HasTextField = true,
        };

        zRotationSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            var newRotation = camera.transform.eulerAngles;

            newRotation.z = zRotationSlider.Value;
            camera.transform.rotation = Quaternion.Euler(newRotation);
        };

        var fieldOfView = camera.fieldOfView;

        fovSlider = new(Translation.Get("cameraPane", "fov"), 20f, 150f, fieldOfView, fieldOfView)
        {
            HasReset = true,
            HasTextField = true,
        };

        fovSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            camera.fieldOfView = fovSlider.Value;
        };

        cameraGrid = new(Enumerable.Range(1, cameraSaveSlotController.SaveSlotCount).Select(x => x.ToString()).ToArray());
        cameraGrid.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            cameraSaveSlotController.CurrentCameraSlot = cameraGrid.SelectedItemIndex;
        };

        paneHeader = new(Translation.Get("cameraPane", "header"), true);
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        cameraGrid.Draw();
        zRotationSlider.Draw();
        fovSlider.Draw();
    }

    public override void UpdatePane()
    {
        updating = true;

        var camera = GameMain.Instance.MainCamera.camera;

        zRotationSlider.Value = camera.transform.eulerAngles.z;
        fovSlider.Value = camera.fieldOfView;

        cameraGrid.SelectedItemIndex = cameraSaveSlotController.CurrentCameraSlot;

        updating = false;
    }

    protected override void ReloadTranslation()
    {
        zRotationSlider.Label = Translation.Get("cameraPane", "zRotation");
        fovSlider.Label = Translation.Get("cameraPane", "fov");
        paneHeader.Label = Translation.Get("cameraPane", "header");
    }
}
