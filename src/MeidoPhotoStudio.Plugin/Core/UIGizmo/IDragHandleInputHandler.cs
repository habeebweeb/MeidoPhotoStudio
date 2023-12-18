using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

public interface IDragHandleInputHandler<T> : IInputHandler
    where T : DragHandleControllerBase
{
    void AddController(T controller);

    void RemoveController(T controller);
}
