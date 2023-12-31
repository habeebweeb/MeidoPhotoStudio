using System;

using UnityEngine;
using UnityEngine.Events;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.UIGizmo;

/// <summary>Drag handle.</summary>
public partial class DragHandle : MonoBehaviour
{
    private static readonly int DragHandleLayer = LayerMask.NameToLayer("AbsolutFront");
    private static readonly Func<Vector3> DefaultPositionDelegate = () => Vector3.zero;
    private static readonly Func<Quaternion> DefaultRotationDelegate = () => Quaternion.identity;

    private Vector3 oldMousePosition;
    private Vector3 handleMousePosition;
    private bool constantSize;
    private Renderer dragHandleRenderer;
    private int instanceId;
    private Camera mainCamera;
    private Vector3 mouseOffset;
    private MoveType movementType;
    private Func<Vector3> positionDelegate = DefaultPositionDelegate;
    private Func<Quaternion> rotationDelegate = DefaultRotationDelegate;
    private Vector3 scale = Vector3.one;
    private float size = 1f;
    private Vector3 targetOffset;
    private Plane positionPlane;
    private Vector3 localClickPoint;

    public enum MoveType
    {
        None,
        X,
        Y,
        Z,
        XY,
        XZ,
        YZ,
        All,
    }

    public Color Color
    {
        get => Renderer.material.color;
        set => Renderer.material.color = value;
    }

    public bool ConstantSize
    {
        get => constantSize;
        set
        {
            if (value == constantSize)
                return;

            constantSize = value;

            if (!constantSize)
                transform.localScale = Scale * Size;
            else
                UpdateSize();
        }
    }

    public UnityEvent Clicked { get; } = new();

    public UnityEvent Dragging { get; } = new();

    public UnityEvent DoubleClicked { get; } = new();

    public UnityEvent Released { get; } = new();

    public MoveType MovementType
    {
        get => movementType;
        set
        {
            var newMovementType = Target ? value : MoveType.None;

            movementType = newMovementType;

            Reset();
        }
    }

    public Func<Vector3> PositionDelegate
    {
        get => positionDelegate;
        set
        {
            var newDelegate = value;

            if (value is null)
                newDelegate = DefaultPositionDelegate;

            positionDelegate = newDelegate;
        }
    }

    public int Priority { get; set; }

    public Func<Quaternion> RotationDelegate
    {
        get => rotationDelegate;
        set
        {
            var newDelegate = value;

            if (value is null)
                newDelegate = DefaultRotationDelegate;

            rotationDelegate = newDelegate;
        }
    }

    public Vector3 Scale
    {
        get => scale;
        set
        {
            if (scale == value)
                return;

            scale = value;

            transform.localScale = Size * Scale;
        }
    }

    public float Size
    {
        get => size;
        set
        {
            if (Mathf.Approximately(size, value))
                return;

            size = value;

            transform.localScale = Size * Scale;
        }
    }

    public Transform Target { get; private set; }

    public bool Visible
    {
        get => Renderer.enabled;
        set => Renderer.enabled = value;
    }

    private Renderer Renderer =>
        dragHandleRenderer ? dragHandleRenderer : dragHandleRenderer = GetComponent<Renderer>();

    private bool MoveWithMouse =>
        movementType is not MoveType.None;

    private bool Selected =>
        ClickHandler.SelectedDragHandleID == instanceId;

    private static bool OnlyLeftClickPressed() =>
        UInput.GetMouseButton(0) && !UInput.GetMouseButton(1) && !UInput.GetMouseButton(2);

    private void Awake()
    {
        mainCamera = GameMain.Instance.MainCamera.camera;
        instanceId = GetInstanceID();
    }

    private void Start() =>
        UpdatePositionAndRotation();

    private void Update()
    {
        UpdateSize();

        if (Selected && OnlyLeftClickPressed())
        {
            if (MoveWithMouse)
            {
                UpdateHandleMousePosition();
                MoveToMouse();
            }

            Drag();
        }

        UpdatePositionAndRotation();

        void UpdateHandleMousePosition()
        {
            var mousePositionDelta = UInput.mousePosition - oldMousePosition;

            oldMousePosition = UInput.mousePosition;
            handleMousePosition += mousePositionDelta;
        }
    }

