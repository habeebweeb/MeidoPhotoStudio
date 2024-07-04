using MeidoPhotoStudio.Plugin.Core.Effects;

namespace MeidoPhotoStudio.Plugin;

public class BlurPane : EffectPane<BlurController>
{
    private readonly Slider blurSizeSlider;
    private readonly Slider blurIterationsSlider;
    private readonly Slider downsampleSlider;

    public BlurPane(BlurController effectController)
        : base(effectController)
    {
        blurSizeSlider = new("Blur Size", 0f, 20f, Effect.BlurSize)
        {
            HasTextField = true,
        };

        blurSizeSlider.ControlEvent += OnBlurSizeChanged;

        blurIterationsSlider = new("Blur Iterations", 0f, 20f, Effect.BlurIterations)
        {
            HasTextField = true,
        };

        blurIterationsSlider.ControlEvent += OnBlurIterationsChanged;

        downsampleSlider = new("Downsample", 0f, 10f, Effect.Downsample)
        {
            HasTextField = true,
        };

        downsampleSlider.ControlEvent += OnDownsampleChanged;
    }

    public override void Draw()
    {
        base.Draw();

        blurSizeSlider.Draw();
        blurIterationsSlider.Draw();
        downsampleSlider.Draw();

        GUI.enabled = true;
    }

    private void OnBlurSizeChanged(object sender, EventArgs e) =>
        Effect.BlurSize = ((Slider)sender).Value;

    private void OnBlurIterationsChanged(object sender, EventArgs e) =>
        Effect.BlurIterations = (int)((Slider)sender).Value;

    private void OnDownsampleChanged(object sender, EventArgs e) =>
        Effect.Downsample = (int)((Slider)sender).Value;
}
