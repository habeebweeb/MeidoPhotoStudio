using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class GravityControlPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Toggle hairToggle;
        private readonly Toggle skirtToggle;
        private readonly Toggle globalToggle;

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
            bool enabled = meidoManager.HasActiveMeido;
            GUI.enabled = enabled;

            Meido meido = meidoManager.ActiveMeido;
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
            if (!meidoManager.HasActiveMeido) return;

            Meido meido = meidoManager.ActiveMeido;

            updating = true;

            hairToggle.Value = meido.HairGravityActive;
            skirtToggle.Value = meido.SkirtGravityActive;

            updating = false;
        }

        private void ToggleGravity(bool value, bool skirt = false)
        {
            if (updating) return;

            if (meidoManager.GlobalGravity)
            {
                foreach (Meido meido in meidoManager.ActiveMeidoList)
                {
                    if (skirt) meido.SkirtGravityActive = value;
                    else meido.HairGravityActive = value;
                }
            }
            else
            {
                if (skirt) meidoManager.ActiveMeido.SkirtGravityActive = value;
                else meidoManager.ActiveMeido.HairGravityActive = value;
            }
        }

        private void SetGlobalGravity(bool value) => meidoManager.GlobalGravity = value;
    }
}
