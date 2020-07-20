using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static CustomGizmo;
    internal class DragDogu : BaseDrag
    {
        private Vector3 off;
        private Vector3 off2;
        private float doguScale;
        private Quaternion doguRotation;
        public GameObject Dogu { get; private set; }
        public event EventHandler Delete;
        public event EventHandler Rotate;
        public event EventHandler Scale;
        public event EventHandler Select;
        public bool DeleteMe { get; private set; }
        public string Name => Dogu.name;
        public DragPointManager.AttachPointInfo attachPointInfo = DragPointManager.AttachPointInfo.Empty;
        public bool keepDogu = false;
        public float scaleFactor = 1f;

        public DragDogu Initialize(GameObject dogu, bool keepDogu = false)
        {
            return Initialize(dogu, keepDogu, GizmoMode.World,
                () => this.Dogu.transform.position,
                () => Vector3.zero
            );
        }

        public DragDogu Initialize(GameObject dogu, bool keepDogu, GizmoMode mode,
            Func<Vector3> position, Func<Vector3> rotation
        )
        {
            this.keepDogu = keepDogu;
            this.Dogu = dogu;
            base.InitializeDragPoint(position, rotation);
            InitializeGizmo(this.Dogu.transform, 1f, mode);
            gizmo.GizmoDrag += (s, a) =>
            {
                if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
                {
                    OnRotate();
                }
            };
            return this;
        }

        protected override void Update()
        {
            float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
            transform.localScale = Vector3.one * (0.4f * InitialScale.x * DragPointScale * distance);
            base.Update();
        }

        protected override void GetDragType()
        {
            bool holdShift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Input.GetKey(KeyCode.A))
            {
                CurrentDragType = DragType.Select;
                CurrentGizmoType = GizmoType.None;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                CurrentDragType = DragType.Delete;
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
            if (CurrentDragType == DragType.Delete)
            {
                this.DeleteMe = true;
                this.Delete?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (CurrentDragType == DragType.Select)
            {
                this.Select?.Invoke(this, EventArgs.Empty);
                return;
            }

            base.InitializeDrag();

            doguScale = Dogu.transform.localScale.x;
            doguRotation = Dogu.transform.rotation;
            off = transform.position - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            );
            off2 = new Vector3(
                transform.position.x - Dogu.transform.position.x,
                transform.position.y - Dogu.transform.position.y,
                transform.position.z - Dogu.transform.position.z
            );
        }

        protected override void DoubleClick()
        {
            if (CurrentDragType == DragType.Scale)
            {
                Dogu.transform.localScale = new Vector3(1f, 1f, 1f);
                OnScale();
            }
            if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
            {
                Dogu.transform.rotation = Quaternion.identity;
                OnRotate();
            }
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.Select || CurrentDragType == DragType.Delete) return;

            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;

            if (CurrentDragType == DragType.MoveXZ)
            {
                Dogu.transform.position = new Vector3(pos.x, Dogu.transform.position.y, pos.z);
            }

            if (CurrentDragType == DragType.MoveY)
            {
                Dogu.transform.position = new Vector3(Dogu.transform.position.x, pos.y, Dogu.transform.position.z);
            }

            if (CurrentDragType == DragType.RotY)
            {
                Vector3 mouseDelta = Input.mousePosition - mousePos;

                Dogu.transform.rotation = doguRotation;
                Dogu.transform.Rotate(Vector3.up, doguRotation.y - mouseDelta.x / 3f, Space.World);
                OnRotate();
            }

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                Vector3 mouseDelta = Input.mousePosition - mousePos;
                Vector3 cameraDirectionForward = Camera.main.transform.TransformDirection(Vector3.forward);
                Vector3 cameraDirectionRight = Camera.main.transform.TransformDirection(Vector3.right);

                Dogu.transform.rotation = doguRotation;
                Dogu.transform.Rotate(cameraDirectionForward, doguRotation.x - mouseDelta.x / 3f, Space.World);
                Dogu.transform.Rotate(cameraDirectionRight, doguRotation.y + mouseDelta.y / 3f, Space.World);
                OnRotate();
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                Vector3 mouseDelta = Input.mousePosition - mousePos;

                Dogu.transform.rotation = doguRotation;
                Dogu.transform.Rotate(Vector3.up * (-mouseDelta.x / 2.2f));
                OnRotate();
            }

            if (CurrentDragType == DragType.Scale)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                float scale = doguScale + (posOther.y / 200f) * scaleFactor;
                if (scale < 0.1f) scale = 0.1f;
                Dogu.transform.localScale = new Vector3(scale, scale, scale);
                OnScale();
            }
        }

        private void OnRotate()
        {
            Rotate?.Invoke(this, EventArgs.Empty);
        }

        private void OnScale()
        {
            Scale?.Invoke(this, EventArgs.Empty);
        }

        private void OnDestroy()
        {
            if (!keepDogu) GameObject.Destroy(this.Dogu);
        }
    }
}
