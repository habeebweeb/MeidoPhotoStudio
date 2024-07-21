using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ThighGizmoController(CustomGizmo gizmo, CharacterController characterController)
    : CharacterDragHandleController(gizmo, characterController)
{
    private NoneMode none;
    private RotateMode rotate;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    private class NoneMode(ThighGizmoController controller)
        : DragHandleMode
    {
        private readonly ThighGizmoController controller = controller;

        public override void OnModeEnter() =>
            controller.GizmoActive = false;
    }

    private class RotateMode(ThighGizmoController controller)
        : DragHandleMode
    {
        private readonly ThighGizmoController controller = controller;

        public override void OnModeEnter() =>
            controller.GizmoActive = controller.BoneMode;

        public override void OnGizmoClicked() =>
            controller.AnimationController.Playing = false;
    }
}
