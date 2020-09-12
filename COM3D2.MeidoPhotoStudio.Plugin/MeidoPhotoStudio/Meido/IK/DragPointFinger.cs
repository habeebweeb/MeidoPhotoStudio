using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointFinger : DragPointMeido
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private Transform[] ikChain;
        private Quaternion[] jointRotation = new Quaternion[2];
        private bool baseFinger;
        private static readonly Color dragpointColour = new Color(0.1f, 0.4f, 0.95f, defaultAlpha);

        public override void Set(Transform finger)
        {
            base.Set(finger);
            string parentName = finger.parent.name.Split(' ')[2];
            // Base finger names have the form 'FingerN' or 'ToeN' where N is a natural number
            this.baseFinger = (parentName.Length == 7) || (parentName.Length == 4);
            this.ikChain = new Transform[2] {
                finger.parent,
                finger
            };
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
            if (Input.GetKey(KeyCode.Space))
            {
                CurrentDragType = Utility.GetModKey(Utility.ModKey.Shift)
                    ? DragType.RotLocalY
                    : DragType.MoveXZ;
            }
            else CurrentDragType = DragType.None;
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
                Porc(IK, ikChain[jointUpper], ikChain[jointUpper], ikChain[jointMiddle]);
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
