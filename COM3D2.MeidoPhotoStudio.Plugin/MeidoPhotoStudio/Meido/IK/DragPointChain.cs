using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointChain : DragPointMeido
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private Transform[] ikChain;
        private Quaternion[] jointRotation = new Quaternion[3];
        private int foot = 1;
        private bool isLower = false;
        private bool isMiddle = false;
        private bool isUpper = false;
        private bool isMune = false;

        public override void Set(Transform lower)
        {
            base.Set(lower);
            this.isMune = lower.name.StartsWith("Mune");
            this.foot = lower.name.EndsWith("Foot") ? -1 : 1;
            this.isLower = lower.name.EndsWith("Hand") || foot == -1;
            this.isMiddle = lower.name.EndsWith("Calf") || lower.name.EndsWith("Forearm");
            this.isUpper = !(isMiddle || isLower) && !isMune;
            this.ikChain = new Transform[] {
                lower.parent,
                lower.parent,
                lower
            };
            if (this.isLower) ikChain[0] = ikChain[0].parent;
        }

        private void InitializeRotation()
        {
            for (int i = 0; i < jointRotation.Length; i++)
            {
                jointRotation[i] = ikChain[i].localRotation;
            }
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
                if (isMune) ApplyProperties(true, false, false);
                else if (isMiddle) ApplyProperties(false, false, isBone);
                else ApplyProperties();
            }
            else if (current == DragType.MoveXZ)
            {
                if (isLower) ApplyProperties(true, isBone, false);
                else ApplyProperties();
            }
            else ApplyProperties(!isMune, (isBone && !isMune), false);
        }

        protected override void UpdateDragType()
        {
            bool control = Utility.GetModKey(Utility.ModKey.Control);
            bool alt = Utility.GetModKey(Utility.ModKey.Alt);

            if (Input.GetKey(KeyCode.Space) || OtherDragType())
            {
                CurrentDragType = DragType.Ignore;
            }
            else if (control && alt)
            {
                // mune
                CurrentDragType = DragType.RotY;
            }
            else if (control)
            {
                CurrentDragType = DragType.MoveXZ;
            }
            else if (alt)
            {
                bool shift = Utility.GetModKey(Utility.ModKey.Shift);
                CurrentDragType = shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else
            {
                CurrentDragType = DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();

            if (isMune) meido.SetMune(true);

            InitializeRotation();

            InitializeIK(IK, ikChain[jointUpper], ikChain[jointMiddle], ikChain[jointLower]);
        }

        protected override void OnDoubleClick()
        {
            if (isMune && CurrentDragType == DragType.RotY) meido.SetMune();
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            bool altRotation = CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.RotY;

            if ((CurrentDragType == DragType.None) || altRotation)
            {
                int upperJoint = altRotation ? jointMiddle : jointUpper;

                Porc(IK, ikChain[upperJoint], ikChain[jointMiddle], ikChain[jointLower]);

                InitializeRotation();
            }

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.RotLocalY)
            {
                int joint = this.isMiddle ? jointUpper : jointLower;
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
