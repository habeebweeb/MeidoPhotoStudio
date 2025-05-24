using MeidoPhotoStudio.Plugin.Core.UI.Legacy;
using MeidoPhotoStudio.Plugin.Core.UIGizmo;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public class DigitBaseDragHandleController : CharacterIKDragHandleController, IColourableDragHandle
{
    private readonly int digitIndex;
    private readonly bool isFoot;

    private NoneMode none;
    private RotateMode rotate;
    private DragMode drag;
    private GizmoRotateMode gizmoRotate;

    public DigitBaseDragHandleController(
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
        var baseBone = Bone.parent;

        isFoot = bone.name.Contains("Toe");
        digitIndex = baseBone.name[baseBone.name.Length - 1] - '0';
        Chain = [baseBone, Bone];
    }

    public Color DragHandleColour
    {
        get => DragHandle.Color;
        set => DragHandle.Color = value;
    }

    public DragHandleMode None =>
        none ??= new NoneMode(this);

    public override DragHandleMode Drag =>
        drag ??= new DragMode(this, Chain);

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

    public DragHandleMode Gizmo1 =>
        digitIndex == (isFoot ? 0 : 4) ? GizmoRotate : None;

    public DragHandleMode Gizmo2 =>
        digitIndex == (isFoot ? 1 : 3) ? GizmoRotate : None;

    public DragHandleMode Gizmo3 =>
        digitIndex is 2 ? GizmoRotate : None;

    public DragHandleMode Gizmo4 =>
        digitIndex is 1 && !isFoot ? GizmoRotate : None;

    public DragHandleMode Gizmo5 =>
        digitIndex is 0 && !isFoot ? GizmoRotate : None;

    public DragHandleMode Twist =>
        rotate ??= new RotateMode(this, Bone.parent);

    public DragHandleMode Twist1 =>
        digitIndex == (isFoot ? 0 : 4) ? Twist : None;

    public DragHandleMode Twist2 =>
        digitIndex == (isFoot ? 1 : 3) ? Twist : None;

    public DragHandleMode Twist3 =>
        digitIndex is 2 ? Twist : None;

    public DragHandleMode Twist4 =>
        digitIndex is 1 && !isFoot ? Twist : None;

    public DragHandleMode Twist5 =>
        digitIndex is 0 && !isFoot ? Twist : None;

    protected override Transform[] Chain { get; }

    private DragHandleMode GizmoRotate =>
        gizmoRotate ??= new GizmoRotateMode(this);

    private class NoneMode(DigitBaseDragHandleController controller)
        : PoseableMode(controller)
    {
        public override void OnModeEnter()
        {
            controller.DragHandleActive = false;
            controller.GizmoActive = false;
        }
    }

    private new class DragMode(DigitBaseDragHandleController controller, Transform[] chain)
        : CharacterIKDragHandleController.DragMode(controller, chain)
    {
        public override void OnModeEnter()
        {
            base.OnModeEnter();

            controller.DragHandle.Visible = true;
        }
    }

    private class RotateMode(DigitBaseDragHandleController controller, Transform digitBase)
        : PoseableMode(controller)
    {
        private readonly Transform digitBase = digitBase;

        private static Vector2 MouseDelta =>
            new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public override void OnModeEnter()
        {
            controller.DragHandleActive = true;
            controller.GizmoActive = false;
        }

        public override void OnClicked()
        {
            base.OnClicked();

            controller.AnimationController.Playing = false;
        }

        public override void OnDragging()
        {
            var (deltaX, _) = MouseDelta;

            digitBase.Rotate(Vector3.right, deltaX * 7f);
        }
    }

    private class GizmoRotateMode(DigitBaseDragHandleController controller)
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
        }
    }
}
