using System;

using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class DragHandleControllerBase
{
    protected static readonly UnityEngine.Camera Camera = GameMain.Instance.MainCamera.camera;

    private static readonly EmptyDragHandleMode EmptyDragHandleMode = new();

    private bool enabled = true;
    private DragHandleMode currentDragHandleMode = EmptyDragHandleMode;

    public DragHandleControllerBase(DragHandle dragHandle)
    {
        DragHandle = dragHandle ? dragHandle : throw new ArgumentNullException(nameof(dragHandle));

        DragHandle.Clicked.AddListener(OnClicked);
        DragHandle.Dragging.AddListener(OnDragging);
        DragHandle.Released.AddListener(OnReleased);
        DragHandle.DoubleClicked.AddListener(OnDoubleClicked);
    }

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
                currentDragHandleMode.OnModeEnter();
            else
                DragHandle.gameObject.SetActive(false);
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
                currentDragHandleMode.OnModeEnter();
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

    public DragHandleMode CurrentMode
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle is destroyed.")
                : currentDragHandleMode;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle is destroyed.");

            var newDragHandleMode = value;

            if (value is null)
                newDragHandleMode = EmptyDragHandleMode;

            currentDragHandleMode.OnModeExit();

            currentDragHandleMode = newDragHandleMode;

            currentDragHandleMode.OnModeEnter();
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

    private void OnDragging() =>
        CurrentMode.OnDragging();

    private void OnClicked() =>
        CurrentMode.OnClicked();

    private void OnDoubleClicked() =>
        CurrentMode.OnDoubleClicked();

    private void OnReleased() =>
        CurrentMode.OnReleased();
}
