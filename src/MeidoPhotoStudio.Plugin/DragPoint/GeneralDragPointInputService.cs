using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin;

public class GeneralDragPointInputService : DragPointInputRepository<IModalDragHandle>
{
    public GeneralDragPointInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    protected override DragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.Select].IsPressed())
            return DragHandleMode.Select;
        else if (inputConfiguration[Hotkey.Delete].IsPressed())
            return DragHandleMode.Delete;
        else if (inputConfiguration[Hotkey.MoveWorldXZ].IsPressed())
            return DragHandleMode.MoveWorldXZ;
        else if (inputConfiguration[Hotkey.MoveWorldY].IsPressed())
            return DragHandleMode.MoveWorldY;
        else if (inputConfiguration[Hotkey.RotateWorldY].IsPressed())
            return DragHandleMode.RotateWorldY;
        else if (inputConfiguration[Hotkey.RotateLocalY].IsPressed())
            return DragHandleMode.RotateLocalY;
        else if (inputConfiguration[Hotkey.RotateLocalXZ].IsPressed())
            return DragHandleMode.RotateLocalXZ;
        else if (inputConfiguration[Hotkey.Scale].IsPressed())
            return DragHandleMode.Scale;
        else
            return DragHandleMode.None;
    }
}
