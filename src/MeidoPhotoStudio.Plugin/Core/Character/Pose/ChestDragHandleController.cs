using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

using GizmoSize = (float RotateSize, float MoveSize);

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestDragHandleController : CharacterIKDragHandleController
{
    private static readonly GizmoSize Sizes = (0.25f, 0.5f);

    private readonly bool left;

    private NoneMode none;
    private DragMode drag;
    private GizmoRotateMode rotateGizmo;
    private MoveMode move;

    public ChestDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController undoRedoController,
        SelectionController<CharacterController> selectionController,
        TabSelectionController tabSelectionController,
        Transform bone,
        Transform ikTarget)
        : base(
            dragHandle,
            gizmo,
            characterController,
            undoRedoController,
            selectionController,
            tabSelectionController,
            bone,
            ikTarget)
    {
        left = Bone.name.StartsWith("Mune_L");

        Chain = [Bone.parent, Bone];
    }

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode RotateGizmo =>
        rotateGizmo ??= new GizmoRotateMode(this);

    public DragHandleMode Move =>
        move ??= new MoveMode(this);

    protected override Transform[] Chain { get; }

    protected override IEnumerable<BoneBackup> CreateBackup() =>
        base.CreateBackup().Concat([BoneBackup.CreateWithPosition(Bone.parent)]);

    private void SetMuneEnabled(bool enabled)
    {
        if (left)
            IKController.MuneLEnabled = enabled;
        else
            IKController.MuneREnabled = enabled;
    }

    private class NoneMode(ChestDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }

    private new class DragMode(ChestDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        protected override bool FixLocalPositions { get; } = false;

        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Visible = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.SetMuneEnabled(false);
        }

        public override void OnDoubleClicked()
        {
            base.OnDoubleClicked();

            controller.SetMuneEnabled(true);
        }
    }

    private class GizmoRotateMode(ChestDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
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

    private class MoveMode(ChestDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
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