    private void OnEnable()
    {
        UpdatePositionAndRotation();

        if (!Selected)
            return;

        Reset();
    }

    private void Click() =>
        Clicked.Invoke();

    private void Drag() =>
        Dragging.Invoke();

    private void Release() =>
        Released.Invoke();

    private void DoubleClick() =>
        DoubleClicked.Invoke();

    private void Select(RaycastHit clickRaycast)
    {
        localClickPoint = transform.InverseTransformPoint(clickRaycast.point);

        Reset();
    }

    private void UpdatePositionAndRotation()
    {
        try
        {
            transform.SetPositionAndRotation(positionDelegate(), rotationDelegate());
        }
        catch
        {
            PositionDelegate = null;
            RotationDelegate = null;

            throw;
        }
    }

    private void UpdateSize()
    {
        if (!ConstantSize)
            return;

        var distance = Vector3.Distance(mainCamera.transform.position, transform.position);
        var frustumHeight = 2f * distance * Mathf.Tan(mainCamera.fieldOfView / 2f * Mathf.Deg2Rad);
        var scale = frustumHeight * Size * 0.05f;

        transform.localScale = new Vector3(scale, scale, scale);
    }

    private void MoveToMouse()
    {
        var newPosition = MouseToWorldPoint() + mouseOffset - targetOffset;

        if (MovementType is not MoveType.All)
            newPosition = ConstrainPosition(newPosition);

        Target.position = newPosition;

        Vector3 ConstrainPosition(Vector3 position)
        {
            var originalPosition = Target.position;

            return MovementType switch
            {
                MoveType.XY => position with { z = originalPosition.z },
                MoveType.YZ => position with { x = originalPosition.x },
                MoveType.XZ => position with { y = originalPosition.y },
                MoveType.X => position with { y = originalPosition.y, z = originalPosition.z },
                MoveType.Y => position with { x = originalPosition.x, z = originalPosition.z },
                MoveType.Z => position with { x = originalPosition.x, y = originalPosition.y },
                MoveType.None or MoveType.All or _ => position,
            };
        }
    }

    private Vector3 MouseToWorldPoint()
    {
        var mousePoint = handleMousePosition;

        return MovementType is MoveType.All
            ? WorldPoint(mousePoint)
            : PointAlongPositionPlane(mousePoint);

        Vector3 PointAlongPositionPlane(Vector3 mousePoint)
        {
            var screenRay = mainCamera.ScreenPointToRay(mousePoint);

            return positionPlane.Raycast(screenRay, out var enter)
                ? screenRay.GetPoint(enter)
                : Target.position - mouseOffset + targetOffset;
        }

        Vector3 WorldPoint(Vector3 mousePoint)
        {
            var distanceFromCamera = mainCamera.WorldToScreenPoint(Target.position).z;
            var mousePosition = new Vector3(mousePoint.x, mousePoint.y, distanceFromCamera);

            return mainCamera.ScreenToWorldPoint(mousePosition);
        }
    }

    private void Reset()
    {
        ResetHandleMousePosition();

        if (!Target)
            return;

        UpdatePositionPlane();
        UpdateOffsets();

        void ResetHandleMousePosition()
        {
            oldMousePosition = UInput.mousePosition;
            handleMousePosition = mainCamera.WorldToScreenPoint(transform.TransformPoint(localClickPoint));
        }

        void UpdatePositionPlane()
        {
            var position = transform.TransformPoint(localClickPoint);

            positionPlane = MovementType switch
            {
                MoveType.XY => new Plane(Vector3.forward, position),
                MoveType.YZ => new Plane(Vector3.right, position),
                MoveType.XZ or MoveType.X or MoveType.Z => new Plane(Vector3.up, position),
                MoveType.Y =>
                    new Plane(Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up), position),
                MoveType.None or MoveType.All or _ => new Plane(Vector3.up, position),
            };
        }

        void UpdateOffsets()
        {
            var position = transform.TransformPoint(localClickPoint);

            mouseOffset = position - MouseToWorldPoint();
            targetOffset = position - Target.position;
        }
    }
}
