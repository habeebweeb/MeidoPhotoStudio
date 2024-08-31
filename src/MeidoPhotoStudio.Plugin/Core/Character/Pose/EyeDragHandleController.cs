using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class EyeDragHandleController(
    DragHandle dragHandle, CharacterController characterController, CharacterUndoRedoController undoRedoController, bool left)
    : CharacterDragHandleController(dragHandle, characterController, undoRedoController)
{
    private readonly bool left = left;

    private Quaternion backupRotation;
    private NoneMode none;
    private RotateEyeMode rotateEye;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode RotateEye =>
        rotateEye ??= new RotateEyeMode(this);

    protected override Transform[] Transforms { get; } = [];

    private void RotateCharacterEye(float x, float y)
    {
        if (left)
            HeadController.RotateLeftEye(x, y);
        else
            HeadController.RotateRightEye(x, y);
    }

    private void ResetEye()
    {
        if (left)
            HeadController.ResetLeftEyeRotation();
        else
            HeadController.ResetRightEyeRotation();
    }

    private void BackupRotation() =>
        backupRotation = left ? HeadController.LeftEyeRotation : HeadController.RightEyeRotation;

    private void ApplyBackupRotation()
    {
        if (left)
            HeadController.LeftEyeRotation = backupRotation;
        else
            HeadController.RightEyeRotation = backupRotation;
    }

    private class NoneMode(EyeDragHandleController controller)
        : DragHandleMode
    {
        private readonly EyeDragHandleController controller = controller;

        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupRotation();
        }
    }

    private class RotateEyeMode(EyeDragHandleController controller)
        : DragHandleMode
    {
        private readonly EyeDragHandleController controller = controller;

        private static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = true;
            controller.DragHandle.Visible = false;
        }

        public override void OnClicked() =>
            controller.BackupRotation();

        public override void OnDoubleClicked()
        {
            controller.BackupRotation();
            controller.ResetEye();
        }

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta * 1.5f;

            controller.RotateCharacterEye(deltaX, deltaY);
        }

        public override void OnCancelled() =>
            controller.ApplyBackupRotation();
    }
}
