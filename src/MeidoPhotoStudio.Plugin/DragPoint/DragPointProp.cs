using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin
{
    public class DragPointProp : DragPointGeneral
    {
        private List<Renderer> renderers;
        public AttachPointInfo AttachPointInfo { get; private set; } = AttachPointInfo.Empty;
        public string Name => MyGameObject.name;
        public string assetName = string.Empty;
        public PropInfo Info { get; set; }

        public bool ShadowCasting
        {
            get => renderers.Count != 0 && renderers.Any(r => r.shadowCastingMode == ShadowCastingMode.On);
            set
            {
                foreach (var renderer in renderers)
                    renderer.shadowCastingMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            DefaultRotation = MyObject.rotation;
            DefaultPosition = MyObject.position;
            DefaultScale = MyObject.localScale;
            renderers = new List<Renderer>(MyObject.GetComponentsInChildren<Renderer>());
        }

        public void AttachTo(Meido meido, AttachPoint point, bool keepWorldPosition = true)
        {
            var attachPoint = meido?.IKManager.GetAttachPointTransform(point);

            AttachPointInfo = meido == null ? AttachPointInfo.Empty : new AttachPointInfo(point, meido);

            var position = MyObject.position;
            var rotation = MyObject.rotation;
            var scale = MyObject.localScale;

            MyObject.transform.SetParent(attachPoint, keepWorldPosition);

            if (keepWorldPosition)
            {
                MyObject.position = position;
                MyObject.rotation = rotation;
            }
            else
            {
                MyObject.localPosition = Vector3.zero;
                MyObject.rotation = Quaternion.identity;
            }

            MyObject.localScale = scale;

            if (attachPoint == null) Utility.FixGameObjectScale(MyGameObject);
        }

        public void DetachFrom(bool keepWorldPosition = true) => AttachTo(null, AttachPoint.None, keepWorldPosition);

        public void DetachTemporary()
        {
            MyObject.transform.SetParent(null, true);
            Utility.FixGameObjectScale(MyGameObject);
        }

        protected override void ApplyDragType()
        {
            var active = DragPointEnabled && Transforming || Special;
            ApplyProperties(active, active, GizmoEnabled && Rotating);
            ApplyColours();
        }

        protected override void OnDestroy()
        {
            Destroy(MyGameObject);
            base.OnDestroy();
        }
    }

    public class PropInfo
    {
        public enum PropType { Mod, MyRoom, Bg, Odogu }

        public PropType Type { get; }
        public string IconFile { get; set; }
        public string Filename { get; set; }
        public string SubFilename { get; set; }
        public int MyRoomID { get; set; }

        public PropInfo(PropType type) => Type = type;

        public static PropInfo FromModItem(ModItem modItem) => new(PropType.Mod)
        {
            Filename = modItem.IsOfficialMod ? Path.GetFileName(modItem.MenuFile) : modItem.MenuFile,
            SubFilename = modItem.BaseMenuFile
        };

        public static PropInfo FromMyRoom(MyRoomItem myRoomItem) => new(PropType.MyRoom)
        {
            MyRoomID = myRoomItem.ID, Filename = myRoomItem.PrefabName
        };

        public static PropInfo FromBg(string name) => new(PropType.Bg) { Filename = name };

        public static PropInfo FromGameProp(string name) => new(PropType.Odogu) { Filename = name };
    }
}
