using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class TorsoDragHandleController : CharacterDragHandleController
{
    private static readonly float[] XZRotationSensitivity = [0.03f, 0.1f, 0.09f, 0.07f];
    private static readonly float[] YRotationSensitivity = [0.08f, 0.08f, 0.15f, 0.15f];

    private readonly Transform[] spineColumn = new Transform[4];

    private NoneMode none;
    private RotateMode rotate;
    private RotateAlternateMode rotateAlternate;

    public TorsoDragHandleController(DragHandle dragHandle, CharacterController characterController)
        : base(dragHandle, characterController)
    {
        var baseSpine = IKController.GetBone("Bip01 Spine1a");
        var spineSegment = baseSpine;

        for (var i = 0; i < 4; i++)
        {
            spineColumn[i] = spineSegment;

            spineSegment = spineSegment.parent;
        }
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        BoneMode ? None : rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        BoneMode ? None : rotateAlternate ??= new RotateAlternateMode(this);

    private class NoneMode(TorsoDragHandleController controller)
        : DragHandleMode
    {
        private readonly TorsoDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
        }
    }

    private class RotateMode(TorsoDragHandleController controller)
        : DragHandleMode
    {
        private readonly TorsoDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            if (controller.BoneMode)
            {
                controller.CurrentMode = controller.None;

                return;
            }

            controller.
            DragHandleActive = true;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            var cameraTransform = Camera.transform;
            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            foreach (var (spineSegment, sensitivity) in controller.spineColumn.Zip(XZRotationSensitivity))
            {
                spineSegment.Rotate(cameraForward, -deltaX * 13f * sensitivity, Space.World);
                spineSegment.Rotate(cameraRight, deltaY * 20f * sensitivity, Space.World);
            }
        }
    }

    private class RotateAlternateMode(TorsoDragHandleController controller)
        : DragHandleMode
    {
        private readonly TorsoDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            if (controller.BoneMode)
            {
                controller.CurrentMode = controller.None;

                return;
            }

            controller.DragHandleActive = true;
        }

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            foreach (var (spineSegment, sensitivity) in controller.spineColumn.Zip(YRotationSensitivity))
                spineSegment.Rotate(Vector3.right, deltaX * 13f * sensitivity);
        }
    }
}
