using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Camera;

public class SubCameraController : MonoBehaviour
{
    private static CameraMain mainCamera;

    private UnityEngine.Camera subCamera;

    private static CameraMain MainCamera =>
        mainCamera ? mainCamera : mainCamera = GameMain.Instance.MainCamera;

    public void Activate() =>
        enabled = true;

    public void Deactivate() =>
        enabled = false;

    private static UnityEngine.Camera CreateSubCamera()
    {
        var subCameraGameObject = new GameObject("Sub Camera");
        var subCamera = subCameraGameObject.AddComponent<UnityEngine.Camera>();

        subCamera.CopyFrom(MainCamera.camera);
        subCamera.clearFlags = CameraClearFlags.Depth;
        subCamera.cullingMask = 1 << 8;
        subCamera.depth = 1f;
        subCamera.transform.parent = MainCamera.transform;

        return subCamera;
    }

    private void Awake() =>
        subCamera = CreateSubCamera();

    private void Update() =>
        subCamera.fieldOfView = MainCamera.camera.fieldOfView;

    private void OnEnable()
    {
        if (subCamera)
            subCamera.gameObject.SetActive(true);
        else
            subCamera = CreateSubCamera();
    }

    private void OnDisable()
    {
        if (!subCamera)
            return;

        subCamera.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (!subCamera)
            return;

        Destroy(subCamera.gameObject);
    }
}
