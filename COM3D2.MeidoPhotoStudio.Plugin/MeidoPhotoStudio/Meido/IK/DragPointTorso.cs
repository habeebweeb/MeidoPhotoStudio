using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using Input = InputManager;
    internal class DragPointTorso : DragPointMeido
    {
        private static readonly float[] blah = new[] { 0.03f, 0.1f, 0.09f, 0.07f };
        private static readonly float[] something = new[] { 0.08f, 0.15f };
        private readonly Quaternion[] spineRotation = new Quaternion[4];
        private readonly Transform[] spine = new Transform[4];

        public override void Set(Transform spine1a)
        {
            base.Set(spine1a);
            Transform spine = spine1a;
            for (int i = 0; i < this.spine.Length; i++)
            {
                this.spine[i] = spine;
                spine = spine.parent;
            }
        }

        protected override void ApplyDragType()
        {
            if (CurrentDragType == DragType.Ignore) ApplyProperties();
            else if (IsBone) ApplyProperties(false, false, false);
            else ApplyProperties(CurrentDragType != DragType.None, false, false);
        }

        protected override void UpdateDragType()
        {
            if (Input.Alt && !Input.Control)
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
            for (int i = 0; i < spine.Length; i++)
            {
                spineRotation[i] = spine[i].localRotation;
            }
        }

        protected override void Drag()
        {
            if (CurrentDragType == DragType.None) return;

            if (isPlaying) meido.Stop = true;

            Vector3 mouseDelta = MouseDelta();

            if (CurrentDragType == DragType.RotLocalXZ)
            {
                for (int i = 0; i < spine.Length; i++)
                {
                    spine[i].localRotation = spineRotation[i];
                    spine[i].Rotate(
                        camera.transform.forward, -mouseDelta.x / 1.5f * blah[i], Space.World
                    );
                    spine[i].Rotate(camera.transform.right, mouseDelta.y * blah[i], Space.World);
                }
            }

            if (CurrentDragType == DragType.RotLocalY)
            {
                for (int i = 0; i < spine.Length; i++)
                {
                    spine[i].localRotation = spineRotation[i];
                    spine[i].Rotate(Vector3.right * (mouseDelta.x / 1.5f * something[i / 2]));
                }
            }
        }
    }
}
