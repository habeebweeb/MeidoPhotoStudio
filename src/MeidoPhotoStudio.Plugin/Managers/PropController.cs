using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Framework;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin.Core.Props;

public class PropController(IPropModel propModel, GameObject prop, ShapeKeyController shapeKeyController = null)
{
    public event EventHandler TransformChanged;

    public TransformBackup InitialTransform { get; init; } = new(prop.transform);

    public GameObject GameObject { get; } = prop ? prop : throw new ArgumentNullException(nameof(prop));

    public IPropModel PropModel { get; } = propModel ?? throw new ArgumentNullException(nameof(propModel));

    public ShapeKeyController ShapeKeyController { get; } = shapeKeyController;

    public bool ShadowCasting
    {
        get => Renderers.Any(renderer => renderer.shadowCastingMode is not ShadowCastingMode.Off);
        set
        {
            foreach (var renderer in Renderers)
                renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    public bool Visible
    {
        get => Renderers.Any(renderer => renderer.enabled);
        set
        {
            foreach (var renderer in Renderers)
                renderer.enabled = value;
        }
    }

    private IEnumerable<Renderer> Renderers =>
        GameObject.GetComponentsInChildren<Renderer>();

    public void UpdateTransform() =>
        TransformChanged?.Invoke(this, EventArgs.Empty);

    public void Focus()
    {
        var propPosition = GameObject.transform.position;
        var cameraAngle = GameMain.Instance.MainCamera.transform.eulerAngles;
        var cameraDistance = GameMain.Instance.MainCamera.GetDistance();

        WfCameraMoveSupportUtility.StartMove(propPosition, cameraDistance, new(cameraAngle.y, cameraAngle.x), 0.45f);
    }
}
