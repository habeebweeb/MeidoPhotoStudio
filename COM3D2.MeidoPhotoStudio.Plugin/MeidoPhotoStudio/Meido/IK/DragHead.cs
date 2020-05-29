using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragHead : BaseDrag
    {
        private Transform head;
        private Vector3 rotate;
        private Vector3 eyeRotL;
        private Vector3 eyeRotR;
        private Vector3 defEyeRotL;
        private Vector3 defEyeRotR;
        private Vector3 mousePosOther;
        public event EventHandler Select;

        public void Initialize(Transform head, Maid maid, Func<Vector3> posFunc, Func<Vector3> rotFunc)
        {
            base.Initialize(maid, posFunc, rotFunc);
            this.head = head;

            // default eye rotations
            defEyeRotL = this.maid.body0.quaDefEyeL.eulerAngles;
            defEyeRotR = this.maid.body0.quaDefEyeR.eulerAngles;

            InitializeGizmo(this.head);
        }

        protected override void GetDragType()
        {
            bool shift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Utility.GetModKey(Utility.ModKey.Alt) && Utility.GetModKey(Utility.ModKey.Control))
            {
                // eyes
                dragType = Utility.GetModKey(Utility.ModKey.Shift) ? DragType.MoveY : DragType.MoveXZ;
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                // head
                dragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                dragType = DragType.Select;
            }
            else
            {
                dragType = DragType.None;
            }
        }
        protected override void DoubleClick()
        {
            if (dragType == DragType.MoveXZ || dragType == DragType.MoveY)
            {
                maid.body0.quaDefEyeL.eulerAngles = defEyeRotL;
                maid.body0.quaDefEyeR.eulerAngles = defEyeRotR;
            }
        }

        protected override void InitializeDrag()
        {
            if (dragType == DragType.Select)
            {
                Select?.Invoke(this, EventArgs.Empty);
                return;
            }

            base.InitializeDrag();

            rotate = head.localEulerAngles;

            eyeRotL = maid.body0.quaDefEyeL.eulerAngles;
            eyeRotR = maid.body0.quaDefEyeR.eulerAngles;
            mousePosOther = Input.mousePosition - mousePos;
        }

        protected override void Drag()
        {
            if (dragType == DragType.None || dragType == DragType.Select) return;

            if (!(dragType == DragType.MoveXZ || dragType == DragType.MoveY))
            {
                if (isPlaying)
                {
                    maid.GetAnimation().Stop();
                    OnDragEvent();
                }
            }

            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z);
            Vector3 vec31 = Input.mousePosition - mousePos;
            Transform t = GameMain.Instance.MainCamera.gameObject.transform;
            Vector3 vec32 = t.TransformDirection(Vector3.right);
            Vector3 vec33 = t.TransformDirection(Vector3.forward);

            if (dragType == DragType.RotLocalXZ)
            {
                head.localEulerAngles = rotate;
                head.RotateAround(head.position, new Vector3(vec32.x, 0.0f, vec32.z), vec31.y / 3f);
                head.RotateAround(head.position, new Vector3(vec33.x, 0.0f, vec33.z), (-vec31.x / 4.5f));
            }

            if (dragType == DragType.RotLocalY)
            {
                head.localEulerAngles = rotate;
                head.localRotation = Quaternion.Euler(head.localEulerAngles) * Quaternion.AngleAxis(vec31.x / 3f, Vector3.right);
            }

            if (dragType == DragType.MoveXZ || dragType == DragType.MoveY)
            {
                int inv = dragType == DragType.MoveY ? -1 : 1;
                Vector3 vec34 = new Vector3(eyeRotR.x, eyeRotR.y + vec31.x / 10f, eyeRotR.z + vec31.y / 10f);

                mousePosOther.y = vec31.y;
                mousePosOther.x = vec31.x;

                maid.body0.quaDefEyeL.eulerAngles = new Vector3(eyeRotL.x, eyeRotL.y - mousePosOther.x / 10f, eyeRotL.z - mousePosOther.y / 10f);
                maid.body0.quaDefEyeR.eulerAngles = new Vector3(eyeRotR.x, eyeRotR.y + inv * mousePosOther.x / 10f, eyeRotR.z + mousePosOther.y / 10f);
            }
        }
    }
}
