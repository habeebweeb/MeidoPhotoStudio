using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    // TODO: Finalize dragpopint scaling
    internal abstract class BaseDrag : MonoBehaviour
    {
        private const float doubleClickSensitivity = 0.3f;
        protected const int upperArm = 0;
        protected const int foreArm = 1;
        protected const int hand = 2;
        protected const int upperArmRot = 0;
        protected const int handRot = 1;
        private GameObject gizmoGo;
        protected Maid maid;
        protected Meido meido;
        protected Func<Vector3> position;
        protected Func<Vector3> rotation;
        protected Renderer dragPointRenderer;
        protected Collider dragPointCollider;
        protected Vector3 worldPoint;
        protected Vector3 mousePos;
        private DragType dragType = DragType.None;
        protected DragType CurrentDragType
        {
            get => dragType;
            set
            {
                dragType = value;
                reInitDrag = dragType != dragTypeOld;
                dragTypeOld = dragType;
            }
        }
        protected DragType dragTypeOld;
        protected float doubleClickStart = 0f;
        protected bool reInitDrag = false;
        protected bool isPlaying;
        protected CustomGizmo gizmo;
        protected GizmoType CurrentGizmoType
        {
            get => gizmo?.CurrentGizmoType ?? GizmoType.None;
            set
            {
                if (gizmo != null)
                {
                    if (GizmoActive) gizmo.CurrentGizmoType = value;
                }
            }
        }
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
                if (gizmoGo != null)
                {
                    gizmoActive = value;
                    gizmoGo.SetActive(gizmoActive);
                    GizmoVisible = gizmoActive;
                }
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
        public GizmoMode CurrentGizmoMode
        {
            get => gizmo?.gizmoMode ?? GizmoMode.Local;
            set
            {
                if (gizmo != null)
                {
                    if (GizmoActive) gizmo.gizmoMode = value;
                }
            }
        }
        public event EventHandler DragEvent;
        protected enum DragType
        {
            None, Select, Delete,
            MoveXZ, MoveY, RotLocalXZ, RotY, RotLocalY,
            Scale
        }

        public static Material LightBlue = new Material(Shader.Find("Transparent/Diffuse"))
        {
            color = new Color(0.4f, 0.4f, 1f, 0.3f)
        };

        public static Material Blue = new Material(Shader.Find("Transparent/Diffuse"))
        {
            color = new Color(0.5f, 0.5f, 1f, 0.8f)
        };

        public static GameObject MakeDragPoint(PrimitiveType primitiveType, Vector3 scale, Material material)
        {
            GameObject dragPoint = GameObject.CreatePrimitive(primitiveType);
            dragPoint.transform.localScale = scale;
            dragPoint.GetComponent<Renderer>().material = material;
            dragPoint.layer = 8;
            return dragPoint;
        }

        public BaseDrag Initialize(Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            this.InitializeDragPoint(position, rotation);
            this.meido = meido;
            this.maid = meido.Maid;
            isPlaying = !meido.IsStop;
            return this;
        }

        protected void InitializeDragPoint(Func<Vector3> position, Func<Vector3> rotation)
        {
            this.BaseScale = transform.localScale;
            this.position = position;
            this.rotation = rotation;
            this.dragPointRenderer = GetComponent<Renderer>();
            this.dragPointCollider = GetComponent<Collider>();
            this.DragPointVisible = true;
        }

        protected void InitializeGizmo(Transform target, float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            gizmoGo = CustomGizmo.MakeGizmo(target, scale, mode);
            gizmo = gizmoGo.GetComponent<CustomGizmo>();
            if (meido != null)
            {
                gizmo.GizmoDrag += (s, a) =>
                {
                    meido.IsStop = true;
                    isPlaying = false;
                };
            }

            GizmoActive = false;
            GizmoVisible = false;
        }

        public void SetDragProp(bool gizmoActive, bool dragPointActive, bool dragPointVisible)
        {
            this.GizmoActive = gizmoActive;
            this.DragPointActive = dragPointActive;
            this.DragPointVisible = dragPointVisible;
        }

        public void SetDragProp(bool gizmoActive, bool dragPointActive, bool dragPointVisible, GizmoMode mode)
        {
            SetDragProp(gizmoActive, dragPointActive, dragPointVisible);
            this.CurrentGizmoMode = mode;
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

            transform.position = position();
            transform.eulerAngles = rotation();
        }

        protected virtual void OnMouseDown()
        {
            InitializeDrag();
        }

        protected virtual void OnMouseDrag()
        {
            if (CurrentDragType == DragType.Select) return;

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
