using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragJointForearm : BaseDrag
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly GameObject[] otherIK = new GameObject[3];
        private Transform[] ikChain;
        private Vector3[] jointRotation = new Vector3[2];
        private Vector3 off;
        private Vector3 off2;
        private bool knee = false;

        public DragJointForearm Initialize(
            Transform[] ikChain, bool knee,
            Meido meido, Func<Vector3> position, Func<Vector3> rotation
        )
        {
            base.Initialize(meido, position, rotation);
            this.ikChain = ikChain;
            this.knee = knee;

            for (int i = 0; i < otherIK.Length; i++)
            {
                otherIK[i] = new GameObject();
                otherIK[i].transform.position = this.ikChain[i].position;
                otherIK[i].transform.localRotation = this.ikChain[i].localRotation;
            }

            InitializeIK();

            InitializeGizmo(this.ikChain[hand]);
            return this;
        }

        public void InitializeIK()
        {
            IK.Init(ikChain[upperArm], ikChain[foreArm], ikChain[hand], maid.body0);
        }

        protected override void GetDragType()
        {
            if (knee && Utility.GetModKey(Utility.ModKey.Shift) && Utility.GetModKey(Utility.ModKey.Alt))
            {
                CurrentDragType = DragType.RotLocalY;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

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
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;

            if (CurrentDragType == DragType.None)
            {
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
                    ikChain[upperArm].localEulerAngles = jointRotation[upperArmRot];
                    ikChain[upperArm].localRotation = Quaternion.Euler(ikChain[upperArm].localEulerAngles)
                        * Quaternion.AngleAxis((-vec31.x / 1.5f), Vector3.right);
                }
            }
        }
    }
}
