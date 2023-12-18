using System;

using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UIGizmo;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.UIGizmo;

/// <summary>Drag handle modes for general drag handles.</summary>
public abstract partial class GeneralDragHandleController
{
    public abstract class GeneralDragHandleMode<T> : DragHandleMode
        where T : GeneralDragHandleController
    {
        public GeneralDragHandleMode(T controller) =>
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));

        protected static Vector2 MouseDelta =>
            new(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y"));

        protected virtual T Controller { get; }

        protected bool Enabled =>
            Controller.Enabled;

        protected bool GizmoEnabled =>
            Controller.GizmoEnabled;

        protected CustomGizmo Gizmo =>
            Controller.Gizmo;

        protected DragHandle DragHandle =>
            Controller.DragHandle;

        protected Transform Target =>
            Controller.Target;

        protected TransformBackup TransformBackup =>
            Controller.TransformBackup;
    }

    public class NoneMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public NoneMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(false);
            DragHandle.MovementType = DragHandle.MoveType.None;

            if (Gizmo)
                Gizmo.gameObject.SetActive(false);
        }
    }

    public class MoveWorldXZMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public MoveWorldXZMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnDoubleClicked() =>
            TransformBackup.ApplyPosition(Target);

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.XZ;
            DragHandle.Color = MoveColour;

            if (Gizmo && GizmoEnabled)
            {
                Gizmo.gameObject.SetActive(GizmoEnabled);
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
            }
        }
    }

    public class MoveWorldYMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public MoveWorldYMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnDoubleClicked() =>
            TransformBackup.ApplyPosition(Target);

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.Y;
            DragHandle.Color = MoveColour;

            if (Gizmo)
            {
                Gizmo.gameObject.SetActive(GizmoEnabled);
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Move;
            }
        }
    }

    public abstract class GeneralDragHandleRotateMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        protected GeneralDragHandleRotateMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnDoubleClicked() =>
            TransformBackup.ApplyRotation(Target);

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = RotateColour;

            if (Gizmo)
            {
                Gizmo.gameObject.SetActive(GizmoEnabled);
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Rotate;
            }
        }
    }

    public class RotateLocalXZMode : GeneralDragHandleRotateMode
    {
        public RotateLocalXZMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

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

    public class RotateWorldYMode : GeneralDragHandleRotateMode
    {
        public RotateWorldYMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnDragging()
        {
            var mouseX = MouseDelta.x;

            Target.Rotate(Vector3.up, -mouseX * 7, Space.World);
        }
    }

    public class RotateLocalYMode : GeneralDragHandleRotateMode
    {
        public RotateLocalYMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnDragging()
        {
            var mouseX = MouseDelta.x;

            Target.Rotate(Vector3.up, -mouseX * 5);
        }
    }

    public class ScaleMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public ScaleMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

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
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = ScaleColour;

            if (Gizmo)
            {
                Gizmo.gameObject.SetActive(GizmoEnabled);
                Gizmo.CurrentGizmoType = CustomGizmo.GizmoType.Scale;
            }
        }
    }

    public class SelectMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public SelectMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = SelectColour;
        }
    }

    public class DeleteMode : GeneralDragHandleMode<GeneralDragHandleController>
    {
        public DeleteMode(GeneralDragHandleController controller)
            : base(controller)
        {
        }

        public override void OnModeEnter()
        {
            DragHandle.gameObject.SetActive(Enabled);
            DragHandle.MovementType = DragHandle.MoveType.None;
            DragHandle.Color = DeleteColour;

            if (Gizmo)
                Gizmo.gameObject.SetActive(false);
        }
    }
}
