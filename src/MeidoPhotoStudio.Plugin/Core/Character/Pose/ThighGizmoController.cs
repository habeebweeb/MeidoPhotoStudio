using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ThighGizmoController : CharacterDragHandleController
{
    private NoneMode none;
    private RotateMode rotate;

    public ThighGizmoController(CustomGizmo gizmo, CharacterController characterController)
        : base(gizmo, characterController) =>
        Gizmo.GizmoDrag += OnGizmoDragging;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    private void OnGizmoDragging(object sender, EventArgs e) =>
        AnimationController.Playing = false;

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
    }
}
