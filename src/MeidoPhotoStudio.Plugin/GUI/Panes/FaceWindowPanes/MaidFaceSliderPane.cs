using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using UnityEngine;

using static MeidoPhotoStudio.Plugin.Meido;

namespace MeidoPhotoStudio.Plugin;

public class MaidFaceSliderPane : BasePane
{
    private static readonly Dictionary<string, float> SliderLimits = new()
    {
        ["eyeclose"] = 1f, // Eye Shut
        ["eyeclose2"] = 1f, // Eye Smile
        ["eyeclose3"] = 1f, // Glare
        ["eyebig"] = 1f, // Wide Eyes
        ["eyeclose6"] = 1f, // Wink 1
        ["eyeclose5"] = 1f, // Wink 2
        ["eyeclose7"] = 1f, // Wink R 1
        ["eyeclose8"] = 1f, // Wink R 2
        ["hitomih"] = 2f, // Highlight
        ["hitomis"] = 3f, // Pupil Size
        ["mayuha"] = 1f, // Brow 1
        ["mayuw"] = 1f, // Brow 2
        ["mayuup"] = 1f, // Brow Up
        ["mayuv"] = 1f, // Brow Down 1
        ["mayuvhalf"] = 1f, // Brow Down 2
        ["moutha"] = 1f, // Mouth Open 1
        ["mouths"] = 1f, // Mouth Open 2
        ["mouthc"] = 1f, // Mouth Narrow
        ["mouthi"] = 1f, // Mouth Widen
        ["mouthup"] = 1.4f, // Smile
        ["mouthdw"] = 1f, // Frown
        ["mouthhe"] = 1f, // Mouth Pucker
        ["mouthuphalf"] = 2f, // Grin
        ["tangout"] = 1f, // Tongue Out
        ["tangup"] = 1f, // Tongue Up
        ["tangopen"] = 1f, // Tongue Base
    };

    private readonly MeidoManager meidoManager;
    private readonly Dictionary<string, BaseControl> faceControls;

    private bool hasTangOpen;
    private bool hasEyeClose7and8;

    public MaidFaceSliderPane(MeidoManager meidoManager)
    {
        this.meidoManager = meidoManager;

        faceControls = new();

        foreach (var key in FaceKeys)
        {
            var uiName = Translation.Get("faceBlendValues", key);
            var slider = new Slider(uiName, 0f, SliderLimits[key]);
            var sliderKey = key;

            slider.ControlEvent += (_, _) =>
                SetFaceValue(sliderKey, slider.Value);

            faceControls[key] = slider;
        }

        foreach (var key in FaceToggleKeys)
        {
            var uiName = Translation.Get("faceBlendValues", key);
            var toggle = new Toggle(uiName);
            var sliderKey = key;

            toggle.ControlEvent += (_, _) =>
                SetFaceValue(sliderKey, toggle.Value);

            faceControls[key] = toggle;
        }

        InitializeSliderLimits(faceControls);
    }

    public override void UpdatePane()
    {
        updating = true;
        var meido = meidoManager.ActiveMeido;

        for (var i = 0; i < FaceKeys.Length; i++)
        {
            var slider = (Slider)faceControls[FaceKeys[i]];

            try
            {
                slider.Value = meido.GetFaceBlendValue(FaceKeys[i]);
            }
            catch
            {
                // Ignored
            }
        }

        for (var i = 0; i < FaceToggleKeys.Length; i++)
        {
            var hash = FaceToggleKeys[i];
            var toggle = (Toggle)faceControls[hash];

            toggle.Value = meido.GetFaceBlendValue(hash) > 0f;

            if (hash is "toothoff")
                toggle.Value = !toggle.Value;
        }

        var faceMorph = meido.Body.Face.morph;

        hasTangOpen = faceMorph.Contains("tangopen");

        var eyeclose7Hash = Utility.GP01FbFaceHash(faceMorph, "eyeclose7");
        var eyeclose8Hash = Utility.GP01FbFaceHash(faceMorph, "eyeclose8");

        hasEyeClose7and8 = faceMorph.Contains(eyeclose7Hash) && faceMorph.Contains(eyeclose8Hash);

        updating = false;
    }

    public override void Draw()
    {
        GUI.enabled = meidoManager.HasActiveMeido;
        DrawSliders("eyeclose", "eyeclose2");
        DrawSliders("eyeclose3", "eyebig");

        DrawSliders("eyeclose6", "eyeclose5");

        if (hasEyeClose7and8)
            DrawSliders("eyeclose8", "eyeclose7");

        DrawSliders("hitomih", "hitomis");
        DrawSliders("mayuha", "mayuw");
        DrawSliders("mayuup", "mayuv");
        DrawSliders("mayuvhalf");
        DrawSliders("moutha", "mouths");
        DrawSliders("mouthc", "mouthi");
        DrawSliders("mouthup", "mouthdw");
        DrawSliders("mouthhe", "mouthuphalf");
        DrawSliders("tangout", "tangup");

        if (hasTangOpen)
            DrawSliders("tangopen");

        MpsGui.WhiteLine();
        DrawToggles("hoho2", "shock", "nosefook");
        DrawToggles("namida", "yodare", "toothoff");
        DrawToggles("tear1", "tear2", "tear3");
        DrawToggles("hohos", "hoho", "hohol");
        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        for (var i = 0; i < FaceKeys.Length; i++)
        {
            var slider = (Slider)faceControls[FaceKeys[i]];

            slider.Label = Translation.Get("faceBlendValues", FaceKeys[i]);
        }

        for (var i = 0; i < FaceToggleKeys.Length; i++)
        {
            var toggle = (Toggle)faceControls[FaceToggleKeys[i]];

            toggle.Label = Translation.Get("faceBlendValues", FaceToggleKeys[i]);
        }
    }

    private static void InitializeSliderLimits(Dictionary<string, BaseControl> controls)
    {
        try
        {
            var sliderLimitsPath = Path.Combine(Constants.DatabasePath, "face_slider_limits.json");
            var sliderLimitsJson = File.ReadAllText(sliderLimitsPath);

            foreach (var kvp in JsonConvert.DeserializeObject<Dictionary<string, float>>(sliderLimitsJson))
            {
                var key = kvp.Key;

                if (FaceKeys.Contains(key) && controls.ContainsKey(key))
                {
                    var limit = kvp.Value;

                    limit = kvp.Value >= 1f ? limit : SliderLimits[key];

                    var slider = (Slider)controls[kvp.Key];

                    slider.SetBounds(slider.Left, limit);
                }
                else
                {
                    Utility.LogWarning($"'{key}' is not a valid face key");
                }
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

    private void DrawSliders(params string[] keys)
    {
        GUILayout.BeginHorizontal();

        foreach (var key in keys)
            faceControls[key].Draw(MpsGui.HalfSlider);

        GUILayout.EndHorizontal();
    }

    private void DrawToggles(params string[] keys)
    {
        GUILayout.BeginHorizontal();

        foreach (var key in keys)
            faceControls[key].Draw();

        GUILayout.EndHorizontal();
    }

    private void SetFaceValue(string key, float value)
    {
        if (updating)
            return;

        meidoManager.ActiveMeido.SetFaceBlendValue(key, value);
    }

    private void SetFaceValue(string key, bool value)
    {
        if (key is "toothoff")
            value = !value;

        SetFaceValue(key, value ? 1f : 0f);
    }
}
