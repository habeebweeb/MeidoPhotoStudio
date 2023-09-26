using UnityEngine;

using DragType = MeidoPhotoStudio.Plugin.DragPoint.DragType;

namespace MeidoPhotoStudio.Plugin;

public class GeneralDragPointInputService : DragPointInputRepository<DragPointGeneral>
{
    static GeneralDragPointInputService()
    {
        InputManager.Register(MpsKey.DragSelect, KeyCode.A, "Select handle mode");
        InputManager.Register(MpsKey.DragDelete, KeyCode.D, "Delete handle mode");
        InputManager.Register(MpsKey.DragMove, KeyCode.Z, "Move handle mode");
        InputManager.Register(MpsKey.DragRotate, KeyCode.X, "Rotate handle mode");
        InputManager.Register(MpsKey.DragScale, KeyCode.C, "Scale handle mode");
    }

    protected override DragType CheckDragType()
    {
        var shift = InputManager.Shift;

        if (InputManager.GetKey(MpsKey.DragSelect))
        {
            return DragType.Select;
        }
        else if (InputManager.GetKey(MpsKey.DragDelete))
        {
            return DragType.Delete;
        }
        else if (InputManager.GetKey(MpsKey.DragMove))
        {
            if (InputManager.Control)
                return DragType.MoveY;
            else
                return shift ? DragType.RotY : DragType.MoveXZ;
        }
        else if (InputManager.GetKey(MpsKey.DragRotate))
        {
            return shift ? DragType.RotLocalY : DragType.RotLocalXZ;
        }
        else if (InputManager.GetKey(MpsKey.DragScale))
        {
            return DragType.Scale;
        }
        else
        {
            return DragType.None;
        }
    }
}
