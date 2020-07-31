using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    internal abstract class DragPoint : MonoBehaviour
    {
        private const float doubleClickSensitivity = 0.3f;
        private Func<Vector3> position;
        private Func<Vector3> rotation;
        private Collider collider;
        private Renderer renderer;
        private bool reinitializeDrag;
        protected bool Transforming => CurrentDragType >= DragType.MoveXZ && CurrentDragType <= DragType.RotLocalY;
        protected bool Moving => CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY;
        protected bool Rotating => CurrentDragType >= DragType.RotLocalXZ && CurrentDragType <= DragType.RotLocalY;
        protected bool Special => CurrentDragType == DragType.Select || CurrentDragType == DragType.Delete;
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
        public static Material LightBlue = new Material(Shader.Find("Transparent/Diffuse"))
        {
            color = new Color(0.4f, 0.4f, 1f, 0.3f)
        };
        public static Material Blue = new Material(Shader.Find("Transparent/Diffuse"))
        {
            color = new Color(0.5f, 0.5f, 1f, 0.8f)
        };
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

        private void Awake()
        {
            this.BaseScale = transform.localScale;
            this.collider = GetComponent<Collider>();
            this.renderer = GetComponent<Renderer>();
            ApplyDragType();
        }

        public static T Make<T>(PrimitiveType primitiveType, Vector3 scale, Material material) where T : DragPoint
        {
            GameObject dragPoint = GameObject.CreatePrimitive(primitiveType);
            dragPoint.transform.localScale = scale;
            dragPoint.GetComponent<Renderer>().material = material;
            dragPoint.layer = 8;

            return dragPoint.AddComponent<T>();
        }

        public virtual void Initialize(Func<Vector3> position, Func<Vector3> rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public virtual void Set(Transform myObject)
        {
            this.MyObject = myObject;
        }

        public virtual void AddGizmo(float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
        {
            Gizmo = CustomGizmo.Make(this.MyObject, scale, mode);
            GizmoGo = Gizmo.gameObject;
            GizmoGo.SetActive(false);
            ApplyDragType();
        }

        protected virtual void ApplyDragType() { }

        public void ApplyProperties(bool active = false, bool visible = false, bool gizmo = false)
        {
            this.collider.enabled = active;
            this.renderer.enabled = visible;
            this.GizmoGo?.SetActive(gizmo);
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
            ApplyDragType();
        }

        private void OnDisable()
        {
            if (GizmoGo) GizmoGo.SetActive(false);
        }

        protected virtual void OnDestroy() => GameObject.Destroy(GizmoGo);
    }
}
