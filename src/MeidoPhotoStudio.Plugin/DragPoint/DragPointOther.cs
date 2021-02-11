using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeidoPhotoStudio.Plugin
{
    public class DragPointBody : DragPointGeneral
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

    public class DragPointBG : DragPointGeneral
    {
        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            DefaultPosition = myObject.position;
        }

        protected override void ApplyDragType()
        {
            ApplyProperties(Transforming, Transforming, Rotating);
            ApplyColours();
        }
    }
}
