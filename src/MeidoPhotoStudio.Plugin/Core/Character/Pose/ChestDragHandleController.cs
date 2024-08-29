using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ChestDragHandleController : CharacterIKDragHandleController
{
    private readonly bool left;

    private NoneMode none;
    private DragMode drag;
    private GizmoRotateMode rotateGizmo;

    public ChestDragHandleController(
        DragHandle dragHandle,
        CustomGizmo gizmo,
        CharacterController characterController,
        CharacterUndoRedoController undoRedoController,
        Transform bone,
        Transform ikTarget)
        : base(dragHandle, gizmo, characterController, undoRedoController, bone, ikTarget)
    {
        left = bone.name.StartsWith("Mune_L");

        Chain = [Bone.parent, Bone];
    }

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode RotateGizmo =>
        rotateGizmo ??= new GizmoRotateMode(this);

    protected override Transform[] Chain { get; }

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
        }

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;

            controller.SetMuneEnabled(false);
        }
    }
}
