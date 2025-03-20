using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class ThighGizmoController(
    CustomGizmo gizmo,
    CharacterController characterController,
    CharacterUndoRedoController undoRedoController,
    SelectionController<CharacterController> selectionController,
    TabSelectionController tabSelectionController,
    Transform thighBone)
    : CharacterDragHandleController(
        gizmo, characterController, undoRedoController, selectionController, tabSelectionController)
{
    private NoneMode none;
    private RotateMode rotate;

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public DragHandleMode Rotate =>
        rotate ??= new RotateMode(this);

    protected override Transform[] Transforms { get; } = [thighBone];

    private class NoneMode(ThighGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.GizmoActive = false;
    }

    private class RotateMode(ThighGizmoController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter() =>
            controller.GizmoActive = controller.BoneMode;

        public override void OnGizmoClicked()
        {
            base.OnGizmoClicked();

            controller.AnimationController.Playing = false;
        }
    }
}
