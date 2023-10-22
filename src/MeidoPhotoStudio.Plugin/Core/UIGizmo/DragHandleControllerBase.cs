using System;

using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class DragHandleControllerBase : IModalDragHandle
{
    protected static readonly UnityEngine.Camera Camera = GameMain.Instance.MainCamera.camera;

    private bool enabled = true;
    private DragHandleMode currentDragHandleMode;

    public DragHandleControllerBase(DragHandle dragHandle) =>
        DragHandle = dragHandle ? dragHandle : throw new ArgumentNullException(nameof(dragHandle));

    public DragHandleControllerBase(DragHandle dragHandle, CustomGizmo gizmo)
        : this(dragHandle) =>
        Gizmo = gizmo ? gizmo : throw new ArgumentNullException(nameof(dragHandle));

    public bool Enabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle is destroyed.")
                : enabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle is destroyed.");

            enabled = value;

            if (enabled)
            {
                OnDragHandleModeChanged();
            }
            else
            {
                DragHandle.gameObject.SetActive(false);
            }
        }
    }

    public bool GizmoEnabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle is destroyed")
                : Gizmo && Gizmo.GizmoVisible;
        set
        {
            if (!Gizmo)
                return;

            Gizmo.GizmoVisible = value;

            if (value)
                OnDragHandleModeChanged();
        }
    }

    public CustomGizmo.GizmoMode GizmoMode
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle is destroyed")
                : Gizmo
                    ? Gizmo.Mode
                    : CustomGizmo.GizmoMode.Local;
        set
        {
            if (!Gizmo)
                return;

            Gizmo.Mode = value;
        }
    }

    public DragHandleMode CurrentDragType
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle is destroyed.")
                : currentDragHandleMode;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle is destroyed.");

            if (value == currentDragHandleMode)
                return;

            currentDragHandleMode = value;

            OnDragHandleModeChanged();
        }
    }

    public bool Destroyed { get; private set; }

    protected DragHandle DragHandle { get; }

    protected CustomGizmo Gizmo { get; set; }

    public void Destroy()
    {
        if (Destroyed)
            return;

        OnDestroying();

        if (DragHandle)
            UnityEngine.Object.Destroy(DragHandle.gameObject);

        if (Gizmo)
            UnityEngine.Object.Destroy(Gizmo.gameObject);

        Destroyed = true;
    }

    protected virtual void OnDestroying()
    {
    }

    protected abstract void OnDragHandleModeChanged();
}
