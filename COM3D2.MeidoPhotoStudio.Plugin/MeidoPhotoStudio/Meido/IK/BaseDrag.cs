using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BaseDrag : MonoBehaviour
    {
        private const float doubleClickSensitivity = 0.3f;
        protected const int upperArm = 0;
        protected const int foreArm = 1;
        protected const int hand = 2;
        protected const int upperArmRot = 0;
        protected const int handRot = 1;
        protected Maid maid;
        protected Func<Vector3> position;
        protected Func<Vector3> rotation;
        protected Renderer dragPointRenderer;
        protected Collider dragPointCollider;
        protected Vector3 worldPoint;
        protected Vector3 mousePos;
        protected DragType dragType = DragType.None;
        protected DragType dragTypeOld;
        protected float doubleClickStart = 0f;
        protected bool reInitDrag = false;
        protected bool isPlaying;
        protected GizmoRender gizmo;
        public bool Visible
        {
            get => dragPointRenderer.enabled;
            set => dragPointRenderer.enabled = value;
        }
        public float DragPointScale
        {
            get => transform.localScale.x;
            set => transform.localScale = new Vector3(value, value, value);
        }
        public float GizmoScale
        {
            set
            {
                if (gizmo != null) gizmo.offsetScale = value;
            }
        }
        public bool GizmoVisible
        {
            get => gizmo?.Visible ?? false;
            set
            {
                if (gizmo != null) gizmo.Visible = value;
            }
        }
        private static bool IsGizmoDrag => Utility.GetFieldValue<GizmoRender, bool>(null, "is_drag_");
        public event EventHandler DragEvent;
        protected enum DragType
        {
            None, Select,
            MoveXZ, MoveY, RotLocalXZ, RotY, RotLocalY,
            Scale
        }

        public virtual void Initialize(Maid maid, Func<Vector3> position, Func<Vector3> rotation)
        {
            this.maid = maid;
            this.position = position;
            this.rotation = rotation;
            this.dragPointRenderer = GetComponent<Renderer>();
            this.dragPointCollider = GetComponent<Collider>();
            this.dragPointRenderer.enabled = true;

            isPlaying = maid.GetAnimation().isPlaying;
        }

        protected void InitializeGizmo(GameObject target, float scale = 0.25f)
        {
            gizmo = target.AddComponent<GizmoRender>();
            gizmo.eRotate = true;
            gizmo.offsetScale = scale;
            gizmo.lineRSelectedThick = 0.25f;
            GizmoVisible = false;
        }

        protected void InitializeGizmo(Transform target, float scale = 0.25f)
        {
            InitializeGizmo(target.gameObject, scale);
        }

        protected virtual void InitializeDrag()
        {
            worldPoint = Camera.main.WorldToScreenPoint(transform.position);
            mousePos = Input.mousePosition;

            isPlaying = maid.GetAnimation().isPlaying;
        }

        protected virtual void DoubleClick() { }
        protected abstract void Drag();
        protected abstract void GetDragType();
        private void OnMouseUp()
        {
            if ((Time.time - doubleClickStart) < doubleClickSensitivity)
            {
                doubleClickStart = -1;
                DoubleClick();
            }
            else
            {
                doubleClickStart = Time.time;
            }
        }

        private void Update()
        {
            GetDragType();

            reInitDrag = dragType != dragTypeOld;

            dragTypeOld = dragType;

            transform.position = position();
            transform.eulerAngles = rotation();

            if (gizmo != null)
            {
                if (GizmoVisible)
                {
                    if (isPlaying && IsGizmoDrag)
                    {
                        maid.GetAnimation().Stop();
                        isPlaying = false;
                    }
                }
            }
        }

        private void OnMouseDown()
        {
            InitializeDrag();
        }

        private void OnMouseDrag()
        {
            if (dragType == DragType.Select) return;

            if (reInitDrag)
            {
                reInitDrag = false;
                InitializeDrag();
            }

            if (mousePos != Input.mousePosition) Drag();
        }

        private void OnEnable()
        {
            if (position != null)
            {
                transform.position = position();
                transform.eulerAngles = rotation();
            }
        }

        private void OnDestroy()
        {
            GameObject.Destroy(gizmo);
        }

        protected void OnDragEvent()
        {
            DragEvent?.Invoke(null, EventArgs.Empty);
        }
    }
}
