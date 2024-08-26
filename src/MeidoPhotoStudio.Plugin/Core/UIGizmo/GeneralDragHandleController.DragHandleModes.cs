using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

/// <summary>Drag handle modes for general drag handles.</summary>
public abstract partial class GeneralDragHandleController
{
    public abstract class GeneralDragHandleMode<T>(T controller) : DragHandleMode
        where T : GeneralDragHandleController
    {
        protected static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        protected virtual T Controller { get; } = controller ?? throw new ArgumentNullException(nameof(controller));

        protected CustomGizmo Gizmo =>
            Controller.Gizmo;

        protected DragHandle DragHandle =>
            Controller.DragHandle;

        protected Transform Target =>
            Controller.Target;

        protected TransformBackup TransformBackup =>
            Controller.TransformBackup;
    }

    public class NoneMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnModeEnter()
        {
            Controller.DragHandleActive = false;
            DragHandle.MovementType = DragHandle.MoveType.None;
            Controller.GizmoActive = false;
        }
    }

    public class MoveWorldXZMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDoubleClicked() =>
            TransformBackup.ApplyPosition(Target);

        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.XZ;
            DragHandle.Color = MoveColour;

            Controller.GizmoActive = true;

            if (Gizmo)
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
        }
    }

    public class MoveWorldYMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDoubleClicked() =>
            TransformBackup.ApplyPosition(Target);

        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.Y;
            DragHandle.Color = MoveColour;

            Controller.GizmoActive = true;

            if (Gizmo)
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
        }
    }

    public abstract class GeneralDragHandleRotateMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDoubleClicked() =>
            TransformBackup.ApplyRotation(Target);

        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = RotateColour;

            Controller.GizmoActive = true;

            if (Gizmo)
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
        }
    }

    public class RotateLocalXZMode<T>(T controller) : GeneralDragHandleRotateMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDragging()
        {
            var cameraTransform = Camera.transform;
            var forward = cameraTransform.forward;
            var right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            var mouseDelta = MouseDelta;
            var mouseX = mouseDelta.x;
            var mouseY = mouseDelta.y;

            Target.Rotate(forward, -mouseX * 5f, Space.World);
            Target.Rotate(right, mouseY * 5f, Space.World);
        }
    }

    public class RotateWorldYMode<T>(T controller) : GeneralDragHandleRotateMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDragging()
        {
            var mouseX = MouseDelta.x;

            Target.Rotate(Vector3.up, -mouseX * 7, Space.World);
        }
    }

    public class RotateLocalYMode<T>(T controller) : GeneralDragHandleRotateMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDragging()
        {
            var mouseX = MouseDelta.x;

            Target.Rotate(Vector3.up, -mouseX * 5);
        }
    }

    public class ScaleMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnDoubleClicked() =>
            TransformBackup.ApplyScale(Target);

        public override void OnDragging()
        {
            var delta = MouseDelta.y * 0.1f;
            var currentScale = Target.localScale;
            var deltaScale = currentScale.normalized * delta;
            var newScale = currentScale + deltaScale;

            if (newScale.x < 0f || newScale.y < 0f || newScale.z < 0f)
                return;

            Target.localScale = newScale;
        }

        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = ScaleColour;

            Controller.GizmoActive = true;

            if (Gizmo)
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Scale;
        }
    }

    public class SelectMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = SelectColour;
        }
    }

    public class DeleteMode<T>(T controller) : GeneralDragHandleMode<T>(controller)
        where T : GeneralDragHandleController
    {
        public override void OnModeEnter()
        {
            Controller.DragHandleActive = true;
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = DeleteColour;

            Controller.GizmoActive = false;
        }
    }
}
