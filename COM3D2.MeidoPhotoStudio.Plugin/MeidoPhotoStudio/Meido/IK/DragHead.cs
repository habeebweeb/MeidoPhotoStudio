using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragHead : BaseDrag
    {
        private Transform head;
        private Vector3 rotate;
        private Vector3 eyeRotL;
        private Vector3 eyeRotR;
        private Vector3 defEyeRotL;
        private Vector3 defEyeRotR;
        private Vector3 mousePosOther;
        public event EventHandler Select;
        public bool IsIK { get; set; }

        public DragHead Initialize(Transform head, Meido meido, Func<Vector3> posFunc, Func<Vector3> rotFunc)
        {
            base.Initialize(meido, posFunc, rotFunc);
            this.head = head;

            // default eye rotations
            defEyeRotL = this.maid.body0.quaDefEyeL.eulerAngles;
            defEyeRotR = this.maid.body0.quaDefEyeR.eulerAngles;

            InitializeGizmo(this.head);
            return this;
        }

        protected override void GetDragType()
        {
            bool shift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Utility.GetModKey(Utility.ModKey.Alt) && Utility.GetModKey(Utility.ModKey.Control))
            {
                // eyes
                CurrentDragType = Utility.GetModKey(Utility.ModKey.Shift) ? DragType.MoveY : DragType.MoveXZ;
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
        protected override void DoubleClick()
        {
            if (CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY)
            {
                maid.body0.quaDefEyeL.eulerAngles = defEyeRotL;
                maid.body0.quaDefEyeR.eulerAngles = defEyeRotR;
            }
            if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
            {
                meido.IsFreeLook = !meido.IsFreeLook;
            }
        }

        protected override void InitializeDrag()
        {
            if (CurrentDragType == DragType.Select)
            {
                Select?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (IsBone) return;

            base.InitializeDrag();

            rotate = head.localEulerAngles;

            eyeRotL = maid.body0.quaDefEyeL.eulerAngles;
            eyeRotR = maid.body0.quaDefEyeR.eulerAngles;
            mousePosOther = Input.mousePosition - mousePos;
        }

        protected override void Drag()
        {
            if (!IsIK || (CurrentDragType == DragType.None || CurrentDragType == DragType.Select) || IsBone) return;

            if (!(CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY))
            {
                if (isPlaying) meido.IsStop = true;
            }

            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z);
            Vector3 vec31 = Input.mousePosition - mousePos;
            Transform t = GameMain.Instance.MainCamera.gameObject.transform;
            Vector3 vec32 = t.TransformDirection(Vector3.right);
            Vector3 vec33 = t.TransformDirection(Vector3.forward);

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                head.localEulerAngles = rotate;
                head.RotateAround(head.position, new Vector3(vec32.x, 0.0f, vec32.z), vec31.y / 3f);
                head.RotateAround(head.position, new Vector3(vec33.x, 0.0f, vec33.z), (-vec31.x / 4.5f));
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                head.localEulerAngles = rotate;
                head.localRotation = Quaternion.Euler(head.localEulerAngles)
                    * Quaternion.AngleAxis(vec31.x / 3f, Vector3.right);
            }

            if (CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.MoveY)
            {
                int inv = CurrentDragType == DragType.MoveY ? -1 : 1;
                Vector3 vec34 = new Vector3(eyeRotR.x, eyeRotR.y + vec31.x / 10f, eyeRotR.z + vec31.y / 10f);

                mousePosOther.y = vec31.y;
                mousePosOther.x = vec31.x;

                maid.body0.quaDefEyeL.eulerAngles = new Vector3(
                    eyeRotL.x, eyeRotL.y - mousePosOther.x / 10f, eyeRotL.z - mousePosOther.y / 10f
                );
                maid.body0.quaDefEyeR.eulerAngles = new Vector3(
                    eyeRotR.x, eyeRotR.y + inv * mousePosOther.x / 10f, eyeRotR.z + mousePosOther.y / 10f
                );
            }
        }
    }
}
