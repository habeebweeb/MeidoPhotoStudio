using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static Meido;
    public class MaidFaceSliderPane : BasePane
    {
        private static readonly Dictionary<string, float> SliderLimits = new Dictionary<string, float>()
        {
            // Eye Shut
            ["eyeclose"] = 1f,
            // Eye Smile
            ["eyeclose2"] = 1f,
            // Glare
            ["eyeclose3"] = 1f,
            // Wide Eyes
            ["eyebig"] = 1f,
            // Wink 1
            ["eyeclose6"] = 1f,
            // Wink 2
            ["eyeclose5"] = 1f,
            // Highlight
            ["hitomih"] = 2f,
            // Pupil Size
            ["hitomis"] = 3f,
            // Brow 1
            ["mayuha"] = 1f,
            // Brow 2
            ["mayuw"] = 1f,
            // Brow Up
            ["mayuup"] = 1f,
            // Brow Down 1
            ["mayuv"] = 1f,
            // Brow Down 2
            ["mayuvhalf"] = 1f,
            // Mouth Open 1
            ["moutha"] = 1f,
            // Mouth Open 2
            ["mouths"] = 1f,
            // Mouth Narrow
            ["mouthc"] = 1f,
            // Mouth Widen
            ["mouthi"] = 1f,
            // Smile
            ["mouthup"] = 1.4f,
            // Frown
            ["mouthdw"] = 1f,
            // Mouth Pucker
            ["mouthhe"] = 1f,
            // Grin
            ["mouthuphalf"] = 2f,
            // Tongue Out
            ["tangout"] = 1f,
            // Tongue Up
            ["tangup"] = 1f,
            // Tongue Base
            ["tangopen"] = 1f
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
                Slider slider = new Slider(uiName, 0f, SliderLimits[key]);
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

            InitializeSliderLimits(faceControls);
        }

        private static void InitializeSliderLimits(Dictionary<string, BaseControl> controls)
        {
            try
            {
                string sliderLimitsPath = Path.Combine(Constants.databasePath, "face_slider_limits.json");
                string sliderLimitsJson = File.ReadAllText(sliderLimitsPath);

                foreach (var kvp in JsonConvert.DeserializeObject<Dictionary<string, float>>(sliderLimitsJson))
                {
                    string key = kvp.Key;
                    if (faceKeys.Contains(key) && controls.ContainsKey(key))
                    {
                        float limit = kvp.Value;
                        limit = kvp.Value >= 1f ? limit : SliderLimits[key];
                        Slider slider = (Slider)controls[kvp.Key];
                        slider.SetBounds(slider.Left, limit);
                    }
                    else Utility.LogWarning($"'{key}' is not a valid face key");
                }
            }
            catch (IOException e)
            {
                Utility.LogWarning($"Could not open face slider limit database because {e.Message}");
            }
            catch (Exception e)
            {
                Utility.LogError($"Could not apply face slider limit database because {e.Message}");
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
            hasTangOpen = meido.Body.Face.morph.Contains("tangopen");
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
            foreach (string key in keys) faceControls[key].Draw(MpsGui.HalfSlider);
            GUILayout.EndHorizontal();
        }

        private void DrawToggles(params string[] keys)
        {
            GUILayout.BeginHorizontal();
            foreach (string key in keys) faceControls[key].Draw();
            GUILayout.EndHorizontal();
        }

        private void SetFaceValue(string key, float value)
        {
            if (updating) return;
            meidoManager.ActiveMeido.SetFaceBlendValue(key, value);
        }

        private void SetFaceValue(string key, bool value)
        {
            if (key == "toothoff") value = !value;
            SetFaceValue(key, value ? 1f : 0f);
        }
    }
}
