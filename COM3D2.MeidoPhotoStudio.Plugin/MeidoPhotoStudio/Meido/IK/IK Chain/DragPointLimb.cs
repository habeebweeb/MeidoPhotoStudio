using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointLimb : DragPointChain
    {
        private int foot = 1;
        private bool isLower;
        private bool isMiddle;
        private bool isUpper;
        public override bool IsBone
        {
            set
            {
                base.IsBone = value;
                BaseScale = isBone ? boneScale : OriginalScale;
            }
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);

            string name = myObject.name;

            foot = name.EndsWith("Foot") ? -1 : 1;
            isLower = name.EndsWith("Hand") || foot == -1;
            isMiddle = name.EndsWith("Calf") || name.EndsWith("Forearm");
            isUpper = !isMiddle && !isLower;

            if (isLower) ikChain[0] = ikChain[0].parent;
        }

        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            bool isBone = IsBone;
            if (CurrentDragType == DragType.Ignore) ApplyProperties();
            else if (current == DragType.RotLocalXZ)
            {
                if (isLower) ApplyProperties(!isBone, false, isBone);
                else ApplyProperties();
            }
            else if (current == DragType.RotLocalY)
            {
                if (isLower || isMiddle) ApplyProperties(!isBone, false, false);
                else if (isUpper) ApplyProperties(false, false, isBone);
                else ApplyProperties();
            }
            else if (current == DragType.RotY)
            {
                if (isMiddle) ApplyProperties(false, false, isBone);
                else ApplyProperties();
            }
            else if (current == DragType.MoveXZ)
            {
                if (isLower) ApplyProperties(true, isBone, false);
                else ApplyProperties();
            }
            else ApplyProperties(true, isBone, false);
        }

        protected override void UpdateDragType()
        {
            bool control = Input.Control;
            bool alt = Input.Alt;
            // Check for DragMove so that hand dragpoint is not in the way
            if (OtherDragType()) CurrentDragType = DragType.Ignore;
            else if (control && !Input.GetKey(MpsKey.DragMove))
            {
                if (alt) CurrentDragType = DragType.RotY;
                else CurrentDragType = DragType.MoveXZ;
            }
            else if (alt) CurrentDragType = Input.Shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            else CurrentDragType = Input.Shift ? DragType.Ignore : DragType.None;
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            bool altRotation = CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.RotY;

            if (CurrentDragType == DragType.None || altRotation)
            {
                int upperJoint = altRotation ? jointMiddle : jointUpper;

                Porc(IK, ikCtrlData, ikChain[upperJoint], ikChain[jointMiddle], ikChain[jointLower]);

                InitializeRotation();
            }

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.RotLocalY)
            {
                int joint = isMiddle ? jointUpper : jointLower;
                ikChain[joint].localRotation = jointRotation[joint];
                ikChain[joint].Rotate(Vector3.right * (-mouseDelta.x / 1.5f));
            }

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                ikChain[jointLower].localRotation = jointRotation[jointLower];
                ikChain[jointLower].Rotate(Vector3.up * (foot * mouseDelta.x / 1.5f));
                ikChain[jointLower].Rotate(Vector3.forward * (foot * mouseDelta.y / 1.5f));
            }
        }
    }
}
