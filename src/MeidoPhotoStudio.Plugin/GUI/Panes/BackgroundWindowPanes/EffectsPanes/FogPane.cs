using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class FogPane : EffectPane<FogEffectManager>
{
    private readonly Slider distanceSlider;
    private readonly Slider densitySlider;
    private readonly Slider heightScaleSlider;
    private readonly Slider heightSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;

    public FogPane(EffectManager effectManager)
        : base(effectManager)
    {
        distanceSlider = new(Translation.Get("effectFog", "distance"), 0f, 30f, EffectManager.Distance);
        distanceSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.Distance = distanceSlider.Value;
        };

        densitySlider = new(Translation.Get("effectFog", "density"), 0f, 10f, EffectManager.Density);
        densitySlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.Density = densitySlider.Value;
        };

        heightScaleSlider = new(Translation.Get("effectFog", "strength"), -5f, 20f, EffectManager.HeightScale);
        heightScaleSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.HeightScale = heightScaleSlider.Value;
        };

        heightSlider = new(Translation.Get("effectFog", "height"), -10f, 10f, EffectManager.Height);
        heightSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.Height = heightSlider.Value;
        };

        var initialFogColour = EffectManager.FogColour;

        redSlider = new(Translation.Get("backgroundWIndow", "red"), 0f, 1f, initialFogColour.r);
        redSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.FogColourRed = redSlider.Value;
        };

        greenSlider = new(Translation.Get("backgroundWIndow", "green"), 0f, 1f, initialFogColour.g);
        greenSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.FogColourGreen = greenSlider.Value;
        };

        blueSlider = new(Translation.Get("backgroundWIndow", "blue"), 0f, 1f, initialFogColour.b);
        blueSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.FogColourBlue = blueSlider.Value;
        };
    }

    protected override FogEffectManager EffectManager { get; set; }

    protected override void TranslatePane()
    {
        distanceSlider.Label = Translation.Get("effectFog", "distance");
        densitySlider.Label = Translation.Get("effectFog", "density");
        heightScaleSlider.Label = Translation.Get("effectFog", "strength");
        heightSlider.Label = Translation.Get("effectFog", "height");
        redSlider.Label = Translation.Get("backgroundWIndow", "red");
        greenSlider.Label = Translation.Get("backgroundWIndow", "green");
        blueSlider.Label = Translation.Get("backgroundWIndow", "blue");
    }

    protected override void UpdateControls()
    {
        distanceSlider.Value = EffectManager.Distance;
        densitySlider.Value = EffectManager.Density;
        heightScaleSlider.Value = EffectManager.HeightScale;
        heightSlider.Value = EffectManager.Height;
        redSlider.Value = EffectManager.FogColourRed;
        greenSlider.Value = EffectManager.FogColourGreen;
        blueSlider.Value = EffectManager.FogColourBlue;
    }

    protected override void DrawPane()
    {
        var sliderWidth = MpsGui.HalfSlider;

        GUILayout.BeginHorizontal();
        distanceSlider.Draw(sliderWidth);
        densitySlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        heightScaleSlider.Draw(sliderWidth);
        heightSlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        redSlider.Draw(sliderWidth);
        greenSlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        blueSlider.Draw(sliderWidth);
    }
}
