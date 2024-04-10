using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

public class BackgroundDragHandleController(DragHandle dragHandle, Transform background)
    : GeneralDragHandleController(dragHandle, background)
{
    public override GeneralDragHandleMode<GeneralDragHandleController> Select =>
        None;

    public override GeneralDragHandleMode<GeneralDragHandleController> Delete =>
        None;
}
