using UnityEngine.EventSystems;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.UIGizmo;

/// <summary>Gizmo click handler.</summary>
public partial class CustomGizmo
{
    public class ClickHandler : MonoBehaviour
    {
        private static readonly int DragHandleLayer = LayerMask.NameToLayer("AbsolutFront");
        private static readonly int NguiLayer = LayerMask.NameToLayer("NGUI");

        private Camera mainCamera;
        private bool clicked;

        public WindowManager WindowManager { get; set; }

        private void Awake() =>
            mainCamera = GameMain.Instance.MainCamera.camera;

        private void Update()
        {
            if (!clicked && NInput.GetMouseButtonDown(0))
            {
                clicked = true;
                is_drag_ = ClickedNothing();
            }
            else if (clicked && !NInput.GetMouseButton(0))
            {
                clicked = false;
                is_drag_ = false;
            }

            bool ClickedNothing()
            {
                if (UICamera.Raycast(UInput.mousePosition))
                    return false;

                if (WindowManager?.MouseOverAnyWindow() ?? false)
                    return false;

                var currentEvent = EventSystem.current;

                if (currentEvent && currentEvent.IsPointerOverGameObject())
                    return false;

                var ray = mainCamera.ScreenPointToRay(UInput.mousePosition);

                Physics.Raycast(ray, out var hit);

                return !hit.transform
                    || hit.transform.gameObject.layer != DragHandleLayer
                    && hit.transform.gameObject.layer != NguiLayer;
            }
        }

        private void OnEnable()
        {
            if (!GameMain.Instance.VRMode)
                return;

            enabled = false;
        }
    }
}
