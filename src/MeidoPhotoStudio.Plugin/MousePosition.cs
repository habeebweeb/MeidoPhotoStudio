using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class MousePosition : MonoBehaviour
{
    private Vector3 mousePosition;

    public Vector3 Position =>
        mousePosition;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        mousePosition = Input.mousePosition;
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mousePosition.x += Input.GetAxis("Mouse X") * 20;
            mousePosition.y += Input.GetAxis("Mouse Y") * 20;
        }
        else
        {
            mousePosition = Input.mousePosition;
        }
    }
}
