using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointSpine : DragPointMeido
    {
        private Quaternion spineRotation;
        private bool isHip;
        private bool isThigh;
        private bool isHead;

        public override void AddGizmo(float scale = 0.25f, CustomGizmo.GizmoMode mode = CustomGizmo.GizmoMode.Local)
        {
            base.AddGizmo(scale, mode);
            if (isHead) Gizmo.GizmoDrag += (s, a) => meido.HeadToCam = false;
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            isHip = myObject.name == "Bip01";
            isThigh = myObject.name.EndsWith("Thigh");
            isHead = myObject.name.EndsWith("Head");
        }

        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            if (IsBone && current != DragType.Ignore)
            {
                if (!isHead && current == DragType.RotLocalXZ) ApplyProperties(false, false, isThigh);
                else if (!isThigh && (current == DragType.MoveY)) ApplyProperties(isHip, isHip, !isHip);
                else if (!(isThigh || isHead) && (current == DragType.RotLocalY)) ApplyProperties(!isHip, !isHip, isHip);
                else ApplyProperties(!isThigh, !isThigh, false);
            }
            else ApplyProperties(false, false, false);
        }

        protected override void UpdateDragType()
        {
            bool shift = Input.Shift;
            bool alt = Input.Alt;

            if (isThigh && alt && shift)
            {
                // gizmo thigh rotation
                CurrentDragType = DragType.RotLocalXZ;
            }
            else if (alt)
            {
                CurrentDragType = DragType.Ignore;
            }
            else if (shift)
            {
                CurrentDragType = DragType.RotLocalY;
            }
            else if (Input.Control)
            {
                // hip y transform and spine gizmo rotation
                CurrentDragType = DragType.MoveY;
            }
            else
            {
                CurrentDragType = OtherDragType() ? DragType.Ignore : DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
            spineRotation = MyObject.rotation;
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.None)
            {
                if (isHead) meido.HeadToCam = false;

                MyObject.rotation = spineRotation;
                MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 4.5f, Space.World);
                MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                if (isHead) meido.HeadToCam = false;

                MyObject.rotation = spineRotation;
                MyObject.Rotate(Vector3.right * mouseDelta.x / 4f);
            }

            if (CurrentDragType == DragType.MoveY)
            {
                Vector3 cursorPosition = CursorPosition();
                MyObject.position = new Vector3(MyObject.position.x, cursorPosition.y, MyObject.position.z);
            }
        }
    }
}
