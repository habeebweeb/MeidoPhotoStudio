using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public interface ICharacterDragHandleController : IDragHandleController
{
    bool BoneMode { get; set; }

    bool IKEnabled { get; set; }
}
