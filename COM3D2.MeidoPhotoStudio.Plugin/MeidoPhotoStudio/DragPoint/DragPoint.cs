using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    internal abstract class DragPoint : MonoBehaviour
    {
        public const float defaultAlpha = 0.75f;
        private static GameObject dragPointParent = new GameObject("[MPS DragPoint Parent]");
        private const float doubleClickSensitivity = 0.3f;
        private Func<Vector3> position;
        private Func<Vector3> rotation;
        private Collider collider;
        private Renderer renderer;
        private bool reinitializeDrag;
        protected bool Transforming => CurrentDragType >= DragType.MoveXZ;
        protected bool Special => CurrentDragType == DragType.Select || CurrentDragType == DragType.Delete;
        protected bool Moving => CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY;
        protected bool Rotating => CurrentDragType >= DragType.RotLocalXZ && CurrentDragType <= DragType.RotLocalY;
        protected bool Scaling => CurrentDragType == DragType.Scale;
        protected bool Selecting => CurrentDragType == DragType.Select;
        protected bool Deleting => CurrentDragType == DragType.Delete;
        private Vector3 startMousePosition;
        protected static Camera camera = GameMain.Instance.MainCamera.camera;
        public enum DragType
        {
            None, Ignore, Select, Delete,
            MoveXZ, MoveY,
            RotLocalXZ, RotY, RotLocalY,
            Scale
        }
        public Transform MyObject { get; protected set; }
        public GameObject MyGameObject => MyObject.gameObject;
        private float startDoubleClick;
        private Vector3 screenPoint;
        private Vector3 startOffset;
        private Vector3 newOffset;
        public static Material dragPointMaterial = new Material(Shader.Find("CM3D2/Trans_AbsoluteFront"));
        public static readonly Color defaultColour = new Color(0f, 0f, 0f, defaultAlpha);
        public Vector3 BaseScale { get; private set; }
        private float dragPointScale = 1f;
        public float DragPointScale
        {
            get => dragPointScale;
            set
            {
                dragPointScale = value;
                transform.localScale = BaseScale * dragPointScale;
            }
        }
        public GameObject GizmoGo { get; protected set; }
        public CustomGizmo Gizmo { get; protected set; }
        private DragType oldDragType;
        private DragType currentDragType;
        protected DragType CurrentDragType
        {
            get => currentDragType;
            set
            {
                if (value != oldDragType)
                {
                    currentDragType = value;
                    reinitializeDrag = true;
                    oldDragType = currentDragType;
                    ApplyDragType();
                }
            }
        }
        private bool dragPointEnabled = true;
        public bool DragPointEnabled
        {
            get => dragPointEnabled;
            set
            {
                if (dragPointEnabled == value) return;
                dragPointEnabled = value;
                ApplyDragType();
            }
        }
        private bool gizmoEnabled = true;
        public bool GizmoEnabled
        {
            get => GizmoGo != null && gizmoEnabled;
            set
            {
                if (GizmoGo == null || (GizmoGo != null && gizmoEnabled == value)) return;
                gizmoEnabled = value;
                ApplyDragType();
            }
        }

        private void Awake()
        {
            this.BaseScale = transform.localScale;
            this.collider = GetComponent<Collider>();
            this.renderer = GetComponent<Renderer>();
            ApplyDragType();
        }

        public static T Make<T>(PrimitiveType primitiveType, Vector3 scale) where T : DragPoint
        {
            GameObject dragPointGo = GameObject.CreatePrimitive(primitiveType);
            dragPointGo.transform.SetParent(dragPointParent.transform, false);
            dragPointGo.transform.localScale = scale;
            dragPointGo.layer = 8;

            T dragPoint = dragPointGo.AddComponent<T>();
            dragPoint.renderer.material = dragPointMaterial;
            dragPoint.renderer.material.color = defaultColour;

            return dragPoint;
        }

        public virtual void Initialize(Func<Vector3> position, Func<Vector3> rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public virtual void Set(Transform myObject)
        {
            this.MyObject = myObject;
            this.gameObject.name = $"[MPS DragPoint: {this.MyObject.name}]";
        }

        public virtual void AddGizmo(float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            Gizmo = CustomGizmo.Make(this.MyObject, scale, mode);
            GizmoGo = Gizmo.gameObject;
            Gizmo.GizmoVisible = false;
            ApplyDragType();
        }

        protected virtual void ApplyDragType() { }

        public void ApplyProperties(bool active = false, bool visible = false, bool gizmo = false)
        {
            this.collider.enabled = active;
            this.renderer.enabled = visible;
            if (this.Gizmo != null) this.Gizmo.GizmoVisible = gizmo;
        }

        protected void ApplyColour(Color colour) => this.renderer.material.color = colour;

        protected void ApplyColour(float r, float g, float b, float a = defaultAlpha)
        {
            ApplyColour(new Color(r, g, b, a));
        }

        protected Vector3 MouseDelta() => Input.mousePosition - startMousePosition;

        protected bool OtherDragType()
        {
            return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)
                || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.C);
        }

        protected Vector3 CursorPosition()
        {
            Vector3 mousePosition = Input.mousePosition;
            return camera.ScreenToWorldPoint(
                new Vector3(mousePosition.x, mousePosition.y, screenPoint.z)
            ) + startOffset - newOffset;
        }

        protected virtual void Update()
        {
            transform.position = position();
            transform.eulerAngles = rotation();

            UpdateDragType();
        }

        protected virtual void OnMouseDown()
        {
            screenPoint = camera.WorldToScreenPoint(transform.position);
            startMousePosition = Input.mousePosition;
            startOffset = transform.position - camera.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z)
            );
            newOffset = transform.position - MyObject.position;
        }

        protected virtual void OnMouseDrag()
        {
            if (reinitializeDrag)
            {
                reinitializeDrag = false;
                OnMouseDown();
            }

            if (collider.enabled && Input.mousePosition != startMousePosition) Drag();
        }

        protected abstract void UpdateDragType();
        protected abstract void Drag();

        protected virtual void OnMouseUp()
        {
            if ((Time.time - startDoubleClick) < doubleClickSensitivity)
            {
                startDoubleClick = -1f;
                OnDoubleClick();
            }
            else
            {
                startDoubleClick = Time.time;
            }
        }

        protected virtual void OnDoubleClick() { }

        private void OnEnable()
        {
            if (position != null)
            {
                transform.position = position();
                transform.eulerAngles = rotation();
            }
            if (GizmoGo != null) GizmoGo.SetActive(true);
            ApplyDragType();
        }

        private void OnDisable()
        {
            if (GizmoGo != null) GizmoGo.SetActive(false);
        }

        protected virtual void OnDestroy() => GameObject.Destroy(GizmoGo);
    }
}
