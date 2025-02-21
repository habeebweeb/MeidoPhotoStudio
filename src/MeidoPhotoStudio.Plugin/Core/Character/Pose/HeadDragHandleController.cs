using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HeadDragHandleController(
    DragHandle dragHandle,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController,
    Transform neckBone)
    : CharacterDragHandleController(dragHandle, characterController, undoRedoController, selectionController)
{
    private readonly TabSelectionController tabSelectionController = tabSelectionController
        ?? throw new ArgumentNullException(nameof(tabSelectionController));

    private readonly Transform neckBone = neckBone ? neckBone : throw new ArgumentNullException(nameof(neckBone));

    private (Quaternion LeftEyeRotation, Quaternion RightEyeRotation) backupRotations;
    private NoneMode none;
    private SelectMode select;
    private RotateAlternateMode rotateAlternate;
    private RotateMode rotate;
    private RotateEyesMode rotateEyes;

    public override bool DragHandleEnabled
    {
        set
        {
        }
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Select =>
        select ??= new SelectMode(this);

    public DragHandleMode Rotate =>
        (!IKEnabled || BoneMode)
            ? none
            : rotate ??= new RotateMode(this);

    public DragHandleMode RotateAlternate =>
        (!IKEnabled || BoneMode)
            ? none
            : rotateAlternate ??= new RotateAlternateMode(this);

    public DragHandleMode RotateEyes =>
        IKEnabled
            ? rotateEyes ??= new RotateEyesMode(this)
            : None;

    protected override Transform[] Transforms { get; } = [neckBone];

    private void BackupEyeRotations() =>
        backupRotations = (HeadController.LeftEyeRotation, HeadController.RightEyeRotation);

    private void ApplyBackupEyeRotations()
    {
        HeadController.LeftEyeRotation = backupRotations.LeftEyeRotation;
        HeadController.RightEyeRotation = backupRotations.RightEyeRotation;
    }

    private class NoneMode(HeadDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupEyeRotations();
        }
    }

    private class SelectMode(HeadDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnClicked()
        {
            base.OnClicked();

            controller.BackupEyeRotations();
            controller.tabSelectionController.SelectTab(MainWindow.Tab.CharacterFace);
        }

        public override void OnDoubleClicked() =>
            controller.CharacterController.FocusOnFace();

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupEyeRotations();
        }
    }

    private abstract class RotateHeadMode(HeadDragHandleController controller)
        : PoseableMode(controller)
    {
        protected readonly HeadDragHandleController controller = controller;

        private bool clicked;

        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnClicked()
        {
            base.OnClicked();

            controller.BackupEyeRotations();
            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            if (!clicked)
            {
                clicked = true;

                OnClicked();
            }

            Drag();
        }

        public override void OnReleased() =>
            clicked = false;

        public override void OnDoubleClicked() =>
            controller.HeadController.FreeLook = !controller.HeadController.FreeLook;

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupEyeRotations();
        }

        protected abstract void Drag();
    }

    private class RotateMode(HeadDragHandleController controller)
        : RotateHeadMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        protected override void Drag()
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
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        protected override void Drag()
        {
            var (deltaX, _) = MouseDelta;

            controller.neckBone.Rotate(Vector3.right, deltaX * 7f);
        }
    }

    private class RotateEyesMode(HeadDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter() =>
            controller.DragHandleActive = true;

        public override void OnClicked()
        {
            base.OnClicked();

            controller.BackupEyeRotations();
        }

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta * 1.5f;

            controller.HeadController.RotateBothEyes(deltaX, deltaY);
        }

        public override void OnDoubleClicked()
        {
            controller.BackupEyeRotations();
            controller.HeadController.ResetBothEyeRotations();
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupEyeRotations();
        }
    }
}
