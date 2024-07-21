using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class DragHandleControllerBase : IDragHandleController, INotifyPropertyChanged
{
    protected static readonly UnityEngine.Camera Camera = GameMain.Instance.MainCamera.camera;

    private static readonly EmptyDragHandleMode EmptyDragHandleMode = new();

    private DragHandle dragHandle;
    private bool enabled = true;
    private DragHandleMode currentDragHandleMode = EmptyDragHandleMode;
    private CustomGizmo gizmo;

    public DragHandleControllerBase(DragHandle dragHandle) =>
        DragHandle = dragHandle ? dragHandle : throw new ArgumentNullException(nameof(dragHandle));

    public DragHandleControllerBase(CustomGizmo gizmo) =>
        Gizmo = gizmo ? gizmo : throw new ArgumentNullException(nameof(gizmo));

    public DragHandleControllerBase(DragHandle dragHandle, CustomGizmo gizmo)
    {
        DragHandle = dragHandle ? dragHandle : throw new ArgumentNullException(nameof(dragHandle));
        Gizmo = gizmo ? gizmo : throw new ArgumentNullException(nameof(gizmo));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    // TODO: Rename to DragHandleEnabled or something
    public virtual bool Enabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed.")
                : enabled;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            if (!DragHandle)
                return;

            enabled = value;

            if (enabled)
                currentDragHandleMode.OnModeEnter();
            else
                DragHandle.gameObject.SetActive(false);

            RaisePropertyChanged(nameof(Enabled));
        }
    }

    public virtual bool GizmoEnabled
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed")
                : Gizmo && Gizmo.GizmoVisible;
        set
        {
            if (!Gizmo)
                return;

            Gizmo.GizmoVisible = value;

            if (value)
                currentDragHandleMode.OnModeEnter();

            RaisePropertyChanged(nameof(GizmoEnabled));
        }
    }

    public CustomGizmo.GizmoMode GizmoMode
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed")
                : Gizmo
                    ? Gizmo.Mode
                    : CustomGizmo.GizmoMode.Local;
        set
        {
            if (!Gizmo)
                return;

            Gizmo.Mode = value;

            RaisePropertyChanged(nameof(GizmoMode));
        }
    }

    public DragHandleMode CurrentMode
    {
        get =>
            Destroyed
                ? throw new InvalidOperationException("Drag handle controller is destroyed.")
                : currentDragHandleMode;
        set
        {
            if (Destroyed)
                throw new InvalidOperationException("Drag handle controller is destroyed.");

            var newDragHandleMode = value;

            if (value is null)
                newDragHandleMode = EmptyDragHandleMode;

            currentDragHandleMode.OnModeExit();

            currentDragHandleMode = newDragHandleMode;

            currentDragHandleMode.OnModeEnter();

            RaisePropertyChanged(nameof(CurrentMode));
        }
    }

    public bool Destroyed { get; private set; }

    protected DragHandle DragHandle
    {
        get => dragHandle;
        private init
        {
            dragHandle = value;
            dragHandle.Clicked.AddListener(OnClicked);
            dragHandle.Dragging.AddListener(OnDragging);
            dragHandle.Released.AddListener(OnReleased);
            dragHandle.DoubleClicked.AddListener(OnDoubleClicked);
        }
    }

    protected CustomGizmo Gizmo
    {
        get => gizmo;
        private init
        {
            gizmo = value;
            gizmo.Clicked.AddListener(OnGizmoClicked);
            gizmo.Dragging.AddListener(OnGizmoDragging);
            gizmo.Released.AddListener(OnGizmoReleased);
        }
    }

    protected bool DragHandleActive
    {
        get => DragHandle && Enabled && DragHandle.isActiveAndEnabled;
        set
        {
            if (!DragHandle)
                return;

            if (!Enabled)
                return;

            DragHandle.gameObject.SetActive(value);
        }
    }

    protected bool GizmoActive
    {
        get => Gizmo && GizmoEnabled && Gizmo.isActiveAndEnabled;
        set
        {
            if (!Gizmo)
                return;

            if (!GizmoEnabled)
                return;

            Gizmo.gameObject.SetActive(value);
        }
    }

    public void Destroy()
    {
        if (Destroyed)
            return;

        OnDestroying();

        if (DragHandle)
            Object.Destroy(DragHandle.gameObject);

        if (Gizmo)
            Object.Destroy(Gizmo.gameObject);

        Destroyed = true;
    }

    protected virtual void OnDestroying()
    {
    }

    protected virtual void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }

    private void OnDragging() =>
        CurrentMode.OnDragging();

    private void OnClicked() =>
        CurrentMode.OnClicked();

    private void OnDoubleClicked() =>
        CurrentMode.OnDoubleClicked();

    private void OnReleased() =>
        CurrentMode.OnReleased();

    private void OnGizmoDragging() =>
        CurrentMode.OnGizmoDragging();

    private void OnGizmoClicked() =>
        CurrentMode.OnGizmoClicked();

    private void OnGizmoReleased() =>
        CurrentMode.OnGizmoReleased();
}
