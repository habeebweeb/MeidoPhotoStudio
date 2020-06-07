using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragJointHand : BaseDrag
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly GameObject[] otherIK = new GameObject[3];
        private Transform[] ikChain;
        private Transform[] ikChainLock;
        private Vector3[] jointRotation = new Vector3[2];
        private Vector3 off;
        private Vector3 off2;
        private int foot = 1;

        public DragJointHand Initialize(
            Transform[] ikChain, bool foot,
            Meido meido, Func<Vector3> position, Func<Vector3> rotation
        )
        {
            base.Initialize(meido, position, rotation);
            this.ikChain = ikChain;
            this.foot = foot ? -1 : 1;
            this.ikChainLock = new Transform[3] {
                ikChain[foreArm],
                ikChain[foreArm],
                ikChain[hand]
            };

            InitializeIK();
            InitializeIK2();
            InitializeGizmo(this.ikChain[hand]);
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
            if (Utility.GetModKey(Utility.ModKey.Shift) && Utility.GetModKey(Utility.ModKey.Alt))
            {
                CurrentDragType = DragType.RotLocalY;
            }
            else if (Utility.GetModKey(Utility.ModKey.Alt))
            {
                CurrentDragType = DragType.RotLocalXZ;
            }
            else if (Utility.GetModKey(Utility.ModKey.Control))
            {
                CurrentDragType = DragType.MoveXZ;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

            Transform[] ikChain = CurrentDragType == DragType.MoveXZ ? this.ikChainLock : this.ikChain;

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            off = transform.position - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            );
            off2 = new Vector3(
                transform.position.x - ikChain[hand].position.x,
                transform.position.y - ikChain[hand].position.y,
                transform.position.z - ikChain[hand].position.z);

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

            if (CurrentDragType == DragType.None || CurrentDragType == DragType.MoveXZ)
            {
                Transform[] ikChain = CurrentDragType == DragType.MoveXZ ? this.ikChainLock : this.ikChain;

                IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos, Vector3.zero, ikData);

                jointRotation[handRot] = ikChain[hand].localEulerAngles;
                jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
                ikChain[upperArm].localEulerAngles = jointRotation[upperArm];
                ikChain[hand].localEulerAngles = jointRotation[handRot];
            }
            else
            {
                Vector3 vec31 = Input.mousePosition - mousePos;

                if (CurrentDragType == DragType.RotLocalY)
                {
                    ikChain[hand].localEulerAngles = jointRotation[handRot];
                    ikChain[hand].localRotation = Quaternion.Euler(ikChain[hand].localEulerAngles)
                        * Quaternion.AngleAxis(vec31.x / 1.5f, Vector3.right);
                }

                if (CurrentDragType == DragType.RotLocalXZ)
                {
                    ikChain[hand].localEulerAngles = jointRotation[handRot];
                    ikChain[hand].localRotation = Quaternion.Euler(ikChain[hand].localEulerAngles)
                        * Quaternion.AngleAxis(foot * vec31.x / 1.5f, Vector3.up);
                    ikChain[hand].localRotation = Quaternion.Euler(ikChain[hand].localEulerAngles)
                        * Quaternion.AngleAxis(foot * vec31.y / 1.5f, Vector3.forward);
                }
            }
        }
    }
}
