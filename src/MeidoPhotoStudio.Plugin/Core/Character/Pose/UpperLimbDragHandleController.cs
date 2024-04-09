using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class UpperLimbDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    Transform bone,
    Transform ikTarget)
    : CharacterIKDragHandleController(dragHandle, gizmo, characterController, bone, ikTarget)
{
    private DragHandleMode drag;
    private RotateMode rotate;

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    protected override Transform[] Chain { get; } = [bone.parent, bone];

    private new class DragMode(UpperLimbDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Scale = controller.BoneMode ? Vector3.one * 0.04f : Vector3.one * 0.1f;
        }
    }

    private class RotateMode(UpperLimbDragHandleController controller)
        : DragHandleMode
    {
        private readonly UpperLimbDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.BoneMode;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.DragHandle.Visible = controller.BoneMode;
            controller.GizmoActive = controller.BoneMode;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDragging()
        {
            var parent = controller.Bone.parent;
            var (deltaX, _) = MouseDelta;

            parent.Rotate(Vector3.right, -deltaX * 7f);
        }
    }
}
