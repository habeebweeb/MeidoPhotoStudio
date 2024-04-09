namespace MeidoPhotoStudio.Plugin;

public static class WfCameraMoveSupportUtility
{
    private static GameObject wfCameraGameObject;

    private static WfCameraMoveSupport wfCameraMoveSupport;

    private static WfCameraMoveSupport WfCameraMoveSupport
    {
        get
        {
            if (wfCameraMoveSupport)
                return wfCameraMoveSupport;

            if (SceneEdit.Instance)
            {
                wfCameraMoveSupport = SceneEdit.Instance.m_cameraMoveSupport;
            }
            else
            {
                if (!wfCameraGameObject)
                    wfCameraGameObject = new GameObject("[MPS WfCameraMoveSupport]");

                wfCameraMoveSupport = wfCameraGameObject.GetOrAddComponent<WfCameraMoveSupport>();
            }

            return wfCameraMoveSupport;
        }
    }

    public static void StartMove(Vector3 position, float distance, Vector2 aroundAngle, float moveTime = 0.5f)
    {
        WfCameraMoveSupport.moveTime = moveTime;
        WfCameraMoveSupport.StartCameraPosition(position, distance, aroundAngle);
    }

    public static void Destroy()
    {
        if (wfCameraGameObject)
            Object.Destroy(wfCameraGameObject);
    }
}
