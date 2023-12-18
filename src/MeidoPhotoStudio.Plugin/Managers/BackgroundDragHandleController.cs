using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

public class BackgroundDragHandleController : GeneralDragHandleController
{
    public BackgroundDragHandleController(DragHandle dragHandle, Transform background)
        : base(dragHandle, background)
    {
    }

    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        None;

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        None;
}
