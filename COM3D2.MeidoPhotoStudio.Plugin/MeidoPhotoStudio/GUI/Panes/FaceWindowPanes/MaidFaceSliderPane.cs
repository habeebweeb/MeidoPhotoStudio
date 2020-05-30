using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public class MaidFaceSliderPane : BasePane
    {
        private MeidoManager meidoManager;

        // TODO: Consider placing in external file to be user editable
        private static readonly Dictionary<string, float[]> SliderRange = new Dictionary<string, float[]>()
        {
            // Eye Shut
            ["eyeclose"] = new[] { 0f, 1f },
            // Eye Smile
            ["eyeclose2"] = new[] { 0f, 1f },
            // Glare
            ["eyeclose3"] = new[] { 0f, 1f },
            // Wide Eyes
            ["eyebig"] = new[] { 0f, 1f },
            // Wink 1
            ["eyeclose6"] = new[] { 0f, 1f },
            // Wink 2
            ["eyeclose5"] = new[] { 0f, 1f },
            // Highlight
            ["hitomih"] = new[] { 0f, 2f },
            // Pupil Size
            ["hitomis"] = new[] { 0f, 3f },
            // Brow 1
            ["mayuha"] = new[] { 0f, 1f },
            // Brow 2
            ["mayuw"] = new[] { 0f, 1f },
            // Brow Up
            ["mayuup"] = new[] { 0f, 0.8f },
            // Brow Down 1
            ["mayuv"] = new[] { 0f, 0.8f },
            // Brow Down 2
            ["mayuvhalf"] = new[] { 0f, 0.9f },
            // Mouth Open 1
            ["moutha"] = new[] { 0f, 1f },
            // Mouth Open 2
            ["mouths"] = new[] { 0f, 0.9f },
            // Mouth Narrow
            ["mouthc"] = new[] { 0f, 1f },
            // Mouth Widen
            ["mouthi"] = new[] { 0f, 1f },
            // Smile
            ["mouthup"] = new[] { 0f, 1.4f },
            // Frown
            ["mouthdw"] = new[] { 0f, 1f },
            // Mouth Pucker
            ["mouthhe"] = new[] { 0f, 1f },
            // Grin
            ["mouthuphalf"] = new[] { 0f, 2f },
            // Tongue Out
            ["tangout"] = new[] { 0f, 1f },
            // Tongue Up
            ["tangup"] = new[] { 0f, 0.7f },
            // Tongue Base
            ["tangopen"] = new[] { 0f, 1f }
        };

        public static readonly string[] faceKeys = new string[24]
        {
            "eyeclose", "eyeclose2", "eyeclose3", "eyebig", "eyeclose6", "eyeclose5", "hitomih",
            "hitomis", "mayuha", "mayuw", "mayuup", "mayuv", "mayuvhalf", "moutha", "mouths",
            "mouthc", "mouthi", "mouthup", "mouthdw", "mouthhe", "mouthuphalf", "tangout",
            "tangup", "tangopen"
        };

        public static readonly string[] faceToggleKeys = new string[12]
        {
            "hoho2", "shock", "nosefook", "namida", "yodare", "toothoff",
            "tear1", "tear2", "tear3", "hohos", "hoho", "hohol"
        };

        public MaidFaceSliderPane(MeidoManager meidoManager)
        {
            this.meidoManager = meidoManager;

            for (int i = 0; i < faceKeys.Length; i++)
            {
                string key = faceKeys[i];
                string uiName = Translation.Get("faceBlendValues", key);
                Slider slider = new Slider(uiName, SliderRange[key][0], SliderRange[key][1]);
                int myIndex = i;
                slider.ControlEvent += (s, a) => this.SetFaceValue(faceKeys[myIndex], slider.Value);
                this.Controls.Add(slider);
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                string uiName = Translation.Get("faceBlendValues", faceToggleKeys[i]);
                Toggle toggle = new Toggle(uiName);
                int myIndex = i;
                toggle.ControlEvent += (s, a) => this.SetFaceValue(faceToggleKeys[myIndex], toggle.Value);
                this.Controls.Add(toggle);
            }
        }

        public void SetFaceValue(string key, float value)
        {
            if (updating) return;
            this.meidoManager.ActiveMeido.SetFaceBlendValue(key, value);
        }

        public void SetFaceValue(string key, bool value)
        {
            float max = (key == "hoho" || key == "hoho2") ? 0.5f : 1f;
            if (key == "toothoff") value = !value;
            SetFaceValue(key, value ? max : 0f);
        }

        public override void Update()
        {
            this.updating = true;
            TMorph morph = this.meidoManager.ActiveMeido.Maid.body0.Face.morph;
            float[] blendValues = Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValues");
            float[] blendValuesBackup = Utility.GetFieldValue<TMorph, float[]>(morph, "BlendValuesBackup");
            for (int i = 0; i < faceKeys.Length; i++)
            {
                string hash = faceKeys[i];
                Slider slider = this.Controls[i] as Slider;
                try
                {
                    if (hash.StartsWith("eyeclose"))
                        slider.Value = blendValuesBackup[(int)morph.hash[hash]];
                    else
                        slider.Value = blendValues[(int)morph.hash[hash]];
                }
                catch { }
            }

            for (int i = 0; i < faceToggleKeys.Length; i++)
            {
                string hash = faceToggleKeys[i];
                Toggle toggle = this.Controls[24 + i] as Toggle;
                if (hash == "nosefook") toggle.Value = morph.boNoseFook;
                else toggle.Value = blendValues[(int)morph.hash[hash]] > 0f;
                if (hash == "toothoff") toggle.Value = !toggle.Value;
            }
            this.updating = false;
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            for (int i = 0; i < faceKeys.Length; i += 2)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 2; j++)
                {
                    Controls[i + j].Draw(GUILayout.Width(90));
                    if (i + j == 12 || i + j == 23)
                    {
                        i--;
                        break;
                    }

                }
                GUILayout.EndHorizontal();
            }

            MiscGUI.WhiteLine();

            for (int i = 0; i < faceToggleKeys.Length; i += 3)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < 3; j++)
                {
                    Controls[24 + i + j].Draw();
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
