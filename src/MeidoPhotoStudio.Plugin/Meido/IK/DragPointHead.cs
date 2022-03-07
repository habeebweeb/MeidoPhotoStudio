using System;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointHead : DragPointMeido
    {
        private Quaternion headRotation;
        private Vector3 eyeRotationL;
        private Vector3 eyeRotationR;
        public event EventHandler Select;
        public bool IsIK { get; set; }

        protected override void ApplyDragType()
        {
            if (IsBone)
            {
                DragType current = CurrentDragType;
                bool active = current == DragType.MoveY || current == DragType.MoveXZ || current == DragType.Select;
                ApplyProperties(active, false, false);
            }
            else ApplyProperties(CurrentDragType != DragType.None, false, false);
        }

        protected override void UpdateDragType()
        {
            bool shift = Input.Shift;
            bool alt = Input.Alt;
            if (alt && Input.Control)
            {
                // eyes
                CurrentDragType = shift ? DragType.MoveY : DragType.MoveXZ;
            }
            else if (alt)
            {
                // head
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(MpsKey.DragSelect))
            {
                CurrentDragType = DragType.Select;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();

            if (Selecting && !ReinitializeDrag)
                Select?.Invoke(this, EventArgs.Empty);

            headRotation = MyObject.rotation;

            eyeRotationL = meido.Body.quaDefEyeL.eulerAngles;
            eyeRotationR = meido.Body.quaDefEyeR.eulerAngles;
        }

        protected override void OnDoubleClick()
        {
            if (CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY)
            {
                meido.Body.quaDefEyeL = meido.DefaultEyeRotL;
                meido.Body.quaDefEyeR = meido.DefaultEyeRotR;
            }
            else if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
            {
                meido.FreeLook = !meido.FreeLook;
            }
        }

        protected override void Drag()
        {
            if (IsIK || CurrentDragType == DragType.Select) return;

            if (!(CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY) && isPlaying)
            {
                meido.Stop = true;
            }

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                MyObject.rotation = headRotation;
                MyObject.Rotate(camera.transform.forward, -mouseDelta.x / 3f, Space.World);
                MyObject.Rotate(camera.transform.right, mouseDelta.y / 3f, Space.World);
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                MyObject.rotation = headRotation;
                MyObject.Rotate(Vector3.right * mouseDelta.x / 3f);
            }

            if (CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY)
            {
                int inv = CurrentDragType == DragType.MoveY ? -1 : 1;

                meido.Body.quaDefEyeL.eulerAngles = new Vector3(
                    eyeRotationL.x, eyeRotationL.y - (mouseDelta.x / 10f), eyeRotationL.z - (mouseDelta.y / 10f)
                );
                meido.Body.quaDefEyeR.eulerAngles = new Vector3(
                    eyeRotationR.x, eyeRotationR.y + (inv * mouseDelta.x / 10f), eyeRotationR.z + (mouseDelta.y / 10f)
                );
            }
        }
    }
}
