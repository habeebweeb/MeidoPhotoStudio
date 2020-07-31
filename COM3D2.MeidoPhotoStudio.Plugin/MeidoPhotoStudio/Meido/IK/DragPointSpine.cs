using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointSpine : DragPointMeido
    {
        private Quaternion spineRotation;
        private bool isHip = false;
        private bool isThigh = false;

        public override void Set(Transform spine)
        {
            base.Set(spine);
            isHip = spine.name == "Bip01";
            isThigh = spine.name.EndsWith("Thigh");
        }

        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            if (IsBone && current != DragType.Ignore)
            {
                if (current == DragType.RotLocalXZ) ApplyProperties(false, false, isThigh);
                else if (!isThigh && (current == DragType.MoveY)) ApplyProperties(isHip, isHip, !isHip);
                else if (!isThigh && (current == DragType.RotLocalY)) ApplyProperties(!isHip, !isHip, isHip);
                else ApplyProperties(!isThigh, !isThigh, false);
            }
            else ApplyProperties(false, false, false);
        }

        protected override void UpdateDragType()
        {
            bool shift = Utility.GetModKey(Utility.ModKey.Shift);
            bool alt = Utility.GetModKey(Utility.ModKey.Alt);

            if (Input.GetKey(KeyCode.Space) || OtherDragType())
            {
                CurrentDragType = DragType.Ignore;
            }
            else if (isThigh && alt && shift)
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
            else if (Utility.GetModKey(Utility.ModKey.Control))
            {
                // hip y transform and spine gizmo rotation
                CurrentDragType = DragType.MoveY;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
            spineRotation = MyObject.rotation;
        }

        protected override void Drag()
        {
            if (isPlaying) meido.IsStop = true;

            if (CurrentDragType == DragType.None)
            {
                Vector3 mouseDelta = MouseDelta();

                MyObject.rotation = spineRotation;
                MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 4.5f, Space.World);
                MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
            }

            if (CurrentDragType == DragType.MoveY)
            {
                Vector3 cursorPosition = CursorPosition();
                MyObject.position = new Vector3(MyObject.position.x, cursorPosition.y, MyObject.position.z);
            }
        }
    }
}
