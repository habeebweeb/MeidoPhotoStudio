using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class HipDragHandleController(
    DragHandle dragHandle,
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController,
    Transform spineSegment)
    : CharacterDragHandleController(
        dragHandle, gizmo, characterController, undoRedoController, selectionController, tabSelectionController),
      IColourableDragHandle
{
    private readonly Transform spineSegment = spineSegment ? spineSegment : throw new ArgumentNullException(nameof(spineSegment));
    private readonly float initialGizmoSize = gizmo.offsetScale;

    private Vector3 hipPositionBackup;
    private NoneMode none;
    private RotateMode rotate;
    private MoveMode move;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode Move =>
        move ??= new MoveMode(this);

    public Color DragHandleColour
    {
        get => DragHandle.Color;
        set => DragHandle.Color = value;
    }

    protected override Transform[] Transforms { get; } = [spineSegment];

    private void BackupHipPosition() =>
        hipPositionBackup = spineSegment.localPosition;

    private void ApplyBackupHipPosition() =>
        spineSegment.localPosition = hipPositionBackup;

    private class NoneMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = controller.BoneMode;
            controller.DragHandle.Visible = true;
            controller.DragHandle.MovementType = DragHandle.MoveType.None;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.BackupHipPosition();
            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, deltaY) = MouseDelta;

            var cameraTransform = Camera.transform;
            var cameraForward = cameraTransform.forward;
            var cameraRight = cameraTransform.right;

            controller.spineSegment.Rotate(cameraForward, -deltaX * 5f, Space.World);
            controller.spineSegment.Rotate(cameraRight, deltaY * 5f, Space.World);
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupHipPosition();
        }
    }

    private class MoveMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;

            controller.GizmoActive = controller.BoneMode;
            controller.Gizmo.Mode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
            controller.Gizmo.offsetScale = controller.initialGizmoSize;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.BackupHipPosition();

            controller.AnimationController.Playing = false;
        }

        public override void OnCancelled()
        {
            base.OnCancelled();

            controller.ApplyBackupHipPosition();
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.BackupHipPosition();
            controller.AnimationController.Playing = false;
        }

        public override void OnGizmoCancelled()
        {
            base.OnGizmoCancelled();

            controller.ApplyBackupHipPosition();
        }
    }

    private class RotateMode(HipDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.DragHandle.Visible = false;
            controller.GizmoActive = controller.BoneMode;
            controller.Gizmo.Mode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
            controller.Gizmo.offsetScale = controller.initialGizmoSize - controller.initialGizmoSize * 0.4f;
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.BackupHipPosition();
            controller.AnimationController.Playing = false;
        }

        public override void OnGizmoCancelled()
        {
            base.OnGizmoCancelled();

            controller.ApplyBackupHipPosition();
        }
    }
}
