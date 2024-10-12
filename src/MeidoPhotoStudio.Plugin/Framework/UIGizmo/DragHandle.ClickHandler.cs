using UnityEngine.EventSystems;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.UIGizmo;

/// <summary>Drag handle click handler.</summary>
public partial class DragHandle
{
    public class ClickHandler : MonoBehaviour
    {
        private const float DefaultDoubleClickSensitivity = 0.3f;

        private static float doubleClickSensitivity = DefaultDoubleClickSensitivity;

        private readonly RaycastHit[] raycastHits = new RaycastHit[10];

        private bool clicked;
        private float clickStartTime;
        private Vector3 clickStartPosition;
        private Camera mainCamera;
        private DragHandle previousSelectedDragHandle;
        private DragHandle selectedDragHandle;

        public static float DoubleClickSensitivity
        {
            get => doubleClickSensitivity;
            set
            {
                var newSensitivity = value;

                if (value < DefaultDoubleClickSensitivity)
                    newSensitivity = DefaultDoubleClickSensitivity;

                doubleClickSensitivity = newSensitivity;
            }
        }

        public static int SelectedDragHandleID { get; private set; }

        public DragHandle SelectedDragHandle
        {
            get => selectedDragHandle;
            set
            {
                selectedDragHandle = value;
                SelectedDragHandleID = value == null ? 0 : value.GetInstanceID();
            }
        }

        public Core.UI.Legacy.WindowManager WindowManager { get; set; }

        private void Awake() =>
            mainCamera = GameMain.Instance.MainCamera.camera;

        private void Update()
        {
            if (UInput.GetMouseButtonDown(0) && GetClickInfo(out var info))
            {
                GizmoRender.is_drag_ = false;

                clicked = true;

                SelectedDragHandle = info.DragHandle;

                if (IsDoubleClick())
                    SelectedDragHandle.DoubleClick();
                else
                    SelectedDragHandle.Click();

                SelectedDragHandle.Select(info.Hit);

                UpdateDoubleClickInfo();
            }
            else if (clicked && UInput.GetMouseButtonDown(1))
            {
                if (SelectedDragHandle)
                {
                    SelectedDragHandle.Cancel();
                    SelectedDragHandle.Release();
                    SelectedDragHandle = null;
                }

                clicked = false;
            }
            else if (clicked && OnlyLeftClickPressed() && SelectedDragHandle)
            {
                SelectedDragHandle.Drag();
            }
            else if (clicked && !OnlyLeftClickPressed() && !ValidDoubleClick())
            {
                if (SelectedDragHandle)
                    SelectedDragHandle.Release();

                previousSelectedDragHandle = SelectedDragHandle;
                SelectedDragHandle = null;
                clicked = false;
            }

            bool GetClickInfo(out (DragHandle DragHandle, RaycastHit Hit) info)
            {
                info = default;

                if (UICamera.Raycast(UInput.mousePosition))
                    return false;

                if (WindowManager?.MouseOverAnyWindow() ?? false)
                    return false;

                var currentEvent = EventSystem.current;

                if (currentEvent && currentEvent.IsPointerOverGameObject())
                    return false;

                var ray = mainCamera.ScreenPointToRay(UInput.mousePosition);

                var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, float.PositiveInfinity, 1 << DragHandleLayer);

                if (hitCount is 0)
                    return false;

                info = raycastHits.Take(hitCount)
                    .Select(hit => (dragHandle: hit.transform.GetComponent<DragHandle>(), hit))
                    .OrderByDescending(pair => pair.dragHandle.Priority)
                    .ThenBy(pair => pair.hit.distance)
                    .First();

                return true;
            }

            static bool OnlyLeftClickPressed() =>
                UInput.GetMouseButton(0) && !UInput.GetMouseButton(2);

            bool IsDoubleClick() =>
                previousSelectedDragHandle == SelectedDragHandle && ValidDoubleClick();

            void UpdateDoubleClickInfo()
            {
                clickStartTime = Time.time;
                clickStartPosition = UInput.mousePosition;
                previousSelectedDragHandle = SelectedDragHandle;
            }

            bool ValidDoubleClick() =>
                Time.time - clickStartTime < DoubleClickSensitivity
                && Vector2.Distance(clickStartPosition, UInput.mousePosition) <= 2f;
        }
    }
}
