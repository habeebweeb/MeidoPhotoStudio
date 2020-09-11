using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class GravityControlPane : BasePane
    {
        private MeidoManager meidoManager;
        private Toggle hairToggle;
        private Toggle skirtToggle;
        private Toggle globalToggle;

        public GravityControlPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            hairToggle = new Toggle(Translation.Get("gravityControlPane", "hairToggle"));
            hairToggle.ControlEvent += (s, a) => ToggleGravity(hairToggle.Value, skirt: false);

            skirtToggle = new Toggle(Translation.Get("gravityControlPane", "skirtToggle"));
            skirtToggle.ControlEvent += (s, a) => ToggleGravity(skirtToggle.Value, skirt: true);

            globalToggle = new Toggle(Translation.Get("gravityControlPane", "globalToggle"));
            globalToggle.ControlEvent += (s, a) => SetGlobalGravity(globalToggle.Value);
        }

        public override void Draw()
        {
            bool enabled = this.meidoManager.HasActiveMeido;
            GUI.enabled = enabled;

            Meido meido = this.meidoManager.ActiveMeido;
            GUILayout.BeginHorizontal();

            GUI.enabled = enabled && meido.HairGravityValid;
            hairToggle.Draw();

            GUI.enabled = enabled && meido.SkirtGravityValid;
            skirtToggle.Draw();

            GUI.enabled = true;
            GUILayout.EndHorizontal();

            globalToggle.Draw();

            GUI.enabled = true;
        }

        public override void UpdatePane()
        {
            if (!this.meidoManager.HasActiveMeido) return;

            Meido meido = meidoManager.ActiveMeido;

            this.updating = true;

            hairToggle.Value = meido.HairGravityActive;
            skirtToggle.Value = meido.SkirtGravityActive;

            this.updating = false;
        }

        private void ToggleGravity(bool value, bool skirt = false)
        {
            if (updating) return;

            if (this.meidoManager.GlobalGravity)
            {
                foreach (Meido meido in this.meidoManager.ActiveMeidoList)
                {
                    if (skirt) meido.SkirtGravityActive = value;
                    else meido.HairGravityActive = value;
                }
            }
            else
            {
                if (skirt) this.meidoManager.ActiveMeido.SkirtGravityActive = value;
                else this.meidoManager.ActiveMeido.HairGravityActive = value;
            }
        }

        private void SetGlobalGravity(bool value) => this.meidoManager.GlobalGravity = value;
    }
}
