using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public abstract class DragHandleInputHandler<T>(InputConfiguration inputConfiguration)
    : IDragHandleInputHandler<T>, IEnumerable<T>
    where T : IDragHandleController
{
    protected readonly InputConfiguration inputConfiguration = inputConfiguration;

    private readonly List<T> controllers = [];

    public virtual bool Active =>
        controllers.Count > 0;

    public void AddController(T controller)
    {
        if (controller == null)
            throw new ArgumentNullException(nameof(controller));

        if (controllers.Contains(controller))
            return;

        controllers.Add(controller);

        OnControllerAdded(controller);
    }

    public void RemoveController(T controller)
    {
        if (controller == null)
            throw new ArgumentNullException(nameof(controller));

        if (!controllers.Contains(controller))
            return;

        controllers.Remove(controller);

        OnControllerRemoved(controller);
    }

    public abstract void CheckInput();

    public IEnumerator<T> GetEnumerator() =>
        controllers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    protected virtual void OnControllerAdded(T controller)
    {
    }

    protected virtual void OnControllerRemoved(T controller)
    {
    }
}
