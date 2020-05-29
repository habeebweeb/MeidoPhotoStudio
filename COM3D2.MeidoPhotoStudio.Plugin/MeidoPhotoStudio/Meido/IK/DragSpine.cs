using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragSpine : BaseDrag
    {
        private Transform spine;
        private Vector3 rotate;

        public void Initialize(Transform spine, Maid maid, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(maid, position, rotation);
            this.spine = spine;

            InitializeGizmo(this.spine);
        }

        protected override void GetDragType()
        {
            dragType = DragType.None;
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();
            rotate = spine.localEulerAngles;
        }

        protected override void Drag()
        {
            if (isPlaying)
            {
                maid.GetAnimation().Stop();
                OnDragEvent();
            }

            if (dragType == DragType.None)
            {
                Vector3 vec31 = Input.mousePosition - mousePos;
                Transform t = GameMain.Instance.MainCamera.gameObject.transform;
                Vector3 vec32 = t.TransformDirection(Vector3.right);
                Vector3 vec33 = t.TransformDirection(Vector3.forward);

                spine.localEulerAngles = rotate;
                spine.RotateAround(spine.position, new Vector3(vec32.x, 0.0f, vec32.z), vec31.y / 3f);
                spine.RotateAround(spine.position, new Vector3(vec33.x, 0.0f, vec33.z), (-vec31.x / 4.5f));
            }
        }
    }
}
