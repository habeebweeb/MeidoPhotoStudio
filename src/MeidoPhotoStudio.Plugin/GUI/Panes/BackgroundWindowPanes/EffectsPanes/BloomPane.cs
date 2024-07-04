using MeidoPhotoStudio.Plugin.Core.Effects;

namespace MeidoPhotoStudio.Plugin;

public class BloomPane : EffectPane<BloomController>
{
    private readonly Slider intensitySlider;
    private readonly Slider blurSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Toggle hdrToggle;

    public BloomPane(BloomController effectController)
        : base(effectController)
    {
        intensitySlider = new(
            Translation.Get("effectBloom", "intensity"), 0f, 100f, Effect.BloomValue, Effect.BloomValue)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnItensitySliderChanged;

        blurSlider = new(Translation.Get("effectBloom", "blur"), 0f, 15f, Effect.BlurIterations, Effect.BlurIterations)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSlider.ControlEvent += OnBlurSliderChanged;

        var bloomThresholdColour = Effect.BloomThresholdColour;

        redSlider = new(Translation.Get("effectBloom", "red"), 1f, 0f, bloomThresholdColour.r, bloomThresholdColour.r)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnRedSliderChanged;

        greenSlider = new(
            Translation.Get("effectBloom", "green"), 1f, 0f, bloomThresholdColour.g, bloomThresholdColour.g)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnGreenSliderChanged;

        blueSlider = new(
            Translation.Get("effectBloom", "blue"), 1f, 0f, bloomThresholdColour.b, bloomThresholdColour.b)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnBlueSliderChanged;

        hdrToggle = new(Translation.Get("effectBloom", "hdrToggle"), Effect.HDR);
        hdrToggle.ControlEvent += OnHDRToggleChanged;
    }

    public override void Draw()
    {
        base.Draw();

        intensitySlider.Draw();
        blurSlider.Draw();
        redSlider.Draw();
        greenSlider.Draw();
        blueSlider.Draw();
        hdrToggle.Draw();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        intensitySlider.Label = Translation.Get("effectBloom", "intensity");
        blurSlider.Label = Translation.Get("effectBloom", "blur");
        redSlider.Label = Translation.Get("backgroundWindow", "red");
        greenSlider.Label = Translation.Get("backgroundWindow", "green");
        blueSlider.Label = Translation.Get("backgroundWindow", "blue");
        hdrToggle.Label = Translation.Get("effectBloom", "hdrToggle");
    }

    private void OnItensitySliderChanged(object sender, EventArgs e) =>
        Effect.BloomValue = (int)((Slider)sender).Value;

    private void OnBlurSliderChanged(object sender, EventArgs e) =>
        Effect.BlurIterations = (int)((Slider)sender).Value;

    private void OnRedSliderChanged(object sender, EventArgs e) =>
        Effect.BloomThresholdColour = Effect.BloomThresholdColour with { r = ((Slider)sender).Value };

    private void OnGreenSliderChanged(object sender, EventArgs e) =>
        Effect.BloomThresholdColour = Effect.BloomThresholdColour with { g = ((Slider)sender).Value };

    private void OnBlueSliderChanged(object sender, EventArgs e) =>
        Effect.BloomThresholdColour = Effect.BloomThresholdColour with { b = ((Slider)sender).Value };

    private void OnHDRToggleChanged(object sender, EventArgs e) =>
        Effect.HDR = ((Toggle)sender).Value;
}
