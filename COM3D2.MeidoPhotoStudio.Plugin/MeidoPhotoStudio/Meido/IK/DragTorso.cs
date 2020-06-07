using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragTorso : BaseDrag
    {
        private Transform[] spine;
        private Vector3[] spineRotation = new Vector3[4];

        public DragTorso Initialize(Transform[] spine, Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(meido, position, rotation);
            this.spine = spine;
            return this;
        }

        protected override void GetDragType()
        {
            bool shift = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKey(KeyCode.LeftAlt))
            {
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

            for (int i = 0; i < spine.Length; i++)
            {
                spineRotation[i] = spine[i].localEulerAngles;
            }
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.None) return;

            if (isPlaying) meido.IsStop = true;

            Vector3 pos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z);
            Vector3 vec31 = Input.mousePosition - mousePos;
            Transform t = GameMain.Instance.MainCamera.gameObject.transform;
            Vector3 vec32 = t.TransformDirection(Vector3.right);
            Vector3 vec33 = t.TransformDirection(Vector3.forward);

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                for (int i = 0; i < 4; i++)
                {
                    spine[i].localEulerAngles = spineRotation[i];
                }

                float num1 = 1.5f;
                float num2 = 1f;
                float num3 = 0.03f;
                float num4 = 0.1f;
                float num5 = 0.09f;
                float num6 = 0.07f;
                spine[0].RotateAround(spine[0].position, new Vector3(vec32.x, 0f, vec32.z), vec31.y / num2 * num3);
                spine[0].RotateAround(spine[0].position, new Vector3(vec33.x, 0f, vec33.z), -vec31.x / num1 * num3);
                spine[1].RotateAround(spine[1].position, new Vector3(vec32.x, 0f, vec32.z), vec31.y / num2 * num4);
                spine[1].RotateAround(spine[1].position, new Vector3(vec33.x, 0f, vec33.z), -vec31.x / num1 * num4);
                spine[2].RotateAround(spine[2].position, new Vector3(vec32.x, 0f, vec32.z), vec31.y / num2 * num5);
                spine[2].RotateAround(spine[2].position, new Vector3(vec33.x, 0f, vec33.z), -vec31.x / num1 * num5);
                spine[3].RotateAround(spine[3].position, new Vector3(vec32.x, 0f, vec32.z), vec31.y / num2 * num6);
                spine[3].RotateAround(spine[3].position, new Vector3(vec33.x, 0f, vec33.z), -vec31.x / num1 * num6);
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                for (int i = 0; i < 4; i++)
                {
                    spine[i].localEulerAngles = spineRotation[i];
                }
                spine[0].localRotation = Quaternion.Euler(spine[0].localEulerAngles)
                    * Quaternion.AngleAxis(vec31.x / 1.5f * 0.08f, Vector3.right);
                spine[2].localRotation = Quaternion.Euler(spine[2].localEulerAngles)
                    * Quaternion.AngleAxis(vec31.x / 1.5f * 0.15f, Vector3.right);
                spine[3].localRotation = Quaternion.Euler(spine[3].localEulerAngles)
                    * Quaternion.AngleAxis(vec31.x / 1.5f * 0.15f, Vector3.right);
            }
        }
    }
}
