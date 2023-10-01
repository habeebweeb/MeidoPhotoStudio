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
    private DragHandleMode oldDragHandleMode;
    private DragHandleMode currentDragHandleMode;
    private bool dragPointEnabled = true;
    private float dragPointScale = 1f;
    private bool gizmoEnabled = true;

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

    public DragHandleMode CurrentDragType
    {
        get => currentDragHandleMode;
        set
        {
            if (value == oldDragHandleMode)
                return;

            currentDragHandleMode = value;
            reinitializeDrag = true;
            oldDragHandleMode = currentDragHandleMode;
            ApplyDragType();
        }
    }

    protected bool Transforming =>
        CurrentDragType is >= DragHandleMode.MoveWorldXZ and <= DragHandleMode.Scale;

    protected bool Special =>
        CurrentDragType is DragHandleMode.Select or DragHandleMode.Delete;

    protected bool Moving =>
        CurrentDragType is DragHandleMode.MoveWorldXZ or DragHandleMode.MoveWorldY;

    protected bool Rotating =>
        CurrentDragType is >= DragHandleMode.RotateWorldY and <= DragHandleMode.RotateLocalXZ;

    protected bool Scaling =>
        CurrentDragType is DragHandleMode.Scale;

    protected bool Selecting =>
        CurrentDragType is DragHandleMode.Select;

    protected bool Deleting =>
        CurrentDragType is DragHandleMode.Delete;

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

    protected abstract void Drag();

    protected virtual void ApplyDragType()
    {
    }

    protected virtual void Update()
    {
        transform.position = position();
        transform.eulerAngles = rotation();
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

    protected Vector3 CursorPosition()
    {
        var mousePosition = Utility.MousePosition;

        return camera.ScreenToWorldPoint(new(mousePosition.x, mousePosition.y, screenPoint.z)) + startOffset
            - newOffset;
    }

    private static GameObject DragPointParent()
    {
        if (dragPointParent)
            return dragPointParent;

        const string dragPointParentName = "[MPS DragPoint Parent]";

        var findParent = GameObject.Find(dragPointParentName);

        return findParent ? (dragPointParent = findParent) : (dragPointParent = new(dragPointParentName));
    }

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
