using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MaidFaceLookPane : BasePane
    {
        private readonly MeidoManager meidoManager;
        private readonly Slider lookXSlider;
        private readonly Slider lookYSlider;
        private readonly Toggle headToCamToggle;
        private readonly Toggle eyeToCamToggle;

        public MaidFaceLookPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            lookXSlider = new Slider(Translation.Get("freeLookPane", "xSlider"), -0.6f, 0.6f);
            lookXSlider.ControlEvent += (s, a) => SetMaidLook();

            lookYSlider = new Slider(Translation.Get("freeLookPane", "ySlider"), 0.5f, -0.55f);
            lookYSlider.ControlEvent += (s, a) => SetMaidLook();

            headToCamToggle = new Toggle(Translation.Get("freeLookPane", "headToCamToggle"));
            headToCamToggle.ControlEvent += (s, a) => SetHeadToCam(headToCamToggle.Value, eye: false);

            eyeToCamToggle = new Toggle(Translation.Get("freeLookPane", "eyeToCamToggle"));
            eyeToCamToggle.ControlEvent += (s, a) => SetHeadToCam(eyeToCamToggle.Value, eye: true);
        }

        protected override void ReloadTranslation()
        {
            lookXSlider.Label = Translation.Get("freeLookPane", "xSlider");
            lookYSlider.Label = Translation.Get("freeLookPane", "ySlider");
            headToCamToggle.Label = Translation.Get("freeLookPane", "headToCamToggle");
            eyeToCamToggle.Label = Translation.Get("freeLookPane", "eyeToCamToggle");
        }

        public void SetHeadToCam(bool value, bool eye = false)
        {
            if (updating) return;

            Meido meido = meidoManager.ActiveMeido;

            if (eye) meido.EyeToCam = value;
            else meido.HeadToCam = value;
        }

        public void SetMaidLook()
        {
            if (updating) return;

            TBody body = meidoManager.ActiveMeido.Body;
            body.offsetLookTarget = new Vector3(lookYSlider.Value, 1f, lookXSlider.Value);
        }

        public void SetBounds()
        {
            float left = 0.5f;
            float right = -0.55f;
            if (meidoManager.ActiveMeido.Stop)
            {
                left *= 0.6f;
                right *= 0.6f;
            }
            lookYSlider.SetBounds(left, right);
        }

        public override void UpdatePane()
        {
            Meido meido = meidoManager.ActiveMeido;
            updating = true;
            SetBounds();
            lookXSlider.Value = meido.Body.offsetLookTarget.z;
            lookYSlider.Value = meido.Body.offsetLookTarget.x;
            eyeToCamToggle.Value = meido.EyeToCam;
            headToCamToggle.Value = meido.HeadToCam;
            updating = false;
        }

        public override void Draw()
        {
            GUI.enabled = meidoManager.HasActiveMeido && meidoManager.ActiveMeido.FreeLook;
            GUILayout.BeginHorizontal();
            lookXSlider.Draw();
            lookYSlider.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = meidoManager.HasActiveMeido;

            GUILayout.BeginHorizontal();
            eyeToCamToggle.Draw();
            headToCamToggle.Draw();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }
    }
}
