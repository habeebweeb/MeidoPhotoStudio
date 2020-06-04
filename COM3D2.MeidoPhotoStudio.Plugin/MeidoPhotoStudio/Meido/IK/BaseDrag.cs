using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    // TODO: Finalize dragpopint scaling
    public abstract class BaseDrag : MonoBehaviour
    {
        private const float doubleClickSensitivity = 0.3f;
        protected const int upperArm = 0;
        protected const int foreArm = 1;
        protected const int hand = 2;
        protected const int upperArmRot = 0;
        protected const int handRot = 1;
        protected Maid maid;
        protected Meido meido;
        protected Func<Vector3> position;
        protected Func<Vector3> rotation;
        protected Renderer dragPointRenderer;
        protected Collider dragPointCollider;
        protected Vector3 worldPoint;
        protected Vector3 mousePos;
        protected DragType dragType = DragType.None;
        protected DragType dragTypeOld;
        protected GizmoType gizmoType;
        protected GizmoType gizmoTypeOld;
        protected float doubleClickStart = 0f;
        protected bool reInitDrag = false;
        protected bool isPlaying;
        protected GizmoRender gizmo;
        public Vector3 BaseScale { get; private set; }
        public Vector3 DragPointScale
        {
            get => transform.localScale;
            set
            {
                transform.localScale = value;
            }
        }
        public bool IsBone { get; set; }
        public float GizmoScale
        {
            get => gizmo.offsetScale;
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
        private bool gizmoActive = false;
        public bool GizmoActive
        {
            get => gizmoActive;
            set
            {
                gizmoActive = value;
                GizmoVisible = gizmoActive;
            }
        }
        private bool dragPointVisible;
        public bool DragPointVisible
        {
            get => dragPointVisible;
            set
            {
                dragPointVisible = value;
                dragPointRenderer.enabled = dragPointVisible;
            }
        }
        private bool dragPointActive;
        public bool DragPointActive
        {
            get => dragPointActive;
            set
            {
                dragPointActive = value;
                dragPointCollider.enabled = dragPointActive;
            }
        }
        protected static bool IsGizmoDrag => Utility.GetFieldValue<GizmoRender, bool>(null, "is_drag_");
        public event EventHandler DragEvent;
        protected enum DragType
        {
            None, Select,
            MoveXZ, MoveY, RotLocalXZ, RotY, RotLocalY,
            Scale
        }
        protected enum GizmoType
        {
            Rotate, Move, Scale, None
        }

        public virtual void Initialize(Func<Vector3> position, Func<Vector3> rotation)
        {
            this.BaseScale = transform.localScale;
            this.position = position;
            this.rotation = rotation;
            this.dragPointRenderer = GetComponent<Renderer>();
            this.dragPointCollider = GetComponent<Collider>();
            this.DragPointVisible = true;
        }

        public virtual BaseDrag Initialize(Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            this.Initialize(position, rotation);
            this.meido = meido;
            this.maid = meido.Maid;
            isPlaying = !meido.IsStop;
            return this;
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

        protected void SetGizmo(GizmoType type)
        {
            if (type == GizmoType.Move)
            {
                gizmo.eAxis = true;
                gizmo.eRotate = false;
                gizmo.eScal = false;
                GizmoVisible = true;
            }
            else if (type == GizmoType.Rotate)
            {
                gizmo.eAxis = false;
                gizmo.eRotate = true;
                gizmo.eScal = false;
                GizmoVisible = true;
            }
            else if (type == GizmoType.Scale)
            {
                gizmo.eAxis = false;
                gizmo.eRotate = false;
                gizmo.eScal = true;
                GizmoVisible = true;
            }
            else if (type == GizmoType.None)
            {
                gizmo.eAxis = false;
                gizmo.eRotate = false;
                gizmo.eScal = false;
                GizmoVisible = false;
            }
        }

        public void SetDragProp(bool gizmoActive, bool dragPointActive, bool dragPointVisible)
        {
            this.GizmoActive = gizmoActive;
            this.DragPointActive = dragPointActive;
            this.DragPointVisible = dragPointVisible;
        }

        protected virtual void InitializeDrag()
        {
            worldPoint = Camera.main.WorldToScreenPoint(transform.position);
            mousePos = Input.mousePosition;

            isPlaying = !meido?.IsStop ?? false;
        }

        protected virtual void DoubleClick() { }
        protected abstract void Drag();
        protected abstract void GetDragType();
        protected virtual void OnMouseUp()
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

        protected virtual void Update()
        {
            GetDragType();

            reInitDrag = dragType != dragTypeOld;

            dragTypeOld = dragType;

            transform.position = position();
            transform.eulerAngles = rotation();

            if (GizmoActive)
            {
                if (meido != null && IsGizmoDrag)
                {
                    meido.IsStop = true;
                    isPlaying = false;
                }

                if (gizmoType != gizmoTypeOld) SetGizmo(gizmoType);

                gizmoTypeOld = gizmoType;
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
