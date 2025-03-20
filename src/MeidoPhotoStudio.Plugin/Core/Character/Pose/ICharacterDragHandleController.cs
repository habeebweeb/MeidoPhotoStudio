using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public interface ICharacterDragHandleController : IDragHandleController
{
    bool DragHandleEnabled { get; set; }

    bool GizmoEnabled { get; set; }

    bool BoneMode { get; set; }

    bool IKEnabled { get; set; }

    bool AutoSelect { get; set; }

    bool AutoSelectTab { get; set; }
}
