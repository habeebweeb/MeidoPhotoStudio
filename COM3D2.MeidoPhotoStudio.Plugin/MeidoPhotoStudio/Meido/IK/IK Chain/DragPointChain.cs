using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class DragPointChain : DragPointMeido
    {
        protected readonly TBody.IKCMO IK = new TBody.IKCMO();
        protected readonly Quaternion[] jointRotation = new Quaternion[3];
        protected IKCtrlData ikCtrlData;
        protected Transform[] ikChain;

        public override void Set(Transform myObject)
        {
            base.Set(myObject);

            ikChain = new Transform[] {
                myObject.parent,
                myObject.parent,
                myObject
            };

            ikCtrlData = IkCtrlData;
        }

        protected void InitializeRotation()
        {
            for (int i = 0; i < jointRotation.Length; i++) jointRotation[i] = ikChain[i].localRotation;
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();

            InitializeRotation();

            InitializeIK(IK, ikChain[jointUpper], ikChain[jointMiddle], ikChain[jointLower]);
        }
    }
}
