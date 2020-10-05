using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointFinger : DragPointMeido
    {
        private static readonly Color dragpointColour = new Color(0.1f, 0.4f, 0.95f, defaultAlpha);
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly Quaternion[] jointRotation = new Quaternion[2];
        private IKCtrlData ikCtrlData;
        private Transform[] ikChain;
        private bool baseFinger;

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            string parentName = myObject.parent.name.Split(' ')[2];
            // Base finger names have the form 'FingerN' or 'ToeN' where N is a natural number
            baseFinger = (parentName.Length == 7) || (parentName.Length == 4);
            ikChain = new Transform[2] {
                myObject.parent,
                myObject
            };
            ikCtrlData = IkCtrlData;
        }

        private void SetRotation(int joint)
        {
            Vector3 rotation = jointRotation[joint].eulerAngles;
            rotation.z = ikChain[joint].localEulerAngles.z;
            ikChain[joint].localRotation = Quaternion.Euler(rotation);
        }

        protected override void ApplyDragType()
        {
            if (baseFinger && CurrentDragType == DragType.RotLocalY) ApplyProperties(true, true, false);
            else if (CurrentDragType == DragType.MoveXZ) ApplyProperties(true, true, false);
            else ApplyProperties(false, false, false);
            ApplyColour(dragpointColour);
        }

        protected override void UpdateDragType()
        {
            CurrentDragType = Input.GetKey(MpsKey.DragFinger)
                ? Input.Shift
                    ? DragType.RotLocalY
                    : DragType.MoveXZ
                : DragType.None;
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
            jointRotation[jointUpper] = ikChain[jointUpper].localRotation;
            jointRotation[jointMiddle] = ikChain[jointMiddle].localRotation;
            InitializeIK(IK, ikChain[jointUpper], ikChain[jointUpper], ikChain[jointMiddle]);
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            if (CurrentDragType == DragType.MoveXZ)
            {
                Porc(IK, ikCtrlData, ikChain[jointUpper], ikChain[jointUpper], ikChain[jointMiddle]);
                if (!baseFinger)
                {
                    SetRotation(jointUpper);
                    SetRotation(jointMiddle);
                }
                else
                {
                    jointRotation[jointUpper] = ikChain[jointUpper].localRotation;
                    jointRotation[jointMiddle] = ikChain[jointMiddle].localRotation;
                }
            }
            else if (CurrentDragType == DragType.RotLocalY)
            {
                Vector3 mouseDelta = MouseDelta();

                ikChain[jointUpper].localRotation = jointRotation[jointUpper];
                ikChain[jointUpper].Rotate(Vector3.right * (mouseDelta.x / 1.5f));
            }
        }
    }
}
