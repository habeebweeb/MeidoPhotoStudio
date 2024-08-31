using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestSubGizmoController(
    CustomGizmo gizmo, CharacterController characterController, CharacterUndoRedoController undoRedoController, Transform bone)
    : CharacterDragHandleController(gizmo, characterController, undoRedoController)
{
    private readonly bool left = bone.name.StartsWith("Mune_L");

    private NoneMode none;
    private RotateMode rotate;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    protected override Transform[] Transforms { get; } = [bone];

    private void SetMuneEnabled(bool enabled)
    {
        if (left)
            IKController.MuneLEnabled = enabled;
        else
            IKController.MuneREnabled = enabled;
    }

    private class NoneMode(ChestSubGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.GizmoActive = false;
    }

    private class RotateMode(ChestSubGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.GizmoActive = true;

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;

            controller.SetMuneEnabled(false);
        }
    }
}
