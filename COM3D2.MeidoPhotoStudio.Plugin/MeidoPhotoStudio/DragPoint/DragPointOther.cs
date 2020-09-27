using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointBody : DragPointGeneral
    {
        public bool IsCube;
        private bool isIK;
        public bool IsIK
        {
            get => isIK;
            set
            {
                if (isIK != value)
                {
                    isIK = value;
                    ApplyDragType();
                }
            }
        }
        protected override void ApplyDragType()
        {
            bool enabled = !IsIK && (Transforming || Selecting);
            bool select = IsIK && Selecting;
            ApplyProperties(enabled || select, IsCube && enabled, false);

            if (IsCube) ApplyColours();
        }
    }

    internal class DragPointBG : DragPointGeneral
    {
        protected override void ApplyDragType()
        {
            ApplyProperties(Transforming, Transforming, Rotating);
            ApplyColours();
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
            DefaultRotation = MyObject.rotation;
            meshRenderers = new List<Renderer>(MyObject.GetComponentsInChildren<SkinnedMeshRenderer>());
            meshRenderers.AddRange(MyObject.GetComponentsInChildren<MeshRenderer>());
        }

        protected override void ApplyDragType()
        {
            bool active = (DragPointEnabled && Transforming) || Special;
            ApplyProperties(active, active, GizmoEnabled && Rotating);
            ApplyColours();
        }

        protected override void OnDestroy()
        {
            GameObject.Destroy(MyGameObject);
            base.OnDestroy();
        }
    }
}
