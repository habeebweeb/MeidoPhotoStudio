using System;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.CustomGizmo;

using Input = MeidoPhotoStudio.Plugin.InputManager;

namespace MeidoPhotoStudio.Plugin;

public abstract class DragPointGeneral : DragPoint
{
    public const float SmallCube = 0.5f;

    public static readonly Color MoveColour = new(0.2f, 0.5f, 0.95f, DefaultAlpha);
    public static readonly Color RotateColour = new(0.2f, 0.75f, 0.3f, DefaultAlpha);
    public static readonly Color ScaleColour = new(0.8f, 0.7f, 0.3f, DefaultAlpha);
    public static readonly Color SelectColour = new(0.9f, 0.5f, 1f, DefaultAlpha);
    public static readonly Color DeleteColour = new(1f, 0.1f, 0.1f, DefaultAlpha);

    private float currentScale;
    private bool scaling;
    private Quaternion currentRotation;

    public event EventHandler Move;

    public event EventHandler Rotate;

    public event EventHandler Scale;

    public event EventHandler EndScale;

    public event EventHandler Delete;

    public event EventHandler Select;

    public Quaternion DefaultRotation { get; set; } = Quaternion.identity;

    public Vector3 DefaultPosition { get; set; } = Vector3.zero;

    public Vector3 DefaultScale { get; set; } = Vector3.one;

    public float ScaleFactor { get; set; } = 1f;

    public bool ConstantScale { get; set; }

    public override void AddGizmo(float scale = 0.35f, GizmoMode mode = GizmoMode.Local)
    {
        base.AddGizmo(scale, mode);

        Gizmo.GizmoDrag += (_, _) =>
        {
            if (Gizmo.CurrentGizmoType is GizmoType.Rotate)
                OnRotate();
        };
    }

    protected override void Update()
    {
        base.Update();

        if (!ConstantScale)
            return;

        var distance = Vector3.Distance(camera.transform.position, transform.position);

        transform.localScale = Vector3.one * (0.4f * BaseScale.x * DragPointScale * distance);
    }

    protected override void UpdateDragType()
    {
        var shift = Input.Shift;

        if (Input.GetKey(MpsKey.DragSelect))
        {
            CurrentDragType = DragType.Select;
        }
        else if (Input.GetKey(MpsKey.DragDelete))
        {
            CurrentDragType = DragType.Delete;
        }
        else if (Input.GetKey(MpsKey.DragMove))
        {
            if (Input.Control)
                CurrentDragType = DragType.MoveY;
            else
                CurrentDragType = shift ? DragType.RotY : DragType.MoveXZ;
        }
        else if (Input.GetKey(MpsKey.DragRotate))
        {
            CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
        }
        else if (Input.GetKey(MpsKey.DragScale))
        {
            CurrentDragType = DragType.Scale;
        }
        else
        {
            CurrentDragType = DragType.None;
        }
    }

    protected override void OnMouseDown()
    {
        if (Deleting)
        {
            OnDelete();

            return;
        }

        if (Selecting)
        {
            OnSelect();

            return;
        }

        base.OnMouseDown();

        currentScale = MyObject.localScale.x;
        currentRotation = MyObject.rotation;
    }

    protected override void OnDoubleClick()
    {
        if (Scaling)
        {
            MyObject.localScale = DefaultScale;
            OnScale();
            OnEndScale();
        }

        if (Rotating)
        {
            ResetRotation();
            OnRotate();
        }

        if (Moving)
        {
            ResetPosition();
            OnMove();
        }
    }

    protected override void OnMouseUp()
    {
        base.OnMouseUp();

        if (scaling)
        {
            scaling = false;
            OnScale();
            OnEndScale();
        }
    }

    protected override void Drag()
    {
        if (CurrentDragType is DragType.Select or DragType.Delete)
            return;

        var cursorPosition = CursorPosition();
        var mouseDelta = MouseDelta();

        // CurrentDragType can only be one thing at a time afaik so maybe refactor to else if chain
        if (CurrentDragType is DragType.MoveXZ)
        {
            MyObject.position = new(cursorPosition.x, MyObject.position.y, cursorPosition.z);

            OnMove();
        }

        if (CurrentDragType is DragType.MoveY)
        {
            MyObject.position = new(MyObject.position.x, cursorPosition.y, MyObject.position.z);

            OnMove();
        }

        if (CurrentDragType is DragType.RotY)
        {
            MyObject.rotation = currentRotation;
            MyObject.Rotate(Vector3.up, -mouseDelta.x / 3f, Space.World);
            OnRotate();
        }

        if (CurrentDragType is DragType.RotLocalXZ)
        {
            MyObject.rotation = currentRotation;

            var forward = camera.transform.forward;
            var right = camera.transform.right;

            forward.y = 0f;
            right.y = 0f;
            MyObject.Rotate(forward, -mouseDelta.x / 6f, Space.World);
            MyObject.Rotate(right, mouseDelta.y / 4f, Space.World);

            OnRotate();
        }

        if (CurrentDragType is DragType.RotLocalY)
        {
            MyObject.rotation = currentRotation;
            MyObject.Rotate(Vector3.up * -mouseDelta.x / 2.2f);

            OnRotate();
        }

        if (CurrentDragType is DragType.Scale)
        {
            scaling = true;

            var scale = currentScale + mouseDelta.y / 200f * ScaleFactor;

            if (scale < 0f)
                scale = 0f;

            MyObject.localScale = new(scale, scale, scale);

            OnScale();
        }
    }

    protected virtual void ApplyColours()
    {
        var colour = MoveColour;

        if (Rotating)
            colour = RotateColour;
        else if (Scaling)
            colour = ScaleColour;
        else if (Selecting)
            colour = SelectColour;
        else if (Deleting)
            colour = DeleteColour;

        ApplyColour(colour);
    }

    protected virtual void ResetPosition() =>
        MyObject.position = DefaultPosition;

    protected virtual void ResetRotation() =>
        MyObject.rotation = DefaultRotation;

    protected virtual void OnEndScale() =>
        OnEvent(EndScale);

    protected virtual void OnScale() =>
        OnEvent(Scale);

    protected virtual void OnMove() =>
        OnEvent(Move);

    protected virtual void OnRotate() =>
        OnEvent(Rotate);

    protected virtual void OnSelect() =>
        OnEvent(Select);

    protected virtual void OnDelete() =>
        OnEvent(Delete);

    private void OnEvent(EventHandler handler) =>
        handler?.Invoke(this, EventArgs.Empty);
}
