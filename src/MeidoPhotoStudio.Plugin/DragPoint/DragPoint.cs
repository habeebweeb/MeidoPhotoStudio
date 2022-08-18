using System;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.CustomGizmo;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPoint : MonoBehaviour
{
    public const float DefaultAlpha = 0.75f;

    public static readonly Color DefaultColour = new(0f, 0f, 0f, 0.4f);

    public static Material DragPointMaterial = new(Shader.Find("CM3D2/Trans_AbsoluteFront"));

    protected static Camera camera = GameMain.Instance.MainCamera.camera;

    private const float DoubleClickSensitivity = 0.3f;

    // TODO: Use this value or just throw it away.
    private static readonly int DragPointLayer = (int)Mathf.Log(LayerMask.GetMask("AbsolutFront"), 2);
    private static GameObject dragPointParent;

    private Func<Vector3> position;
    private Func<Vector3> rotation;
    private Collider collider;
    private Renderer renderer;
    private bool reinitializeDrag;
    private Vector3 startMousePosition;
    private float startDoubleClick;
    private Vector3 screenPoint;
    private Vector3 startOffset;
    private Vector3 newOffset;
    private Vector3 baseScale;
    private DragType oldDragType;
    private DragType currentDragType;
    private bool dragPointEnabled = true;
    private float dragPointScale = 1f;
    private bool gizmoEnabled = true;

    static DragPoint()
    {
        InputManager.Register(MpsKey.DragSelect, KeyCode.A, "Select handle mode");
        InputManager.Register(MpsKey.DragDelete, KeyCode.D, "Delete handle mode");
        InputManager.Register(MpsKey.DragMove, KeyCode.Z, "Move handle mode");
        InputManager.Register(MpsKey.DragRotate, KeyCode.X, "Rotate handle mode");
        InputManager.Register(MpsKey.DragScale, KeyCode.C, "Scale handle mode");
        InputManager.Register(MpsKey.DragFinger, KeyCode.Space, "Show finger handles");
    }

    public enum DragType
    {
        None,
        Ignore,
        Select,
        Delete,
        MoveXZ,
        MoveY,
        RotLocalXZ,
        RotY,
        RotLocalY,
        Scale,
    }

    public Vector3 OriginalScale { get; private set; }

    public Transform MyObject { get; protected set; }

    public GameObject GizmoGo { get; protected set; }

    public CustomGizmo Gizmo { get; protected set; }

    public GameObject MyGameObject =>
        MyObject.gameObject;

    public Vector3 BaseScale
    {
        get => baseScale;
        protected set
        {
            baseScale = value;
            transform.localScale = BaseScale * DragPointScale;
        }
    }

    public float DragPointScale
    {
        get => dragPointScale;
        set
        {
            dragPointScale = value;
            transform.localScale = BaseScale * dragPointScale;
        }
    }

    public bool DragPointEnabled
    {
        get => dragPointEnabled;
        set
        {
            if (dragPointEnabled == value)
                return;

            dragPointEnabled = value;
            ApplyDragType();
        }
    }

    public bool GizmoEnabled
    {
        get => GizmoGo && gizmoEnabled;
        set
        {
            if (!GizmoGo || gizmoEnabled == value)
                return;

            gizmoEnabled = value;
            ApplyDragType();
        }
    }

    protected DragType CurrentDragType
    {
        get => currentDragType;
        set
        {
            if (value == oldDragType)
                return;

            currentDragType = value;
            reinitializeDrag = true;
            oldDragType = currentDragType;
            ApplyDragType();
        }
    }

    protected bool Transforming =>
        CurrentDragType >= DragType.MoveXZ;

    protected bool Special =>
        CurrentDragType is DragType.Select or DragType.Delete;

    protected bool Moving =>
        CurrentDragType is DragType.MoveXZ or DragType.MoveY;

    protected bool Rotating =>
        CurrentDragType is >= DragType.RotLocalXZ and <= DragType.RotLocalY;

    protected bool Scaling =>
        CurrentDragType is DragType.Scale;

    protected bool Selecting =>
        CurrentDragType is DragType.Select;

    protected bool Deleting =>
        CurrentDragType is DragType.Delete;

    public static T Make<T>(PrimitiveType primitiveType, Vector3 scale)
        where T : DragPoint
    {
        var dragPointGo = GameObject.CreatePrimitive(primitiveType);

        dragPointGo.transform.SetParent(DragPointParent().transform, false);
        dragPointGo.transform.localScale = scale;
        dragPointGo.layer = 8;

        var dragPoint = dragPointGo.AddComponent<T>();

        dragPoint.renderer.material = DragPointMaterial;
        dragPoint.renderer.material.color = DefaultColour;

        return dragPoint;
    }

    public virtual void Initialize(Func<Vector3> position, Func<Vector3> rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public virtual void Set(Transform myObject)
    {
        MyObject = myObject;
        gameObject.name = $"[MPS DragPoint: {MyObject.name}]";
    }

    public virtual void AddGizmo(float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
    {
        Gizmo = CustomGizmo.Make(MyObject, scale, mode);
        GizmoGo = Gizmo.gameObject;
        Gizmo.GizmoVisible = false;
        ApplyDragType();
    }

    public void ApplyProperties(bool active = false, bool visible = false, bool gizmo = false)
    {
        collider.enabled = active;
        renderer.enabled = visible;

        if (Gizmo)
            Gizmo.GizmoVisible = gizmo;
    }

    protected abstract void UpdateDragType();

    protected abstract void Drag();

    protected virtual void ApplyDragType()
    {
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
        startMousePosition = Utility.MousePosition;
        startOffset = transform.position
            - camera.ScreenToWorldPoint(new(startMousePosition.x, startMousePosition.y, screenPoint.z));
        newOffset = transform.position - MyObject.position;
    }

    protected virtual void OnMouseDrag()
    {
        if (reinitializeDrag)
        {
            reinitializeDrag = false;
            OnMouseDown();
        }

        if (collider.enabled && startMousePosition != Utility.MousePosition)
            Drag();
    }

    protected virtual void OnMouseUp()
    {
        if (Time.time - startDoubleClick < DoubleClickSensitivity)
        {
            startDoubleClick = -1f;
            OnDoubleClick();
        }
        else
        {
            startDoubleClick = Time.time;
        }
    }

    protected virtual void OnDoubleClick()
    {
    }

    protected virtual void OnDestroy() =>
        Destroy(GizmoGo);

    protected void ApplyColour(Color colour) =>
        renderer.material.color = colour;

    protected void ApplyColour(float r, float g, float b, float a = DefaultAlpha) =>
        ApplyColour(new(r, g, b, a));

    protected Vector3 MouseDelta() =>
        Utility.MousePosition - startMousePosition;

    protected bool OtherDragType() =>
        InputManager.GetKey(MpsKey.DragSelect) || InputManager.GetKey(MpsKey.DragDelete)
        || InputManager.GetKey(MpsKey.DragMove) || InputManager.GetKey(MpsKey.DragRotate)
        || InputManager.GetKey(MpsKey.DragScale) || InputManager.GetKey(MpsKey.DragFinger);

    protected Vector3 CursorPosition()
    {
        var mousePosition = Utility.MousePosition;

        return camera.ScreenToWorldPoint(new(mousePosition.x, mousePosition.y, screenPoint.z)) + startOffset
            - newOffset;
    }

    private static GameObject DragPointParent() =>
        dragPointParent ? dragPointParent : (dragPointParent = new("[MPS DragPoint Parent]"));

    private void Awake()
    {
        BaseScale = OriginalScale = transform.localScale;
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
        ApplyDragType();
    }

    private void OnEnable()
    {
        if (position is not null)
        {
            transform.position = position();
            transform.eulerAngles = rotation();
        }

        if (GizmoGo)
            GizmoGo.SetActive(true);

        ApplyDragType();
    }

    private void OnDisable()
    {
        if (GizmoGo)
            GizmoGo.SetActive(false);
    }
}
