using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DepthOfFieldPane : EffectPane<DepthOfFieldController>
{
    private readonly Slider focalLengthSlider;
    private readonly Slider focalSizeSlider;
    private readonly Slider apertureSlider;
    private readonly Slider blurSizeSlider;
    private readonly Toggle visualizeFocusToggle;

    public DepthOfFieldPane(DepthOfFieldController effectController)
        : base(effectController)
    {
        focalLengthSlider = new(
            Translation.Get("effectDof", "focalLength"), 0f, 10f, Effect.FocalLength, Effect.FocalLength)
        {
            HasTextField = true,
            HasReset = true,
        };

        focalLengthSlider.ControlEvent += OnFocalLengthSliderChanged;

        focalSizeSlider = new(Translation.Get("effectDof", "focalArea"), 0f, 2f, Effect.FocalSize, Effect.FocalSize)
        {
            HasTextField = true,
            HasReset = true,
        };

        focalSizeSlider.ControlEvent += OnFocalSizeSliderChanged;

        apertureSlider = new(Translation.Get("effectDof", "aperture"), 0f, 60f, Effect.Aperture, Effect.Aperture)
        {
            HasTextField = true,
            HasReset = true,
        };

        apertureSlider.ControlEvent += OnApertureSliderChanged;

        blurSizeSlider = new(Translation.Get("effectDof", "blur"), 0f, 10f, Effect.MaxBlurSize, Effect.MaxBlurSize)
        {
            HasTextField = true,
            HasReset = true,
        };

        blurSizeSlider.ControlEvent += OnBlurSizeSliderChanged;

        visualizeFocusToggle = new(Translation.Get("effectDof", "visualizeFocus"), Effect.VisualizeFocus);
        visualizeFocusToggle.ControlEvent += OnVisualizeFocusToggleChanged;
    }

    public override void Draw()
    {
        base.Draw();

        visualizeFocusToggle.Draw();
        focalLengthSlider.Draw();
        focalSizeSlider.Draw();
        apertureSlider.Draw();
        blurSizeSlider.Draw();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        focalLengthSlider.Label = Translation.Get("effectDof", "focalLength");
        focalSizeSlider.Label = Translation.Get("effectDof", "focalArea");
        apertureSlider.Label = Translation.Get("effectDof", "aperture");
        blurSizeSlider.Label = Translation.Get("effectDof", "blur");
        visualizeFocusToggle.Label = Translation.Get("effectDof", "visualizeFocus");
    }

    protected override void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnEffectPropertyChanged(sender, e);

        var depthOfField = (DepthOfFieldController)sender;

        if (e.PropertyName is nameof(DepthOfFieldController.FocalLength))
            focalLengthSlider.SetValueWithoutNotify(depthOfField.FocalLength);
        else if (e.PropertyName is nameof(DepthOfFieldController.FocalSize))
            focalSizeSlider.SetValueWithoutNotify(depthOfField.FocalSize);
        else if (e.PropertyName is nameof(DepthOfFieldController.Aperture))
            apertureSlider.SetValueWithoutNotify(depthOfField.Aperture);
        else if (e.PropertyName is nameof(DepthOfFieldController.MaxBlurSize))
            blurSizeSlider.SetValueWithoutNotify(depthOfField.MaxBlurSize);
        else if (e.PropertyName is nameof(DepthOfFieldController.VisualizeFocus))
            visualizeFocusToggle.SetEnabledWithoutNotify(depthOfField.VisualizeFocus);
    }

    private void OnFocalLengthSliderChanged(object sender, EventArgs e) =>
        Effect.FocalLength = ((Slider)sender).Value;

    private void OnFocalSizeSliderChanged(object sender, EventArgs e) =>
        Effect.FocalSize = ((Slider)sender).Value;

    private void OnApertureSliderChanged(object sender, EventArgs e) =>
        Effect.Aperture = ((Slider)sender).Value;

    private void OnBlurSizeSliderChanged(object sender, EventArgs e) =>
        Effect.MaxBlurSize = ((Slider)sender).Value;

    private void OnVisualizeFocusToggleChanged(object sender, EventArgs e) =>
        Effect.VisualizeFocus = ((Toggle)sender).Value;
}
