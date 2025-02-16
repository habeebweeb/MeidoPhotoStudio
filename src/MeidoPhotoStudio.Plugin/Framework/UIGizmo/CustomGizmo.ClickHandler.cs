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

        private readonly RaycastHit[] raycastHits = new RaycastHit[10];

        private Camera mainCamera;
        private bool clicked;

        public Core.UI.Legacy.WindowManager WindowManager { get; set; }

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

                if (WindowManager && WindowManager.MouseOverAnyWindow())
                    return false;

                var currentEvent = EventSystem.current;

                if (currentEvent && currentEvent.IsPointerOverGameObject())
                    return false;

                var ray = mainCamera.ScreenPointToRay(UInput.mousePosition);

                var hitCount = Physics.RaycastNonAlloc(ray, raycastHits);

                return raycastHits
                    .Take(hitCount)
                    .Select(static hit => hit.transform.gameObject.layer)
                    .All(static layer => layer != DragHandleLayer && layer != NguiLayer);
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
