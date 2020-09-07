using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidFaceLookPane : BasePane
    {
        private MeidoManager meidoManager;
        private Slider lookXSlider;
        private Slider lookYSlider;
        private Toggle headToCamToggle;
        private Toggle eyeToCamToggle;

        public MaidFaceLookPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            this.lookXSlider = new Slider(Translation.Get("freeLookPane", "xSlider"), -0.6f, 0.6f);
            this.lookXSlider.ControlEvent += (s, a) => SetMaidLook();

            this.lookYSlider = new Slider(Translation.Get("freeLookPane", "ySlider"), 0.5f, -0.55f);
            this.lookYSlider.ControlEvent += (s, a) => SetMaidLook();

            this.headToCamToggle = new Toggle(Translation.Get("freeLookPane", "headToCamToggle"));
            this.headToCamToggle.ControlEvent += (s, a) => SetHeadToCam(headToCamToggle.Value, eye: false);

            this.eyeToCamToggle = new Toggle(Translation.Get("freeLookPane", "eyeToCamToggle"));
            this.eyeToCamToggle.ControlEvent += (s, a) => SetHeadToCam(eyeToCamToggle.Value, eye: true);
        }

        protected override void ReloadTranslation()
        {
            this.lookXSlider.Label = Translation.Get("freeLookPane", "xSlider");
            this.lookYSlider.Label = Translation.Get("freeLookPane", "ySlider");
            this.headToCamToggle.Label = Translation.Get("freeLookPane", "headToCamToggle");
            this.eyeToCamToggle.Label = Translation.Get("freeLookPane", "eyeToCamToggle");
        }

        public void SetHeadToCam(bool value, bool eye = false)
        {
            if (updating) return;

            Meido meido = this.meidoManager.ActiveMeido;

            if (eye) meido.EyeToCam = value;
            else meido.HeadToCam = value;
        }

        public void SetMaidLook()
        {
            if (updating) return;

            TBody body = this.meidoManager.ActiveMeido.Body;
            body.offsetLookTarget = new Vector3(lookYSlider.Value, 1f, lookXSlider.Value);
        }

        public void SetBounds()
        {
            float left = 0.5f;
            float right = -0.55f;
            if (this.meidoManager.ActiveMeido.Stop)
            {
                left *= 0.6f;
                right *= 0.6f;
            }
            this.lookYSlider.SetBounds(left, right);
        }

        public override void UpdatePane()
        {
            Meido meido = this.meidoManager.ActiveMeido;
            this.updating = true;
            this.SetBounds();
            this.lookXSlider.Value = meido.Body.offsetLookTarget.z;
            this.lookYSlider.Value = meido.Body.offsetLookTarget.x;
            this.eyeToCamToggle.Value = meido.EyeToCam;
            this.headToCamToggle.Value = meido.HeadToCam;
            this.updating = false;
        }

        public override void Draw()
        {
            GUI.enabled = this.meidoManager.HasActiveMeido && this.meidoManager.ActiveMeido.FreeLook;
            GUILayout.BeginHorizontal();
            this.lookXSlider.Draw();
            this.lookYSlider.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = this.meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            this.eyeToCamToggle.Draw();
            this.headToCamToggle.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }
    }
}
