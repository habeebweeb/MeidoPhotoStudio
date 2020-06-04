using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragJointFinger : BaseDrag
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly GameObject[] otherIK = new GameObject[3];
        private Transform[] ikChain;
        private Vector3[] jointRotation = new Vector3[2];
        private Vector3 off;
        private Vector3 off2;
        private bool baseFinger;

        public DragJointFinger Initialize(
            Transform[] ikChain, bool baseFinger,
            Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(meido, position, rotation);
            this.ikChain = ikChain;
            this.baseFinger = baseFinger;
            InitializeIK();
            InitializeIK2();
            return this;
        }
        public void InitializeIK()
        {
            IK.Init(ikChain[upperArm], ikChain[foreArm], ikChain[hand], maid.body0);
        }

        private void InitializeIK2()
        {
            for (int i = 0; i < otherIK.Length; i++)
            {
                otherIK[i] = new GameObject();
                otherIK[i].transform.position = this.ikChain[i].position;
                otherIK[i].transform.localRotation = this.ikChain[i].localRotation;
            }
        }

        protected override void GetDragType()
        {
            if (Utility.GetModKey(Utility.ModKey.Shift))
            {
                dragType = DragType.RotLocalY;
            }
            else
            {
                dragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            off = transform.position - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            );
            off2 = new Vector3(
                transform.position.x - ikChain[hand].position.x,
                transform.position.y - ikChain[hand].position.y,
                transform.position.z - ikChain[hand].position.z
            );

            jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
            jointRotation[handRot] = ikChain[hand].localEulerAngles;
            InitializeIK();
        }

        protected override void Drag()
        {
            if (isPlaying) meido.IsStop = true;

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            Vector3 pos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            ) + off - off2;

            if (dragType == DragType.None)
            {
                IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos, Vector3.zero, ikData);

                if (baseFinger)
                {
                    jointRotation[handRot] = ikChain[hand].localEulerAngles;
                    jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
                    ikChain[upperArm].localEulerAngles = jointRotation[upperArm];
                    ikChain[hand].localEulerAngles = jointRotation[handRot];
                }
                else
                {
                    ikChain[hand].localEulerAngles = new Vector3(
                        jointRotation[handRot].x, jointRotation[handRot].y, ikChain[hand].localEulerAngles.z
                    );
                    ikChain[upperArm].localEulerAngles = new Vector3(
                        jointRotation[upperArmRot].x, jointRotation[upperArmRot].y, ikChain[upperArm].localEulerAngles.z
                    );
                }
            }
            else
            {
                if (dragType == DragType.RotLocalY)
                {
                    Vector3 vec31 = Input.mousePosition - mousePos;
                    ikChain[upperArm].localEulerAngles = jointRotation[upperArmRot];
                    ikChain[upperArm].localRotation = Quaternion.Euler(ikChain[upperArm].localEulerAngles)
                        * Quaternion.AngleAxis((-vec31.x / 1.5f), Vector3.right);
                }
            }

        }
    }
}
