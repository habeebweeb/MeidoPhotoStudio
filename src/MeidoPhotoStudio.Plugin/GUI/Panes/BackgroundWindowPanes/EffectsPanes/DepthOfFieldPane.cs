using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DepthOfFieldPane : EffectPane<DepthOfFieldEffectManager>
{
    private readonly Slider focalLengthSlider;
    private readonly Slider focalSizeSlider;
    private readonly Slider apertureSlider;
    private readonly Slider blurSlider;
    private readonly Toggle thicknessToggle;

    public DepthOfFieldPane(EffectManager effectManager)
        : base(effectManager)
    {
        focalLengthSlider =
            new(Translation.Get("effectDof", "focalLength"), 0f, 10f, EffectManager.FocalLength);

        focalLengthSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.FocalLength = focalLengthSlider.Value;
        };

        focalSizeSlider = new(Translation.Get("effectDof", "focalArea"), 0f, 2f, EffectManager.FocalSize);
        focalSizeSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.FocalSize = focalSizeSlider.Value;
        };

        apertureSlider = new(Translation.Get("effectDof", "aperture"), 0f, 60f, EffectManager.Aperture);
        apertureSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.Aperture = apertureSlider.Value;
        };

        blurSlider = new(Translation.Get("effectDof", "blur"), 0f, 10f, EffectManager.MaxBlurSize);
        blurSlider.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.MaxBlurSize = blurSlider.Value;
        };

        thicknessToggle = new(Translation.Get("effectDof", "thicknessToggle"), EffectManager.VisualizeFocus);
        thicknessToggle.ControlEvent += (_, _) =>
        {
            if (updating)
                return;

            EffectManager.VisualizeFocus = thicknessToggle.Value;
        };
    }

    protected override DepthOfFieldEffectManager EffectManager { get; set; }

    protected override void TranslatePane()
    {
        focalLengthSlider.Label = Translation.Get("effectDof", "focalLength");
        focalSizeSlider.Label = Translation.Get("effectDof", "focalArea");
        apertureSlider.Label = Translation.Get("effectDof", "aperture");
        blurSlider.Label = Translation.Get("effectDof", "blur");
        thicknessToggle.Label = Translation.Get("effectDof", "thicknessToggle");
    }

    protected override void UpdateControls()
    {
        focalLengthSlider.Value = EffectManager.FocalLength;
        focalSizeSlider.Value = EffectManager.FocalSize;
        apertureSlider.Value = EffectManager.Aperture;
        blurSlider.Value = EffectManager.MaxBlurSize;
        thicknessToggle.Value = EffectManager.VisualizeFocus;
    }

    protected override void DrawPane()
    {
        focalLengthSlider.Draw();

        var sliderWidth = MpsGui.HalfSlider;

        GUILayout.BeginHorizontal();
        focalSizeSlider.Draw(sliderWidth);
        apertureSlider.Draw(sliderWidth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        blurSlider.Draw(sliderWidth);
        GUILayout.FlexibleSpace();
        thicknessToggle.Draw();
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }
}
