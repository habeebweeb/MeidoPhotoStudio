using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static TBody;
    internal class DragPointGravity : DragPointGeneral
    {
        private static readonly SlotID[] skirtSlots = { SlotID.skirt, SlotID.onepiece, SlotID.mizugi, SlotID.panz };
        private static readonly SlotID[] hairSlots = { SlotID.hairF, SlotID.hairR, SlotID.hairS, SlotID.hairT };
        public GravityTransformControl Control { get; private set; }
        public bool Valid => Control.isValid;
        public bool Active => Valid && gameObject.activeSelf;

        public static GravityTransformControl MakeGravityControl(Maid maid, bool skirt = false)
        {
            string category = skirt ? "skirt" : "hair";

            Transform bone = maid.body0.GetBone("Bip01");
            string gravityGoName = $"GravityDatas_{maid.status.guid}_{category}";
            Transform gravityTransform = maid.gameObject.transform.Find(gravityGoName);
            if (gravityTransform == null)
            {
                GameObject go = new GameObject(gravityGoName);
                go.transform.SetParent(bone, false);
                go.transform.SetParent(maid.transform, true);
                go.transform.localScale = Vector3.one;
                go.transform.rotation = Quaternion.identity;
                GameObject go2 = new GameObject(gravityGoName);
                go2.transform.SetParent(go.transform, false);
                gravityTransform = go2.transform;
            }
            else
            {
                gravityTransform = gravityTransform.GetChild(0);
                GravityTransformControl control = gravityTransform.GetComponent<GravityTransformControl>();
                if (control != null) GameObject.Destroy(control);
            }

            GravityTransformControl gravityControl = gravityTransform.gameObject.AddComponent<GravityTransformControl>();

            SlotID[] slots = skirt ? skirtSlots : hairSlots;

            gravityControl.SetTargetSlods(slots);
            gravityControl.forceRate = 0.1f;

            return gravityControl;
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            Control = myObject.GetComponent<GravityTransformControl>();
            gameObject.SetActive(false);
        }

        protected override void ApplyDragType()
        {
            ApplyProperties(Moving, Moving, false);
            ApplyColours();
        }

        protected override void OnDestroy()
        {
            if (Control.isValid)
            {
                Control.transform.localPosition = Vector3.zero;
                Control.Update();
            }
            GameObject.Destroy(Control.transform.parent.gameObject);
            base.OnDestroy();
        }

        private void OnDisable() => Control.isEnabled = false;

        private void OnEnable()
        {
            if (Control)
            {
                Control.isEnabled = true;
                if (!Control.isEnabled) gameObject.SetActive(false);
            }
        }
    }
}
