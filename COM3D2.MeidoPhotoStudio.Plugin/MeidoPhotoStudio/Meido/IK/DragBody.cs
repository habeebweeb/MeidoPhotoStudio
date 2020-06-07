using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragBody : BaseDrag
    {
        private Vector3 off;
        private Vector3 off2;
        private Vector3 mousePos2;
        private float maidScale;
        private Vector3 maidRot;
        private bool scaling;
        public event EventHandler Select;
        public event EventHandler Scale;

        protected override void GetDragType()
        {
            bool holdShift = Utility.GetModKey(Utility.ModKey.Shift);
            if (Input.GetKey(KeyCode.A))
            {
                CurrentDragType = DragType.Select;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                if (Utility.GetModKey(Utility.ModKey.Control)) CurrentDragType = DragType.MoveY;
                else CurrentDragType = holdShift ? DragType.RotY : DragType.MoveXZ;
            }
            else if (Input.GetKey(KeyCode.X))
            {
                CurrentDragType = holdShift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else if (Input.GetKey(KeyCode.C))
            {
                CurrentDragType = DragType.Scale;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }
        protected override void InitializeDrag()
        {
            if (CurrentDragType == DragType.Select)
            {
                Select?.Invoke(this, EventArgs.Empty);
                return;
            }

            base.InitializeDrag();

            maidScale = maid.transform.localScale.x;
            maidRot = maid.transform.localEulerAngles;
            off = transform.position - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            );
            off2 = new Vector3(
                transform.position.x - maid.transform.position.x,
                transform.position.y - maid.transform.position.y,
                transform.position.z - maid.transform.position.z
            );
        }

        protected override void DoubleClick()
        {
            if (CurrentDragType == DragType.Scale)
            {
                maid.transform.localScale = new Vector3(1f, 1f, 1f);
                Scale?.Invoke(this, EventArgs.Empty);
            }

            if (CurrentDragType == DragType.RotLocalY || CurrentDragType == DragType.RotLocalXZ)
                maid.transform.eulerAngles = new Vector3(0f, maid.transform.eulerAngles.y, 0f);
        }

        protected override void OnMouseUp()
        {
            base.OnMouseUp();
            if (scaling)
            {
                scaling = false;
                Scale?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void Drag()
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            ) + off - off2;

            if (CurrentDragType == DragType.MoveXZ)
            {
                maid.transform.position = new Vector3(pos.x, maid.transform.position.y, pos.z);
            }

            if (CurrentDragType == DragType.MoveY)
            {
                maid.transform.position = new Vector3(maid.transform.position.x, pos.y, maid.transform.position.z);
            }

            if (CurrentDragType == DragType.RotY)
            {
                Vector3 posOther = Input.mousePosition - mousePos;
                maid.transform.eulerAngles = new Vector3(
                    maid.transform.eulerAngles.x, maidRot.y - posOther.x / 3f, maid.transform.eulerAngles.z
                );

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
                    maid.transform.localEulerAngles = maidRot;
                    maid.transform.RotateAround(
                        maid.transform.position,
                        new Vector3(vector3_3.x, 0.0f, vector3_3.z),
                        posOther.y / 4f
                    );
                    maid.transform.RotateAround(
                        maid.transform.position,
                        new Vector3(vector3_4.x, 0.0f, vector3_4.z),
                        -posOther.x / 6.0f
                    );
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
                    maid.transform.localEulerAngles = maidRot;
                    maid.body0.transform.localRotation = Quaternion.Euler(maid.transform.localEulerAngles)
                        * Quaternion.AngleAxis((-posOther.x / 2.2f), Vector3.up);
                }

                mousePos2 = Input.mousePosition;
            }

            if (CurrentDragType == DragType.Scale)
            {
                scaling = true;
                Vector3 posOther = Input.mousePosition - mousePos;
                float scale = maidScale + posOther.y / 200f;
                if (scale < 0.1f) scale = 0.1f;
                maid.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
