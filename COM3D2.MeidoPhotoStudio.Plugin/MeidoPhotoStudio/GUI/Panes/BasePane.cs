using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BasePane : BaseControl
    {
        protected List<BaseControl> Controls { get; set; }
        protected List<BasePane> Panes { get; set; }
        protected bool updating = false;

        public BasePane()
        {
            Controls = new List<BaseControl>();
            Panes = new List<BasePane>();
        }
    }
}
