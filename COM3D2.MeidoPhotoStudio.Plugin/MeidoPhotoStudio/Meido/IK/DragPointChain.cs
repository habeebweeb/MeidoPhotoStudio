using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    internal class DragPointChain : DragPointMeido
    {
        private readonly TBody.IKCMO IK = new TBody.IKCMO();
        private readonly Quaternion[] jointRotation = new Quaternion[3];
        private IKCtrlData ikCtrlData;
        private Transform[] ikChain;
        private int foot = 1;
        private bool isLower;
        private bool isMiddle;
        private bool isUpper;
        private bool isMune;
        private bool isMuneL;

        public override void Set(Transform myObject)
        {
            base.Set(myObject);

            string name = myObject.name;

            isMune = name.StartsWith("Mune");
            isMuneL = isMune && name[5] == 'L'; // Mune_L_Sub
            foot = name.EndsWith("Foot") ? -1 : 1;
            isLower = name.EndsWith("Hand") || foot == -1;
            isMiddle = name.EndsWith("Calf") || name.EndsWith("Forearm");
            isUpper = !(isMiddle || isLower) && !isMune;

            ikChain = new Transform[] {
                myObject.parent,
                myObject.parent,
                myObject
            };

            if (isLower) ikChain[0] = ikChain[0].parent;

            ikCtrlData = IkCtrlData;
        }

        private void InitializeRotation()
        {
            for (int i = 0; i < jointRotation.Length; i++) jointRotation[i] = ikChain[i].localRotation;
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
            else ApplyProperties(!isMune, isBone && !isMune, false);
        }

        protected override void UpdateDragType()
        {
            bool control = Input.Control;
            bool alt = Input.Alt;

            if (control && alt)
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
                CurrentDragType = Input.Shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            }
            else
            {
                CurrentDragType = OtherDragType() ? DragType.Ignore : DragType.None;
            }
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();

            if (isMune) meido.SetMune(false, isMuneL);

            InitializeRotation();

            InitializeIK(IK, ikChain[jointUpper], ikChain[jointMiddle], ikChain[jointLower]);
        }

        protected override void OnDoubleClick()
        {
            if (isMune && CurrentDragType == DragType.RotY) meido.SetMune(true, isMuneL);
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            bool altRotation = CurrentDragType == DragType.MoveXZ || CurrentDragType == DragType.RotY;

            if ((CurrentDragType == DragType.None) || altRotation)
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
