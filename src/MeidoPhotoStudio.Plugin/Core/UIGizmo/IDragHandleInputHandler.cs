using MeidoPhotoStudio.Plugin.Framework.Input;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public interface IDragHandleInputHandler<T> : IInputHandler
    where T : IDragHandleController
{
    void AddController(T controller);

    void RemoveController(T controller);
}
