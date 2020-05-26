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

        public void Initialize(Transform[] ikChain, bool foot, Maid maid, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(maid, position, rotation);
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
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftAlt))
            {
                dragType = DragType.RotLocalY;
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                dragType = DragType.RotLocalXZ;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                dragType = DragType.MoveXZ;
            }
            else
            {
                dragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

            Transform[] ikChain = dragType == DragType.MoveXZ ? this.ikChainLock : this.ikChain;

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            off = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z));
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
            if (isPlaying)
            {
                maid.GetAnimation().Stop();
            }

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;

            if (dragType == DragType.None || dragType == DragType.MoveXZ)
            {
                Transform[] ikChain = dragType == DragType.MoveXZ ? this.ikChainLock : this.ikChain;

                IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos, Vector3.zero, ikData);

                jointRotation[handRot] = ikChain[hand].localEulerAngles;
                jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
                ikChain[upperArm].localEulerAngles = jointRotation[upperArm];
                ikChain[hand].localEulerAngles = jointRotation[handRot];
            }
            else
            {
                Vector3 vec31 = Input.mousePosition - mousePos;

                if (dragType == DragType.RotLocalY)
                {
                    ikChain[hand].localEulerAngles = jointRotation[handRot];
                    ikChain[hand].localRotation = Quaternion.Euler(ikChain[hand].localEulerAngles)
                        * Quaternion.AngleAxis(vec31.x / 1.5f, Vector3.right);
                }

                if (dragType == DragType.RotLocalXZ)
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
