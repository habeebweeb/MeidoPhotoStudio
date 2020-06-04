using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragSpine : BaseDrag
    {
        private Transform spine;
        private Vector3 rotate;
        private Vector3 off;
        private Vector3 off2;
        private bool isHip;

        public DragSpine Initialize(
            Transform spine, bool isHip,
            Meido meido, Func<Vector3> position, Func<Vector3> rotation
        )
        {
            base.Initialize(meido, position, rotation);
            this.spine = spine;
            this.isHip = isHip;

            InitializeGizmo(this.spine);
            return this;
        }

        protected override void GetDragType()
        {
            if (isHip && Utility.GetModKey(Utility.ModKey.Control))
            {
                dragType = DragType.MoveY;
                if (GizmoActive) SetGizmo(GizmoType.Rotate);
            }
            else
            {
                dragType = DragType.None;
                if (GizmoActive) SetGizmo(GizmoType.Rotate);
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();
            rotate = spine.localEulerAngles;

            if (isHip)
            {
                off = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z));
                off2 = new Vector3(
                    transform.position.x - spine.position.x,
                    transform.position.y - spine.position.y,
                    transform.position.z - spine.position.z
                );
            }
        }

        protected override void Drag()
        {
            if (isPlaying) meido.IsStop = true;

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

            if (dragType == DragType.MoveY)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;
                spine.position = new Vector3(spine.position.x, pos.y, spine.position.z);
            }
        }
    }
}
