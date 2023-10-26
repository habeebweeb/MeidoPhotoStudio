using MeidoPhotoStudio.Plugin;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

public class BackgroundDragHandleController : GeneralDragHandleController
{
    public BackgroundDragHandleController(DragHandle dragHandle, Transform background)
        : base(dragHandle, background)
    {
    }

    protected override void OnDragHandleModeChanged()
    {
        base.OnDragHandleModeChanged();

        if (CurrentDragType is DragHandleMode.Delete or DragHandleMode.Select)
            DragHandle.gameObject.SetActive(false);
    }
}
