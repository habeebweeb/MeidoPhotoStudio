using UnityEngine.Events;

namespace MeidoPhotoStudio.Plugin.Framework.UIGizmo;

/// <summary>Gizmo.</summary>
public partial class CustomGizmo : GizmoRender
{
    public GizmoMode Mode;

    private static readonly Camera Camera = GameMain.Instance.MainCamera.camera;

    private bool clicked;
    private GizmoType gizmoType;
    private Transform target;
    private bool hasAlternateTarget;
    private Transform positionTransform;
    private Vector3 positionOld = Vector3.zero;
    private Vector3 deltaPosition = Vector3.zero;
    private Vector3 deltaLocalPosition = Vector3.zero;
    private Quaternion rotationOld = Quaternion.identity;
    private Quaternion deltaRotation = Quaternion.identity;
    private Quaternion deltaLocalRotation = Quaternion.identity;
    private Vector3 deltaScale = Vector3.zero;
    private Vector3 scaleOld = Vector3.one;

    public enum GizmoType
    {
        Rotate,
        Move,
        Scale,
    }

    public enum GizmoMode
    {
        Local,
        World,
        Global,
    }

    public UnityEvent Dragging { get; } = new();

    public UnityEvent Clicked { get; } = new();

    public UnityEvent Released { get; } = new();

    public UnityEvent Cancelled { get; } = new();

    public GizmoType CurrentGizmoType
    {
        get => gizmoType;
        set
        {
            if (value == gizmoType)
                return;

            gizmoType = value;

            eAxis = gizmoType is GizmoType.Move;
            eScal = gizmoType is GizmoType.Scale;
            eRotate = gizmoType is GizmoType.Rotate;
        }
    }

    public bool Holding =>
        GizmoVisible && is_drag_ && beSelectedType is not MOVETYPE.NONE;

    public bool GizmoVisible
    {
        get => Visible;
        set
        {
            if (value && is_drag_)
                is_drag_ = false;

            Visible = value;
        }
    }

    public static CustomGizmo Make(Transform target, float scale = 0.25f, GizmoMode mode = GizmoMode.Local)
    {
        var gizmoGo = new GameObject($"[MPS Gizmo {target.gameObject.name}]");

        gizmoGo.transform.SetParent(target);

        var gizmo = gizmoGo.AddComponent<CustomGizmo>();

        gizmo.target = target;
        gizmo.lineRSelectedThick = 0.25f;
        gizmo.offsetScale = scale;
        gizmo.Mode = mode;
        gizmo.CurrentGizmoType = GizmoType.Rotate;

        return gizmo;
    }

    public override void Update()
    {
        if (GameMain.Instance.VRMode)
            return;

        BeginUpdate();

        if (Holding)
        {
            if (NInput.GetMouseButtonDown(1))
            {
                is_drag_ = false;
                Cancelled.Invoke();
            }
            else
            {
                SetTargetTransform();
                CheckDragged();
            }
        }

        SetTransform();

        EndUpdate();
    }

    public override void OnRenderObject()
    {
        if (GameMain.Instance.VRMode)
            return;

        if (global_control_lock)
            return;

        var startMoveType = beSelectedType;

        base.OnRenderObject();

        if (startMoveType == beSelectedType)
            return;

        if (startMoveType is MOVETYPE.NONE && NInput.GetMouseButton(0))
        {
            clicked = true;
            Clicked.Invoke();
        }
        else if (beSelectedType is MOVETYPE.NONE)
        {
            clicked = false;
            Released.Invoke();
        }
    }

    public void SetAlternateTarget(Transform trans)
    {
        if (trans == target || trans == positionTransform)
            return;

        positionTransform = trans;
        hasAlternateTarget = trans != null;
    }

    public void SetVisibleRotateHandles(bool x = true, bool y = true, bool z = true)
    {
        VisibleRotateX = x;
        VisibleRotateY = y;
        VisibleRotateZ = z;
    }

    private void OnDisable()
    {
        if (beSelectedType is not MOVETYPE.NONE && local_control_lock_)
            local_control_lock_ = false;

        if (clicked)
        {
            clicked = false;

            Released.Invoke();
        }
    }

    private void OnEnable() =>
        is_drag_ = false;

    private void OnDestroy()
    {
        Clicked.RemoveAllListeners();
        Dragging.RemoveAllListeners();
        Released.RemoveAllListeners();
        Cancelled.RemoveAllListeners();

        if (beSelectedType is not MOVETYPE.NONE && local_control_lock_)
            local_control_lock_ = false;

        if (clicked)
            Released.Invoke();
    }

    private void BeginUpdate()
    {
        var rotation = transform.rotation;

        deltaPosition = transform.position - positionOld;
        deltaRotation = rotation * Quaternion.Inverse(rotationOld);
        deltaLocalPosition = transform.InverseTransformVector(deltaPosition);
        deltaLocalRotation = Quaternion.Inverse(rotationOld) * rotation;
        deltaScale = transform.localScale - scaleOld;
    }

    private void EndUpdate()
    {
        var transform = this.transform;

        positionOld = transform.position;
        rotationOld = transform.rotation;
        scaleOld = transform.localScale;
    }

    private void SetTargetTransform()
    {
        if (Mode is GizmoMode.Local)
        {
            target.position += target.transform.TransformVector(deltaLocalPosition).normalized
                * deltaLocalPosition.magnitude;
            target.rotation *= deltaLocalRotation;
        }
        else if (Mode is GizmoMode.World or GizmoMode.Global)
        {
            target.position += deltaPosition;
            target.rotation = deltaRotation * target.rotation;
        }

        var newScale = target.localScale + deltaScale;

        if (newScale.x < 0f || newScale.y < 0f || newScale.z < 0f)
            return;

        target.localScale = newScale;
    }

    private void CheckDragged()
    {
        var dragged = Mode switch
        {
            GizmoMode.Local => deltaLocalRotation != Quaternion.identity || deltaLocalPosition != Vector3.zero
                || deltaScale != Vector3.zero,
            GizmoMode.World or GizmoMode.Global => deltaRotation != Quaternion.identity
                || deltaPosition != Vector3.zero || deltaScale != Vector3.zero,
            _ => throw new ArgumentOutOfRangeException(nameof(Mode)),
        };

        if (!dragged)
            return;

        Dragging.Invoke();
    }

    private void SetTransform()
    {
        var transform = this.transform;

        var position = (hasAlternateTarget ? positionTransform : target).position;
        var rotation = this switch
        {
            { CurrentGizmoType: GizmoType.Scale } or { Mode: GizmoMode.Local } => target.rotation,
            { Mode: GizmoMode.World } => Quaternion.identity,
            { Mode: GizmoMode.Global } => Quaternion.LookRotation(transform.position - Camera.transform.position),
            _ => target.rotation,
        };

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = target.localScale;
    }
}
