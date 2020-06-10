using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    public class DragDogu : BaseDrag
    {
        private GameObject dogu;
        private Vector3 off;
        private Vector3 off2;
        private Vector3 mousePos2;
        private float doguScale;
        private Vector3 doguRot;
        public event EventHandler Delete;
        public bool DeleteMe { get; private set; }
        public void Initialize(GameObject dogu)
        {
            this.dogu = dogu;
            base.InitializeDragPoint(() => this.dogu.transform.position, () => Vector3.zero);
            InitializeGizmo(this.dogu.transform, 1f, GizmoMode.World);
        }

        protected override void GetDragType()
        {
            bool holdShift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Input.GetKey(KeyCode.D))
            {
                // Actually delete
                CurrentDragType = DragType.Select;
                CurrentGizmoType = GizmoType.None;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                if (Utility.GetModKey(Utility.ModKey.Control)) CurrentDragType = DragType.MoveY;
                else CurrentDragType = holdShift ? DragType.RotY : DragType.MoveXZ;
                CurrentGizmoType = GizmoType.None;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                CurrentDragType = holdShift ? DragType.RotLocalY : DragType.RotLocalXZ;
                CurrentGizmoType = GizmoType.Rotate;
            }
            else if (Input.GetKey(KeyCode.C))
            {
                CurrentDragType = DragType.Scale;
                CurrentGizmoType = GizmoType.None;
            }
            else
            {
                CurrentDragType = DragType.None;
                CurrentGizmoType = GizmoType.None;
            }
        }

        protected override void InitializeDrag()
        {
            if (CurrentDragType == DragType.Select)
            {
                this.DeleteMe = true;
                this.Delete?.Invoke(this, EventArgs.Empty);
                return;
            }

            base.InitializeDrag();

            doguScale = dogu.transform.localScale.x;
            doguRot = dogu.transform.localEulerAngles;
            off = transform.position - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            );
            off2 = new Vector3(
                transform.position.x - dogu.transform.position.x,
                transform.position.y - dogu.transform.position.y,
                transform.position.z - dogu.transform.position.z
            );
        }

        protected override void DoubleClick()
        {
            if (CurrentDragType == DragType.Scale) dogu.transform.localScale = new Vector3(1f, 1f, 1f);
            if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
                dogu.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
        }

        protected override void Drag()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;

            if (CurrentDragType == DragType.MoveXZ)
            {
                dogu.transform.position = new Vector3(pos.x, dogu.transform.position.y, pos.z);
            }

            if (CurrentDragType == DragType.MoveY)
            {
                dogu.transform.position = new Vector3(dogu.transform.position.x, pos.y, dogu.transform.position.z);
            }

            if (CurrentDragType == DragType.RotY)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                dogu.transform.eulerAngles =
                    new Vector3(dogu.transform.eulerAngles.x, doguRot.y - posOther.x / 3f, dogu.transform.eulerAngles.z);

            }

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                Transform transform = Camera.main.transform;
                Vector3 vector3_3 = transform.TransformDirection(Vector3.right);
                Vector3 vector3_4 = transform.TransformDirection(Vector3.forward);
                transform.TransformDirection(Vector3.forward);
                if (mousePos2 != Input.mousePosition)
                {
                    dogu.transform.localEulerAngles = doguRot;
                    dogu.transform.RotateAround(dogu.transform.position, new Vector3(vector3_3.x, 0.0f, vector3_3.z), posOther.y / 4f);
                    dogu.transform.RotateAround(dogu.transform.position, new Vector3(vector3_4.x, 0.0f, vector3_4.z), (-posOther.x / 6.0f));
                }
                mousePos2 = Input.mousePosition;
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                Transform transform = Camera.main.transform;
                Vector3 vector3_3 = transform.TransformDirection(Vector3.right);
                transform.TransformDirection(Vector3.forward);
                if (mousePos2 != Input.mousePosition)
                {
                    dogu.transform.localEulerAngles = doguRot;
                    dogu.transform.localRotation = Quaternion.Euler(dogu.transform.localEulerAngles)
                        * Quaternion.AngleAxis((-posOther.x / 2.2f), Vector3.up);
                }

                mousePos2 = Input.mousePosition;
            }

            if (CurrentDragType == DragType.Scale)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                float scale = doguScale + posOther.y / 200f;
                if (scale < 0.1f) scale = 0.1f;
                dogu.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void OnDestroy()
        {
            GameObject.Destroy(this.dogu);
        }
    }
}
