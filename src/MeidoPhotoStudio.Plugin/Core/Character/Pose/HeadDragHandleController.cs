using System;

using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HeadDragHandleController(
    DragHandle dragHandle,
    CharacterController characterController,
    Transform neckBone,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController)
    : CharacterDragHandleController(dragHandle, characterController)
{
    private readonly SelectionController<CharacterController> selectionController = selectionController
        ?? throw new ArgumentNullException(nameof(selectionController));

    private readonly TabSelectionController tabSelectionController = tabSelectionController
        ?? throw new ArgumentNullException(nameof(tabSelectionController));

    private readonly Transform neckBone = neckBone;

    private NoneMode none;
    private SelectMode select;
    private RotateAlternateMode rotateAlternate;
    private RotateMode rotate;
    private RotateEyesMode rotateEyes;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Select =>
        select ??= new SelectMode(this);

    public DragHandleMode Rotate =>
        BoneMode ? none : rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        BoneMode ? none : rotateAlternate ??= new RotateAlternateMode(this);

    public DragHandleMode RotateEyes =>
        rotateEyes ??= new RotateEyesMode(this);

    private class NoneMode(HeadDragHandleController controller)
        : DragHandleMode
    {
        private readonly HeadDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
        }
    }

    private class SelectMode(HeadDragHandleController controller)
        : DragHandleMode
    {
        private readonly HeadDragHandleController controller = controller;

        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnClicked()
        {
            controller.selectionController.Select(controller.CharacterController);
            controller.tabSelectionController.SelectTab(Constants.Window.Face);
        }

        public override void OnDoubleClicked() =>
            controller.CharacterController.FocusOnFace();
    }

    private abstract class RotateHeadMode(HeadDragHandleController controller)
        : DragHandleMode
    {
        protected readonly HeadDragHandleController controller = controller;

        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnClicked() =>
            controller.AnimationController.Playing = false;

        public override void OnDoubleClicked() =>
            controller.HeadController.FreeLook = !controller.HeadController.FreeLook;
    }

    private class RotateMode(HeadDragHandleController controller)
        : RotateHeadMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            var cameraTransform = Camera.transform;
            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            controller.neckBone.Rotate(cameraForward, -deltaX * 7f, Space.World);
            controller.neckBone.Rotate(cameraRight, deltaY * 7f, Space.World);
        }
    }

    private class RotateAlternateMode(HeadDragHandleController controller)
        : RotateHeadMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            controller.neckBone.Rotate(Vector3.right, deltaX * 7f);
        }
    }

    private class RotateEyesMode(HeadDragHandleController controller)
        : DragHandleMode
    {
        private readonly HeadDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta * 1.5f;

            controller.HeadController.RotateBothEyes(deltaX, deltaY);
        }

        public override void OnDoubleClicked() =>
            controller.HeadController.ResetBothEyeRotations();
    }
}
