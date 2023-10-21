using System.Linq;

using UnityEngine;
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

        private void Awake() =>
            mainCamera = Camera.main;

        private void Update()
        {
            if (!clicked && UInput.GetMouseButtonDown(0) && GetClickedDragHandle(out var dragHandle))
            {
                GizmoRender.global_control_lock = true;
                GizmoRender.is_drag_ = false;

                clicked = true;

                SelectedDragHandle = dragHandle;

                SelectedDragHandle.Select();
                SelectedDragHandle.Click();
            }
            else if (clicked && !OnlyLeftClickPressed())
            {
                if (SelectedDragHandle)
                {
                    if (IsDoubleClick())
                        SelectedDragHandle.DoubleClick();

                    SelectedDragHandle.Release();
                }

                GizmoRender.global_control_lock = false;
                previousSelectedDragHandle = SelectedDragHandle;
                SelectedDragHandle = null;
                clicked = false;
            }

            bool GetClickedDragHandle(out DragHandle dragHandle)
            {
                dragHandle = null;

                if (UICamera.Raycast(UInput.mousePosition))
                    return false;

                var currentEvent = EventSystem.current;

                if (currentEvent && currentEvent.IsPointerOverGameObject())
                    return false;

                var ray = mainCamera.ScreenPointToRay(UInput.mousePosition);

                var hitCount = Physics.RaycastNonAlloc(ray, raycastHits, float.PositiveInfinity, 1 << DragHandleLayer);

                if (hitCount is 0)
                    return false;

                if (hitCount is 1)
                {
                    dragHandle = raycastHits[0].transform.GetComponent<DragHandle>();

                    return true;
                }

                var dragHandles = raycastHits.Take(hitCount)
                    .Select(hit => new { hit, dragHandle = hit.transform.GetComponent<DragHandle>() })
                    .OrderByDescending(pair => pair.dragHandle.Priority)
                    .ThenBy(pair => pair.hit.distance)
                    .Select(pair => pair.dragHandle);

                dragHandle = dragHandles.First();

                return true;
            }

            bool IsDoubleClick()
            {
                var newClickTime = Time.time;
                var result = newClickTime - clickStartTime < DoubleClickSensitivity;

                clickStartTime = newClickTime;

                return previousSelectedDragHandle == SelectedDragHandle && result;
            }
        }
    }
}
