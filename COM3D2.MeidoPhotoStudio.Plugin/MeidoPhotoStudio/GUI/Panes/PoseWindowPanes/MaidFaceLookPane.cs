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

            this.lookXSlider = new Slider(Translation.Get("freeLook", "x"), -0.6f, 0.6f);
            this.lookXSlider.ControlEvent += (s, a) => SetMaidLook();

            this.lookYSlider = new Slider(Translation.Get("freeLook", "y"), 0.5f, -0.55f);
            this.lookYSlider.ControlEvent += (s, a) => SetMaidLook();

        }

        public void SetMaidLook()
        {
            if (updating) return;

            TBody body = this.meidoManager.ActiveMeido.Maid.body0;

            bool isPlaying = this.meidoManager.ActiveMeido.Maid.GetAnimation().isPlaying;
            body.offsetLookTarget = new Vector3(lookYSlider.Value * (isPlaying ? 1f : 0.6f), 1f, lookXSlider.Value);
        }

        public override void Update()
        {
            TBody body = this.meidoManager.ActiveMeido.Maid.body0;
            this.updating = true;
            this.lookXSlider.Value = body.offsetLookTarget.z;
            this.lookYSlider.Value = body.offsetLookTarget.x;
            this.updating = false;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUI.enabled = this.Enabled;
            GUILayout.BeginHorizontal();
            lookXSlider.Draw();
            lookYSlider.Draw();
            GUILayout.EndHorizontal();
        }
    }
}
