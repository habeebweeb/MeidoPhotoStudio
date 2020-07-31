using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointHead : DragPointMeido
    {
        TBody body;
        private Quaternion headRotation;
        private Vector3 eyeRotationL;
        private Vector3 eyeRotationR;
        private Quaternion defEyeRotL;
        private Quaternion defEyeRotR;
        public event EventHandler Select;
        public bool IsIK { get; set; }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            this.body = this.maid.body0;
            this.defEyeRotL = this.maid.body0.quaDefEyeL;
            this.defEyeRotR = this.maid.body0.quaDefEyeR;
        }

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
            bool shift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Utility.GetModKey(Utility.ModKey.Alt) && Utility.GetModKey(Utility.ModKey.Control))
            {
                // eyes
                CurrentDragType = shift ? DragType.MoveY : DragType.MoveXZ;
            }
            else if (Utility.GetModKey(Utility.ModKey.Alt))
            {
                // head
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(KeyCode.A))
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

            if (CurrentDragType == DragType.Select) Select?.Invoke(this, EventArgs.Empty);

            headRotation = MyObject.rotation;

            eyeRotationL = body.quaDefEyeL.eulerAngles;
            eyeRotationR = body.quaDefEyeR.eulerAngles;
        }

        protected override void OnDoubleClick()
        {
            if (CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY)
            {
                body.quaDefEyeL = defEyeRotL;
                body.quaDefEyeR = defEyeRotR;
            }
            else if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
            {
                meido.IsFreeLook = !meido.IsFreeLook;
            }
        }

        protected override void Drag()
        {
            if (IsIK) return;

            if (!(CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY))
            {
                if (isPlaying) meido.IsStop = true;
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

                body.quaDefEyeL.eulerAngles = new Vector3(
                    eyeRotationL.x, eyeRotationL.y - mouseDelta.x / 10f, eyeRotationL.z - mouseDelta.y / 10f
                );
                body.quaDefEyeR.eulerAngles = new Vector3(
                    eyeRotationR.x, eyeRotationR.y + inv * mouseDelta.x / 10f, eyeRotationR.z + mouseDelta.y / 10f
                );
            }
        }
    }
}
