using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public abstract class BaseWindowPane : BasePane
    {
        protected List<BasePane> Panes = new List<BasePane>();
        protected Vector2 scrollPos;
        public bool ActiveWindow { get; set; }

        public virtual void UpdatePanes()
        {
            foreach (BasePane pane in Panes) pane.UpdatePane();
        }
    }
}
