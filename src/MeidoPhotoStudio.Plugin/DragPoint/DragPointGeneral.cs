using System;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    using Input = InputManager;

    public abstract class DragPointGeneral : DragPoint
    {
        public const float smallCube = 0.5f;
        private float currentScale;
        private bool scaling;
        private Quaternion currentRotation;
        public Quaternion DefaultRotation { get; set; } = Quaternion.identity;
        public Vector3 DefaultPosition { get; set; } = Vector3.zero;
        public Vector3 DefaultScale { get; set; } = Vector3.one;
        public float ScaleFactor { get; set; } = 1f;
        public bool ConstantScale { get; set; }
        public static readonly Color moveColour = new Color(0.2f, 0.5f, 0.95f, defaultAlpha);
        public static readonly Color rotateColour = new Color(0.2f, 0.75f, 0.3f, defaultAlpha);
        public static readonly Color scaleColour = new Color(0.8f, 0.7f, 0.3f, defaultAlpha);
        public static readonly Color selectColour = new Color(0.9f, 0.5f, 1f, defaultAlpha);
        public static readonly Color deleteColour = new Color(1f, 0.1f, 0.1f, defaultAlpha);
        public event EventHandler Move;
        public event EventHandler Rotate;
        public event EventHandler Scale;
        public event EventHandler EndScale;
        public event EventHandler Delete;
        public event EventHandler Select;

        public override void AddGizmo(float scale = 0.35f, GizmoMode mode = GizmoMode.Local)
        {
            base.AddGizmo(scale, mode);
            Gizmo.GizmoDrag += (s, a) =>
            {
                if (Gizmo.CurrentGizmoType == GizmoType.Rotate) OnRotate();
            };
        }

        protected virtual void ApplyColours()
        {
            Color colour = moveColour;
            if (Rotating) colour = rotateColour;
            else if (Scaling) colour = scaleColour;
            else if (Selecting) colour = selectColour;
            else if (Deleting) colour = deleteColour;
            ApplyColour(colour);
        }

        protected override void Update()
        {
            base.Update();

            if (ConstantScale)
            {
                float distance = Vector3.Distance(camera.transform.position, transform.position);
                transform.localScale = Vector3.one * (0.4f * BaseScale.x * DragPointScale * distance);
            }
        }

        protected override void UpdateDragType()
        {
            bool shift = Input.Shift;
            if (Input.GetKey(MpsKey.DragSelect))
            {
                CurrentDragType = DragType.Select;
            }
            else if (Input.GetKey(MpsKey.DragDelete))
            {
                CurrentDragType = DragType.Delete;
            }
            else if (Input.GetKey(MpsKey.DragMove))
            {
                if (Input.Control) CurrentDragType = DragType.MoveY;
                else CurrentDragType = shift ? DragType.RotY : DragType.MoveXZ;
            }
            else if (Input.GetKey(MpsKey.DragRotate))
            {
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(MpsKey.DragScale))
            {
                CurrentDragType = DragType.Scale;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            if (!ReinitializeDrag)
            {
                if (Deleting)
                {
                    OnDelete();
                    return;
                }

                if (Selecting)
                {
                    OnSelect();
                    return;
                }
            }

            base.OnMouseDown();

            currentScale = MyObject.localScale.x;
            currentRotation = MyObject.rotation;
        }

        protected override void OnDoubleClick()
        {
            if (Scaling)
            {
                MyObject.localScale = DefaultScale;
                OnScale();
                OnEndScale();
            }

            if (Rotating)
            {
                ResetRotation();
                OnRotate();
            }

            if (Moving)
            {
                ResetPosition();
                OnMove();
            }
        }

        protected virtual void ResetPosition() => MyObject.position = DefaultPosition;

        protected virtual void ResetRotation() => MyObject.rotation = DefaultRotation;

        protected override void OnMouseUp()
        {
            base.OnMouseUp();
            if (scaling)
            {
                scaling = false;
                OnScale();
                OnEndScale();
            }
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.Select || CurrentDragType == DragType.Delete) return;

            Vector3 cursorPosition = CursorPosition();
            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.MoveXZ)
            {
                MyObject.position = new Vector3(cursorPosition.x, MyObject.position.y, cursorPosition.z);
                OnMove();
            }

            if (CurrentDragType == DragType.MoveY)
            {
                MyObject.position = new Vector3(
                    MyObject.position.x, cursorPosition.y, MyObject.position.z
                );
                OnMove();
            }

            if (CurrentDragType == DragType.RotY)
            {
                MyObject.rotation = currentRotation;
                MyObject.Rotate(Vector3.up, -mouseDelta.x / 3f, Space.World);
                OnRotate();
            }

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                MyObject.rotation = currentRotation;
                Vector3 forward = camera.transform.forward;
                Vector3 right = camera.transform.right;
                forward.y = 0f;
                right.y = 0f;
                MyObject.Rotate(forward, -mouseDelta.x / 6f, Space.World);
                MyObject.Rotate(right, mouseDelta.y / 4f, Space.World);
                OnRotate();
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                MyObject.rotation = currentRotation;
                MyObject.Rotate(Vector3.up * -mouseDelta.x / 2.2f);
                OnRotate();
            }

            if (CurrentDragType == DragType.Scale)
            {
                scaling = true;
                float scale = currentScale + (mouseDelta.y / 200f * ScaleFactor);
                if (scale < 0f) scale = 0f;
                MyObject.localScale = new Vector3(scale, scale, scale);
                OnScale();
            }
        }

        protected virtual void OnEndScale() => OnEvent(EndScale);
        protected virtual void OnScale() => OnEvent(Scale);
        protected virtual void OnMove() => OnEvent(Move);
        protected virtual void OnRotate() => OnEvent(Rotate);
        protected virtual void OnSelect() => OnEvent(Select);
        protected virtual void OnDelete() => OnEvent(Delete);
        private void OnEvent(EventHandler handler) => handler?.Invoke(this, EventArgs.Empty);
    }
}
