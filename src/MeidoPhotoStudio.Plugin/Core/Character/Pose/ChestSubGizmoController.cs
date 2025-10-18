using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

using GizmoSize = (float RotateSize, float MoveSize);

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestSubGizmoController(
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController,
    Transform bone)
    : CharacterDragHandleController(
        gizmo, characterController, undoRedoController, selectionController, tabSelectionController)
{
    private static readonly GizmoSize Sizes = (0.2f, 0.45f);

    private readonly bool left = bone.name.StartsWith("Mune_L");

    private NoneMode none;
    private RotateMode rotate;
    private MoveMode move;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    public DragHandleMode Move =>
        move ??= new MoveMode(this);

    protected override Transform[] Transforms { get; } = [bone];

    private Transform Bone { get; } = bone;

    protected override IEnumerable<BoneBackup> CreateBackup() =>
        base.CreateBackup().Concat([BoneBackup.CreateWithPosition(Bone)]);

    private void SetMuneEnabled(bool enabled)
    {
        if (left)
            IKController.MuneLEnabled = enabled;
        else
            IKController.MuneREnabled = enabled;
    }

    private class NoneMode(ChestSubGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.GizmoActive = false;
    }

    private class RotateMode(ChestSubGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.GizmoActive = true;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
            controller.GizmoMode = CustomGizmo.GizmoMode.Local;
            controller.Gizmo.offsetScale = Sizes.RotateSize;
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;

            controller.SetMuneEnabled(false);
        }
    }

    private class MoveMode(ChestSubGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.GizmoActive = true;
            controller.Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
            controller.GizmoMode = CustomGizmo.GizmoMode.World;
            controller.Gizmo.offsetScale = Sizes.MoveSize;
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;

            controller.SetMuneEnabled(false);
        }
    }
}
