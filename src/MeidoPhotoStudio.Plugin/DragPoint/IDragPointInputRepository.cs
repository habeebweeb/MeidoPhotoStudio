using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Service.Input;

namespace MeidoPhotoStudio.Plugin;

public interface IDragPointInputRepository<T> : IInputHandler
    where T : IModalDragHandle
{
    void AddDragHandle(T dragHandle);

    void RemoveDragHandle(T dragHandle);
}
