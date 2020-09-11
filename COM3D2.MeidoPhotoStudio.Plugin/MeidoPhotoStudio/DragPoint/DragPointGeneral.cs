using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;

    internal abstract class DragPointGeneral : DragPoint
    {
        public const float smallCube = 0.5f;
        private float currentScale;
        private bool scaling;
        private Quaternion currentRotation;
        public float ScaleFactor { get; set; } = 1f;
        public bool ConstantScale { get; set; }
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
            bool shift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Input.GetKey(KeyCode.A))
            {
                CurrentDragType = DragType.Select;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                CurrentDragType = DragType.Delete;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                if (Utility.GetModKey(Utility.ModKey.Control)) CurrentDragType = DragType.MoveY;
                else CurrentDragType = shift ? DragType.RotY : DragType.MoveXZ;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(KeyCode.C))
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
            if (CurrentDragType == DragType.Delete)
            {
                OnDelete();
                return;
            }

            if (CurrentDragType == DragType.Select)
            {
                OnSelect();
                return;
            }

            base.OnMouseDown();

            currentScale = this.MyObject.localScale.x;
            currentRotation = this.MyObject.rotation;
        }

        protected override void OnDoubleClick()
        {
            if (CurrentDragType == DragType.Scale)
            {
                this.MyObject.localScale = Vector3.one;
                OnScale();
                OnEndScale();
            }

            if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
            {
                this.MyObject.rotation = Quaternion.identity;
                OnRotate();
            }
        }

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
                MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 6f, Space.World);
                MyObject.Rotate(camera.transform.right, mouseDelta.y / 4f, Space.World);
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
                float scale = currentScale + (mouseDelta.y / 200f) * ScaleFactor;
                if (scale < 0.1f) scale = 0.1f;
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
