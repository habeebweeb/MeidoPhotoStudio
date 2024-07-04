using MeidoPhotoStudio.Plugin.Core.Effects;

namespace MeidoPhotoStudio.Plugin;

public class VignettePane : EffectPane<VignetteController>
{
    private readonly Slider intensitySlider;
    private readonly Slider blurSlider;
    private readonly Slider blurSpreadSlider;
    private readonly Slider chromaticAberrationSlider;

    public VignettePane(VignetteController effectController)
        : base(effectController)
    {
        intensitySlider = new Slider(
            Translation.Get("effectVignette", "intensity"), -40f, 70f, Effect.Intensity, Effect.Intensity)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnItensitySliderChanged;

        blurSlider = new Slider(Translation.Get("effectVignette", "blur"), 0f, 5f, Effect.Blur, Effect.Blur)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSlider.ControlEvent += OnBlurSliderChanged;

        blurSpreadSlider = new Slider(
            Translation.Get("effectVignette", "blurSpread"), 0, 40f, Effect.BlurSpread, Effect.BlurSpread)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSpreadSlider.ControlEvent += OnBlurSpreadSliderChanged;

        chromaticAberrationSlider = new Slider(
            Translation.Get("effectVignette", "chromaticAberration"),
            -50f,
            50f,
            Effect.ChromaticAberration,
            Effect.ChromaticAberration)
        {
            HasTextField = true,
            HasReset = true,
        };

        chromaticAberrationSlider.ControlEvent += OnAberrationSliderChanged;
    }

    public override void Draw()
    {
        base.Draw();

        intensitySlider.Draw();
        blurSlider.Draw();
        blurSpreadSlider.Draw();
        chromaticAberrationSlider.Draw();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        intensitySlider.Label = Translation.Get("effectVignette", "intensity");
        blurSlider.Label = Translation.Get("effectVignette", "blur");
        blurSpreadSlider.Label = Translation.Get("effectVignette", "blurSpread");
        chromaticAberrationSlider.Label = Translation.Get("effectVignette", "chromaticAberration");
    }

    private void OnItensitySliderChanged(object sender, EventArgs e) =>
        Effect.Intensity = ((Slider)sender).Value;

    private void OnBlurSliderChanged(object sender, EventArgs e) =>
        Effect.Blur = ((Slider)sender).Value;

    private void OnBlurSpreadSliderChanged(object sender, EventArgs e) =>
        Effect.BlurSpread = ((Slider)sender).Value;

    private void OnAberrationSliderChanged(object sender, EventArgs e) =>
        Effect.ChromaticAberration = ((Slider)sender).Value;
}
