using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class DragMune : BaseDrag
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly GameObject[] things = new GameObject[3];
        private Transform[] ikChain;
        private Vector3[] jointRotation = new Vector3[2];
        private Vector3 off;
        private Vector3 off2;

        public DragMune Initialize(Transform[] ikChain, Meido meido, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(meido, position, rotation);
            this.ikChain = ikChain;

            for (int i = 0; i < things.Length; i++)
            {
                things[i] = new GameObject();
                things[i].transform.position = this.ikChain[i].position;
                things[i].transform.localRotation = this.ikChain[i].localRotation;
            }

            InitializeIK();
            return this;
        }

        public void InitializeIK()
        {
            IK.Init(ikChain[upperArm], ikChain[foreArm], ikChain[hand], maid.body0);
        }

        protected override void GetDragType()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
            {
                CurrentDragType = DragType.RotLocalXZ;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void DoubleClick()
        {
            if (CurrentDragType == DragType.RotLocalXZ) meido.SetMune();
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
                transform.position.z - ikChain[hand].position.z);

            jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
            jointRotation[handRot] = ikChain[hand].localEulerAngles;
            meido.SetMune(true);
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.None) return;

            if (isPlaying) meido.IsStop = true;
            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            Vector3 pos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)
            ) + off - off2;

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos, Vector3.zero, ikData);

                jointRotation[handRot] = ikChain[hand].localEulerAngles;
                jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
                ikChain[upperArm].localEulerAngles = jointRotation[upperArm];
                ikChain[hand].localEulerAngles = jointRotation[handRot];
            }
        }
    }
}
