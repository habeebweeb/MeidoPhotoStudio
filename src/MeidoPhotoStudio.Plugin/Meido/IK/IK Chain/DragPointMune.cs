using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointMune : DragPointChain
    {
        private bool isMuneL;
        private int inv = 1;

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            isMuneL = myObject.name[5] == 'L'; // Mune_L_Sub
            if (isMuneL) inv *= -1;
        }

        protected override void ApplyDragType() => ApplyProperties(CurrentDragType != DragType.None, false, false);

        protected override void OnMouseDown()
        {
            base.OnMouseDown();

            meido.SetMune(false, isMuneL);
        }

        protected override void OnDoubleClick()
        {
            if (CurrentDragType != DragType.None) meido.SetMune(true, isMuneL);
        }

        protected override void UpdateDragType()
        {
            if (Input.Control && Input.Alt) CurrentDragType = Input.Shift ? DragType.RotLocalY : DragType.RotLocalXZ;
            else CurrentDragType = DragType.None;
        }

        protected override void Drag()
        {
            if (isPlaying) meido.Stop = true;

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                Porc(IK, ikCtrlData, ikChain[jointUpper], ikChain[jointMiddle], ikChain[jointLower]);
                InitializeRotation();
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                Vector3 mouseDelta = MouseDelta();
                ikChain[jointLower].localRotation = jointRotation[jointLower];
                ikChain[jointLower].Rotate(Vector3.up * (-mouseDelta.x / 1.5f) * inv);
                ikChain[jointLower].Rotate(Vector3.forward * (mouseDelta.y / 1.5f) * inv);
            }
        }
    }
}