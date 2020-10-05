using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    public class DragPointPelvis : DragPointMeido
    {
        private Quaternion pelvisRotation;

        protected override void ApplyDragType()
        {
            if (CurrentDragType == DragType.Ignore) ApplyProperties();
            else if (IsBone) ApplyProperties(false, false, false);
            else ApplyProperties(CurrentDragType != DragType.None, false, false);
        }

        protected override void UpdateDragType()
        {
            CurrentDragType = Input.Alt && !Input.Control
                ? Input.Shift ? DragType.RotLocalY : DragType.RotLocalXZ
                : OtherDragType() ? DragType.Ignore : DragType.None;
        }

        protected override void OnMouseDown()
        {
            base.OnMouseDown();
            pelvisRotation = MyObject.rotation;
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.None) return;

            if (isPlaying) meido.Stop = true;

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                MyObject.rotation = pelvisRotation;
                MyObject.Rotate(camera.transform.forward, mouseDelta.x / 6f, Space.World);
                MyObject.Rotate(camera.transform.right, mouseDelta.y / 4f, Space.World);
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                MyObject.rotation = pelvisRotation;
                MyObject.Rotate(Vector3.right * (mouseDelta.x / 2.2f));
            }
        }
    }
}
