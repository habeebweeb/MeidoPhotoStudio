using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        private List<Renderer> meshRenderers;
        public AttachPointInfo attachPointInfo = AttachPointInfo.Empty;
        public string Name => MyGameObject.name;
        public string assetName = string.Empty;
        public bool ShadowCasting
        {
            get
            {
                if (meshRenderers.Count == 0) return false;
                return meshRenderers[0].shadowCastingMode == ShadowCastingMode.On;
            }
            set
            {
                foreach (Renderer renderer in meshRenderers)
                {
                    renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
                }
            }
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            meshRenderers = new List<Renderer>(MyObject.GetComponentsInChildren<SkinnedMeshRenderer>());
            meshRenderers.AddRange(MyObject.GetComponentsInChildren<MeshRenderer>());
        }

        protected override void ApplyDragType()
        {
            DragType current = CurrentDragType;
            bool active = (DragPointEnabled && Transforming) || Special;
            ApplyProperties(active, active, GizmoEnabled && Rotating);
        }

        protected override void OnDestroy()
        {
            GameObject.Destroy(MyGameObject);
            base.OnDestroy();
        }
    }
}
