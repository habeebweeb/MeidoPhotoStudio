using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragPelvis : BaseDrag
    {
        private Transform pelvis;
        private Vector3 pelvisRotation;

        public DragPelvis Initialize(Transform pelvis, Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(meido, position, rotation);
            this.pelvis = pelvis;
            return this;
        }

        protected override void GetDragType()
        {
            bool shift = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                dragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else
            {
                dragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();
            pelvisRotation = pelvis.localEulerAngles;
        }

        protected override void Drag()
        {
            if (dragType == DragType.None) return;

            if (isPlaying) meido.IsStop = true;

            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z);
            Vector3 vec31 = Input.mousePosition - mousePos;
            Transform t = GameMain.Instance.MainCamera.gameObject.transform;
            Vector3 vec32 = t.TransformDirection(Vector3.right);
            Vector3 vec33 = t.TransformDirection(Vector3.forward);

            if (dragType == DragType.RotLocalXZ)
            {
                pelvis.localEulerAngles = pelvisRotation;
                pelvis.RotateAround(pelvis.position, new Vector3(vec32.x, 0.0f, vec32.z), vec31.y / 4f);
                pelvis.RotateAround(pelvis.position, new Vector3(vec33.x, 0.0f, vec33.z), vec31.x / 6f);
            }

            if (dragType == DragType.RotLocalY)
            {
                pelvis.localEulerAngles = pelvisRotation;
                pelvis.localRotation = Quaternion.Euler(pelvis.localEulerAngles)
                    * Quaternion.AngleAxis(vec31.x / 3f, Vector3.right);
            }
        }
    }
}
