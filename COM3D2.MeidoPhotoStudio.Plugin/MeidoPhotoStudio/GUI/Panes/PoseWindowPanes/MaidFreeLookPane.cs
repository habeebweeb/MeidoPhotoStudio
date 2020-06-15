using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidFaceLookPane : BasePane
    {
        private MeidoManager meidoManager;
        private Slider lookXSlider;
        private Slider lookYSlider;

        public MaidFaceLookPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            // this.meidoManager.AnimeChange += (s, a) => SetBounds();

            this.lookXSlider = new Slider(Translation.Get("freeLook", "x"), -0.6f, 0.6f);
            this.lookXSlider.ControlEvent += (s, a) => SetMaidLook();

            this.lookYSlider = new Slider(Translation.Get("freeLook", "y"), 0.5f, -0.55f);
            this.lookYSlider.ControlEvent += (s, a) => SetMaidLook();

        }

        protected override void ReloadTranslation()
        {
            this.lookXSlider.Label = Translation.Get("freeLook", "x");
            this.lookYSlider.Label = Translation.Get("freeLook", "y");
        }

        public void SetMaidLook()
        {
            if (updating) return;

            TBody body = this.meidoManager.ActiveMeido.Maid.body0;
            body.offsetLookTarget = new Vector3(lookYSlider.Value, 1f, lookXSlider.Value);
        }

        public void SetBounds()
        {
            float left = 0.5f;
            float right = -0.55f;
            if (this.meidoManager.ActiveMeido.IsStop)
            {
                left *= 0.6f;
                right *= 0.6f;
            }
            this.lookYSlider.SetBounds(left, right);
        }

        public override void UpdatePane()
        {
            TBody body = this.meidoManager.ActiveMeido.Maid.body0;
            this.updating = true;
            this.SetBounds();
            this.lookXSlider.Value = body.offsetLookTarget.z;
            this.lookYSlider.Value = body.offsetLookTarget.x;
            this.updating = false;
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido && this.meidoManager.ActiveMeido.IsFreeLook;
            GUILayout.BeginHorizontal();
            lookXSlider.Draw();
            lookYSlider.Draw();
            GUILayout.EndHorizontal();
        }
    }
}
