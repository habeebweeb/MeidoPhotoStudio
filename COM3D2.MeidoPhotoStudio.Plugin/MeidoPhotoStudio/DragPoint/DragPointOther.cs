using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointBody : DragPointGeneral
    {
        public bool IsCube = false;
        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            bool transforming = !(current == DragType.None || current == DragType.Delete);
            ApplyProperties(transforming, IsCube && transforming, false);
        }
    }

    internal class DragPointBG : DragPointGeneral
    {
        protected override void ApplyDragType()
        {
            ApplyProperties(Transforming, Transforming, Rotating);
        }
    }

    internal class DragPointDogu : DragPointGeneral
    {
        public AttachPointInfo attachPointInfo = AttachPointInfo.Empty;
        public string Name => MyGameObject.name;
        public string assetName = string.Empty;

        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            bool active = Transforming || Special;
            ApplyProperties(active, active, Rotating);
        }

        protected override void OnDestroy()
        {
            GameObject.Destroy(MyGameObject);
            base.OnDestroy();
        }
    }
}
