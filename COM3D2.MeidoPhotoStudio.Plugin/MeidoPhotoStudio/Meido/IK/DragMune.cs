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

        public void Initialize(Transform[] ikChain, Maid maid, Func<Vector3> position, Func<Vector3> rotation)
        {
            base.Initialize(maid, position, rotation);
            this.ikChain = ikChain;

            for (int i = 0; i < things.Length; i++)
            {
                things[i] = new GameObject();
                things[i].transform.position = this.ikChain[i].position;
                things[i].transform.localRotation = this.ikChain[i].localRotation;
            }

            InitializeIK();
        }

        public void InitializeIK()
        {
            IK.Init(ikChain[upperArm], ikChain[foreArm], ikChain[hand], maid.body0);
        }

        protected override void GetDragType()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
            {
                dragType = DragType.RotLocalXZ;
            }
            else
            {
                dragType = DragType.None;
            }
        }

        protected override void DoubleClick()
        {
            if (dragType == DragType.RotLocalXZ)
            {
                maid.body0.MuneYureL(1f);
                maid.body0.MuneYureR(1f);
                maid.body0.jbMuneL.enabled = true;
                maid.body0.jbMuneR.enabled = true;
            }
        }

        protected override void InitializeDrag()
        {
            base.InitializeDrag();

            off = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z));
            off2 = new Vector3(
                transform.position.x - ikChain[hand].position.x,
                transform.position.y - ikChain[hand].position.y,
                transform.position.z - ikChain[hand].position.z);

            jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
            jointRotation[handRot] = ikChain[hand].localEulerAngles;
            maid.body0.MuneYureL(0f);
            maid.body0.MuneYureR(0f);
            maid.body0.jbMuneL.enabled = false;
            maid.body0.jbMuneR.enabled = false;
        }

        protected override void Drag()
        {
            if (dragType == DragType.None) return;

            if (isPlaying)
            {
                maid.GetAnimation().Stop();
            }

            IKCtrlData ikData = maid.body0.IKCtrl.GetIKData("左手");
            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, worldPoint.z)) + off - off2;

            if (dragType == DragType.RotLocalXZ)
            {
                IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos, Vector3.zero, ikData);
                // IK.Porc(ikChain[upperArm], ikChain[foreArm], ikChain[hand], pos + (pos - ikChain[hand].position), Vector3.zero, ikData);

                jointRotation[handRot] = ikChain[hand].localEulerAngles;
                jointRotation[upperArmRot] = ikChain[upperArm].localEulerAngles;
                ikChain[upperArm].localEulerAngles = jointRotation[upperArm];
                ikChain[hand].localEulerAngles = jointRotation[handRot];
            }
        }
    }
}
