using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class DigitDragHandleController : CharacterIKDragHandleController
{
    private readonly int digitIndex;
    private readonly bool isFoot;

    private NoneMode none;
    private RotateMode rotate;
    private DragMode dragMode;
    private GizmoRotateMode gizmoRotate;

    public DigitDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        Transform digit,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController, digit, ikTarget)
    {
        var digitNumberIndex = digit.name.EndsWith("Nub") ? 4 : 2;

        isFoot = digit.name.Contains("Toe");
        digitIndex = digit.name[digit.name.Length - digitNumberIndex] - '0';
        Chain = [digit.parent, digit];
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public override DragHandleMode Drag =>
        dragMode ??= new DragMode(this, Chain);

    public DragHandleMode Drag1 =>
        digitIndex == (isFoot ? 0 : 4) ? Drag : None;

    public DragHandleMode Drag2 =>
        digitIndex == (isFoot ? 1 : 3) ? Drag : None;

    public DragHandleMode Drag3 =>
        digitIndex is 2 ? Drag : None;

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

    private new class DragMode(DigitDragHandleController controller, Transform[] chain) : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        private readonly DigitDragHandleController controller = controller
            ?? throw new ArgumentNullException(nameof(controller));

        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Visible = true;
        }
    }

    private class NoneMode(DigitDragHandleController controller)
        : DragHandleMode
    {
        private readonly DigitDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }

    private class RotateMode(DigitDragHandleController controller, Transform digitBase)
        : DragHandleMode
    {
        private readonly DigitDragHandleController controller = controller;
        private readonly Transform digitBase = digitBase;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = !controller.IKController.LimitDigitRotations;
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

    private class GizmoRotateMode(DigitDragHandleController controller)
        : DragHandleMode
    {
        private readonly DigitDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = true;

            if (controller.IKController.LimitDigitRotations)
                controller.Gizmo.SetVisibleRotateHandles(false, false, true);
            else
                controller.Gizmo.SetVisibleRotateHandles(true, true, true);
        }
    }
}
