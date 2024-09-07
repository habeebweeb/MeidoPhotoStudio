using MeidoPhotoStudio.Plugin.Core.Camera;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CameraPane : BasePane
{
    private readonly CameraController cameraController;
    private readonly CameraSaveSlotController cameraSaveSlotController;
    private readonly SelectionGrid cameraSlotSelectionGrid;
    private readonly Slider zRotationSlider;
    private readonly Slider fovSlider;
    private readonly PaneHeader paneHeader;

    public CameraPane(CameraController cameraController, CameraSaveSlotController cameraSaveSlotController)
    {
        this.cameraController = cameraController
            ?? throw new ArgumentNullException(nameof(cameraController));

        this.cameraSaveSlotController = cameraSaveSlotController
            ?? throw new ArgumentNullException(nameof(cameraSaveSlotController));

        this.cameraController.CameraChange += OnCameraChanged;

        var camera = GameMain.Instance.MainCamera.camera;
        var cameraRotation = camera.transform.eulerAngles;

        zRotationSlider = new(Translation.Get("cameraPane", "zRotation"), 0f, 360f, cameraRotation.z)
        {
            HasReset = true,
            HasTextField = true,
        };

        zRotationSlider.ControlEvent += OnZRotationChanged;

        var fieldOfView = camera.fieldOfView;

        fovSlider = new(Translation.Get("cameraPane", "fov"), 20f, 150f, fieldOfView, fieldOfView)
        {
            HasReset = true,
            HasTextField = true,
        };

        fovSlider.ControlEvent += OnFieldOfViewSliderChanged;

        cameraSlotSelectionGrid = new(Enumerable.Range(1, cameraSaveSlotController.SaveSlotCount).Select(x => x.ToString()).ToArray());
        cameraSlotSelectionGrid.ControlEvent += OnCameraSlotChanged;

        paneHeader = new(Translation.Get("cameraPane", "header"), true);
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        cameraSlotSelectionGrid.Draw();
        zRotationSlider.Draw();
        fovSlider.Draw();
    }

    public override void UpdatePane()
    {
        var camera = GameMain.Instance.MainCamera.camera;

        zRotationSlider.SetValueWithoutNotify(camera.transform.eulerAngles.z);
        fovSlider.SetValueWithoutNotify(camera.fieldOfView);
        cameraSlotSelectionGrid.SetValueWithoutNotify(cameraSaveSlotController.CurrentCameraSlot);
    }

    protected override void ReloadTranslation()
    {
        zRotationSlider.Label = Translation.Get("cameraPane", "zRotation");
        fovSlider.Label = Translation.Get("cameraPane", "fov");
        paneHeader.Label = Translation.Get("cameraPane", "header");
    }

    private void OnZRotationChanged(object sender, EventArgs e)
    {
        var camera = GameMain.Instance.MainCamera.camera;
        var newRotation = camera.transform.eulerAngles;

        newRotation.z = zRotationSlider.Value;
        camera.transform.rotation = Quaternion.Euler(newRotation);
    }

    private void OnFieldOfViewSliderChanged(object sender, EventArgs e) =>
        GameMain.Instance.MainCamera.camera.fieldOfView = fovSlider.Value;

    private void OnCameraSlotChanged(object sender, EventArgs e) =>
        cameraSaveSlotController.CurrentCameraSlot = cameraSlotSelectionGrid.SelectedItemIndex;

    private void OnCameraChanged(object sender, EventArgs e) =>
        UpdatePane();
}
