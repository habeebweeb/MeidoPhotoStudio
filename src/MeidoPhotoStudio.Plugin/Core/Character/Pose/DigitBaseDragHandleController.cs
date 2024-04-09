using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class DigitBaseDragHandleController : CharacterIKDragHandleController
{
    private readonly int digitIndex;
    private readonly bool isFoot;

    private NoneMode none;
    private RotateMode rotate;
    private DragMode drag;
    private GizmoRotateMode gizmoRotate;

    public DigitBaseDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController, bone, ikTarget)
    {
        var baseBone = Bone.parent;

        isFoot = bone.name.Contains("Toe");
        digitIndex = baseBone.name[baseBone.name.Length - 1] - '0';
        Chain = [baseBone, Bone];
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode Drag1 =>
        digitIndex == (isFoot ? 0 : 4) ? Drag : None;

    public DragHandleMode Drag2 =>
        digitIndex == (isFoot ? 1 : 3) ? Drag : None;

    public DragHandleMode Drag3 =>
        digitIndex is 2 && !isFoot ? Drag : None;

    public DragHandleMode Drag4 =>
        digitIndex is 1 && !isFoot ? Drag : None;

    public DragHandleMode Drag5 =>
        digitIndex is 0 && !isFoot ? Drag : None;

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this, Bone.parent);

    public DragHandleMode Rotate1 =>
        digitIndex == (isFoot ? 0 : 4) ? GizmoRotate : None;

    public DragHandleMode Rotate2 =>
        digitIndex == (isFoot ? 1 : 3) ? GizmoRotate : None;

    public DragHandleMode Rotate3 =>
        digitIndex is 2 ? GizmoRotate : None;

    public DragHandleMode Rotate4 =>
        digitIndex is 1 && !isFoot ? GizmoRotate : None;

    public DragHandleMode Rotate5 =>
        digitIndex is 0 && !isFoot ? GizmoRotate : None;

    protected override Transform[] Chain { get; }

    private DragHandleMode GizmoRotate =>
        gizmoRotate ??= new GizmoRotateMode(this);

    private class NoneMode(DigitBaseDragHandleController controller)
        : DragHandleMode
    {
        private readonly DigitBaseDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }

    private new class DragMode(DigitBaseDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Visible = true;
        }
    }

    private class RotateMode(DigitBaseDragHandleController controller, Transform digitBase)
        : DragHandleMode
    {
        private readonly DigitBaseDragHandleController controller = controller;
        private readonly Transform digitBase = digitBase;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = true;
            controller.GizmoActive = false;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            digitBase.Rotate(Vector3.right, deltaX * 7f);
        }
    }

    private class GizmoRotateMode(DigitBaseDragHandleController controller)
        : DragHandleMode
    {
        private readonly DigitBaseDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = true;
        }
    }
}
