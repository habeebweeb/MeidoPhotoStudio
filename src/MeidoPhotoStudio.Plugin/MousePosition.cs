using UnityEngine;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin;

public class MousePosition : MonoBehaviour
{
    private Vector3 mousePosition;

    public Vector3 Position =>
        mousePosition;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        mousePosition = UInput.mousePosition;
    }

    private void Update()
    {
        if (UInput.GetMouseButton(0))
        {
            mousePosition.x += UInput.GetAxis("Mouse X") * 20;
            mousePosition.y += UInput.GetAxis("Mouse Y") * 20;
        }
        else
        {
            mousePosition = UInput.mousePosition;
        }
    }
}
