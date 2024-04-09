using MeidoPhotoStudio.Plugin.Core.Configuration;

namespace MeidoPhotoStudio.Plugin;

public class GeneralDragPointInputService : DragPointInputRepository<IModalDragHandle>
{
    public GeneralDragPointInputService(InputConfiguration inputConfiguration)
        : base(inputConfiguration)
    {
    }

    protected override LegacyDragHandleMode CheckDragType()
    {
        if (inputConfiguration[Hotkey.Select].IsPressed())
            return LegacyDragHandleMode.Select;
        else if (inputConfiguration[Hotkey.Delete].IsPressed())
            return LegacyDragHandleMode.Delete;
        else if (inputConfiguration[Hotkey.MoveWorldXZ].IsPressed())
            return LegacyDragHandleMode.MoveWorldXZ;
        else if (inputConfiguration[Hotkey.MoveWorldY].IsPressed())
            return LegacyDragHandleMode.MoveWorldY;
        else if (inputConfiguration[Hotkey.RotateWorldY].IsPressed())
            return LegacyDragHandleMode.RotateWorldY;
        else if (inputConfiguration[Hotkey.RotateLocalY].IsPressed())
            return LegacyDragHandleMode.RotateLocalY;
        else if (inputConfiguration[Hotkey.RotateLocalXZ].IsPressed())
            return LegacyDragHandleMode.RotateLocalXZ;
        else if (inputConfiguration[Hotkey.Scale].IsPressed())
            return LegacyDragHandleMode.Scale;
        else
            return LegacyDragHandleMode.None;
    }
}
