using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundDragHandleController(DragHandle dragHandle, Transform background)
    : GeneralDragHandleController(dragHandle, background)
{
    public override DragHandleMode Select =>
        None;

    public override DragHandleMode Delete =>
        None;
}
