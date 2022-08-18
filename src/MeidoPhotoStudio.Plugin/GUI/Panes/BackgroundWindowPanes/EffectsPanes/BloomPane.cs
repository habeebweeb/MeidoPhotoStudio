using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BloomPane : EffectPane<BloomEffectManager>
{
    private readonly Slider intensitySlider;
    private readonly Slider blurSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Toggle hdrToggle;

    public BloomPane(EffectManager effectManager)
        : base(effectManager)
    {
        intensitySlider = new(Translation.Get("effectBloom", "intensity"), 0f, 100f, EffectManager.BloomValue);
        intensitySlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BloomValue = intensitySlider.Value;
        };

        blurSlider = new(Translation.Get("effectBloom", "blur"), 0f, 15f, EffectManager.BlurIterations);
        blurSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BlurIterations = (int)blurSlider.Value;
        };

        redSlider = new(Translation.Get("backgroundWindow", "red"), 1f, 0.5f, EffectManager.BloomThresholdColorRed);
        redSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BloomThresholdColorRed = redSlider.Value;
        };

        greenSlider =
            new(Translation.Get("backgroundWindow", "green"), 1f, 0.5f, EffectManager.BloomThresholdColorGreen);

        greenSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BloomThresholdColorGreen = greenSlider.Value;
        };

        blueSlider = new(Translation.Get("backgroundWindow", "blue"), 1f, 0.5f, EffectManager.BloomThresholdColorBlue);
        blueSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BloomThresholdColorBlue = blueSlider.Value;
        };

        hdrToggle = new(Translation.Get("effectBloom", "hdrToggle"), EffectManager.BloomHDR);
        hdrToggle.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.BloomHDR = hdrToggle.Value;
        };
    }

    protected override BloomEffectManager EffectManager { get; set; }

    protected override void TranslatePane()
    {
        intensitySlider.Label = Translation.Get("effectBloom", "intensity");
        blurSlider.Label = Translation.Get("effectBloom", "blur");
        redSlider.Label = Translation.Get("backgroundWindow", "red");
        greenSlider.Label = Translation.Get("backgroundWindow", "green");
        blueSlider.Label = Translation.Get("backgroundWindow", "blue");
        hdrToggle.Label = Translation.Get("effectBloom", "hdrToggle");
    }

    protected override void UpdateControls()
    {
        intensitySlider.Value = EffectManager.BloomValue;
        blurSlider.Value = EffectManager.BlurIterations;
        redSlider.Value = EffectManager.BloomThresholdColorRed;
        greenSlider.Value = EffectManager.BloomThresholdColorGreen;
        blueSlider.Value = EffectManager.BloomThresholdColorBlue;
        hdrToggle.Value = EffectManager.BloomHDR;
    }

    protected override void DrawPane()
    {
        var sliderWidth = MpsGui.HalfSlider;

        GUILayout.BeginHorizontal();
        intensitySlider.Draw(sliderWidth);
        blurSlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        redSlider.Draw(sliderWidth);
        greenSlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        blueSlider.Draw(sliderWidth);
        GUILayout.FlexibleSpace();
        hdrToggle.Draw(GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
    }
}
