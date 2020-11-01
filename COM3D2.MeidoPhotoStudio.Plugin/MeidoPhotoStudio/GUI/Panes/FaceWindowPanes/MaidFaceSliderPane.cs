using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static Meido;
    public class MaidFaceSliderPane : BasePane
    {
        // TODO: Consider placing in external file to be user editable
        private static readonly Dictionary<string, SliderProp> SliderRange = new Dictionary<string, SliderProp>()
        {
            // Eye Shut
            ["eyeclose"] = new SliderProp(0f, 1f),
            // Eye Smile
            ["eyeclose2"] = new SliderProp(0f, 1f),
            // Glare
            ["eyeclose3"] = new SliderProp(0f, 1f),
            // Wide Eyes
            ["eyebig"] = new SliderProp(0f, 1f),
            // Wink 1
            ["eyeclose6"] = new SliderProp(0f, 1f),
            // Wink 2
            ["eyeclose5"] = new SliderProp(0f, 1f),
            // Highlight
            ["hitomih"] = new SliderProp(0f, 2f),
            // Pupil Size
            ["hitomis"] = new SliderProp(0f, 3f),
            // Brow 1
            ["mayuha"] = new SliderProp(0f, 1f),
            // Brow 2
            ["mayuw"] = new SliderProp(0f, 1f),
            // Brow Up
            ["mayuup"] = new SliderProp(0f, 0.8f),
            // Brow Down 1
            ["mayuv"] = new SliderProp(0f, 0.8f),
            // Brow Down 2
            ["mayuvhalf"] = new SliderProp(0f, 0.9f),
            // Mouth Open 1
            ["moutha"] = new SliderProp(0f, 1f),
            // Mouth Open 2
            ["mouths"] = new SliderProp(0f, 0.9f),
            // Mouth Narrow
            ["mouthc"] = new SliderProp(0f, 1f),
            // Mouth Widen
            ["mouthi"] = new SliderProp(0f, 1f),
            // Smile
            ["mouthup"] = new SliderProp(0f, 1.4f),
            // Frown
            ["mouthdw"] = new SliderProp(0f, 1f),
            // Mouth Pucker
            ["mouthhe"] = new SliderProp(0f, 1f),
            // Grin
            ["mouthuphalf"] = new SliderProp(0f, 2f),
            // Tongue Out
            ["tangout"] = new SliderProp(0f, 1f),
            // Tongue Up
            ["tangup"] = new SliderProp(0f, 0.7f),
            // Tongue Base
            ["tangopen"] = new SliderProp(0f, 1f)
        };
        private readonly MeidoManager meidoManager;
        private readonly Dictionary<string, BaseControl> faceControls;
        private bool hasTangOpen;

        public MaidFaceSliderPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;
            faceControls = new Dictionary<string, BaseControl>();

            foreach (string key in faceKeys)
            {
                string uiName = Translation.Get("faceBlendValues", key);
                Slider slider = new Slider(uiName, SliderRange[key]);
                string myKey = key;
                slider.ControlEvent += (s, a) => SetFaceValue(myKey, slider.Value);
                faceControls[key] = slider;
            }

            foreach (string key in faceToggleKeys)
            {
                string uiName = Translation.Get("faceBlendValues", key);
                Toggle toggle = new Toggle(uiName);
                string myKey = key;
                toggle.ControlEvent += (s, a) => SetFaceValue(myKey, toggle.Value);
                faceControls[key] = toggle;
            }
        }

        protected override void ReloadTranslation()
        {
            for (int i = 0; i < faceKeys.Length; i++)
            {
                Slider slider = (Slider)faceControls[faceKeys[i]];
                slider.Label = Translation.Get("faceBlendValues", faceKeys[i]);
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                Toggle toggle = (Toggle)faceControls[faceToggleKeys[i]];
                toggle.Label = Translation.Get("faceBlendValues", faceToggleKeys[i]);
            }
        }

        public override void UpdatePane()
        {
            updating = true;
            Meido meido = meidoManager.ActiveMeido;
            for (int i = 0; i < faceKeys.Length; i++)
            {
                Slider slider = (Slider)faceControls[faceKeys[i]];
                try
                {
                    slider.Value = meido.GetFaceBlendValue(faceKeys[i]);
                }
                catch { }
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                string hash = faceToggleKeys[i];
                Toggle toggle = (Toggle)faceControls[hash];
                toggle.Value = meido.GetFaceBlendValue(hash) > 0f;
                if (hash == "toothoff") toggle.Value = !toggle.Value;
            }
            hasTangOpen = meido.Body.Face.morph.hash["tangopen"] != null;
            updating = false;
        }

        public override void Draw()
        {
            GUI.enabled = meidoManager.HasActiveMeido;
            DrawSliders("eyeclose", "eyeclose2");
            DrawSliders("eyeclose3", "eyebig");
            DrawSliders("eyeclose6", "eyeclose5");
            DrawSliders("hitomih", "hitomis");
            DrawSliders("mayuha", "mayuw");
            DrawSliders("mayuup", "mayuv");
            DrawSliders("mayuvhalf");
            DrawSliders("moutha", "mouths");
            DrawSliders("mouthc", "mouthi");
            DrawSliders("mouthup", "mouthdw");
            DrawSliders("mouthhe", "mouthuphalf");
            DrawSliders("tangout", "tangup");
            if (hasTangOpen) DrawSliders("tangopen");
            MpsGui.WhiteLine();
            DrawToggles("hoho2", "shock", "nosefook");
            DrawToggles("namida", "yodare", "toothoff");
            DrawToggles("tear1", "tear2", "tear3");
            DrawToggles("hohos", "hoho", "hohol");
            GUI.enabled = true;
        }

        private void DrawSliders(params string[] keys)
        {
            GUILayout.BeginHorizontal();
            foreach (string key in keys)
            {
                ((Slider)faceControls[key]).Draw(MpsGui.HalfSlider);
            }
            GUILayout.EndHorizontal();
        }

        private void DrawToggles(params string[] keys)
        {
            GUILayout.BeginHorizontal();
            foreach (string key in keys)
            {
                ((Toggle)faceControls[key]).Draw();
            }
            GUILayout.EndHorizontal();
        }

        private void SetFaceValue(string key, float value)
        {
            if (updating) return;
            meidoManager.ActiveMeido.SetFaceBlendValue(key, value);
        }

        private void SetFaceValue(string key, bool value)
        {
            float max = (key == "hoho" || key == "hoho2") ? 0.5f : 1f;
            if (key == "toothoff") value = !value;
            SetFaceValue(key, value ? max : 0f);
        }
    }
}
