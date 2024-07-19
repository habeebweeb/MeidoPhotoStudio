using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;

namespace MeidoPhotoStudio.Plugin;

public class FogPane : EffectPane<FogController>
{
    private readonly Slider distanceSlider;
    private readonly Slider densitySlider;
    private readonly Slider heightScaleSlider;
    private readonly Slider heightSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;

    public FogPane(FogController effectController)
        : base(effectController)
    {
        distanceSlider = new(Translation.Get("effectFog", "distance"), 0f, 30f, Effect.Distance, Effect.Distance)
        {
            HasTextField = true,
            HasReset = true,
        };

        distanceSlider.ControlEvent += OnDistanceSliderChanged;

        densitySlider = new(Translation.Get("effectFog", "density"), 0f, 10f, Effect.Density, Effect.Density)
        {
            HasTextField = true,
            HasReset = true,
        };

        densitySlider.ControlEvent += OnDensitySliderChanged;

        heightScaleSlider = new(
            Translation.Get("effectFog", "heightScale"), -5f, 20f, Effect.HeightScale, Effect.HeightScale)
        {
            HasTextField = true,
            HasReset = true,
        };

        heightScaleSlider.ControlEvent += OnHeightScaleSliderChanged;

        heightSlider = new(Translation.Get("effectFog", "height"), -10f, 10f, Effect.Height, Effect.Height)
        {
            HasTextField = true,
            HasReset = true,
        };

        heightSlider.ControlEvent += OnHeightSliderChanged;

        redSlider = new(Translation.Get("effectFog", "red"), 0f, 1f, Effect.FogColour.r, Effect.FogColour.r)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new(Translation.Get("effectFog", "green"), 0f, 1f, Effect.FogColour.g, Effect.FogColour.g)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new(Translation.Get("effectFog", "blue"), 0f, 1f, Effect.FogColour.b, Effect.FogColour.b)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;
    }

    public override void Draw()
    {
        base.Draw();

        distanceSlider.Draw();
        densitySlider.Draw();
        heightScaleSlider.Draw();
        heightSlider.Draw();
        redSlider.Draw();
        greenSlider.Draw();
        blueSlider.Draw();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        distanceSlider.Label = Translation.Get("effectFog", "distance");
        densitySlider.Label = Translation.Get("effectFog", "density");
        heightScaleSlider.Label = Translation.Get("effectFog", "heightScale");
        heightSlider.Label = Translation.Get("effectFog", "height");
        redSlider.Label = Translation.Get("effectFog", "red");
        greenSlider.Label = Translation.Get("effectFog", "green");
        blueSlider.Label = Translation.Get("effectFog", "blue");
    }

    protected override void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        base.OnEffectPropertyChanged(sender, e);

        var fog = (FogController)sender;

        if (e.PropertyName is nameof(FogController.Distance))
        {
            distanceSlider.SetValueWithoutNotify(fog.Distance);
        }
        else if (e.PropertyName is nameof(FogController.Density))
        {
            densitySlider.SetValueWithoutNotify(fog.Density);
        }
        else if (e.PropertyName is nameof(FogController.HeightScale))
        {
            heightScaleSlider.SetValueWithoutNotify(fog.HeightScale);
        }
        else if (e.PropertyName is nameof(FogController.Height))
        {
            heightSlider.SetValueWithoutNotify(fog.Height);
        }
        else if (e.PropertyName is nameof(FogController.FogColour))
        {
            redSlider.SetValueWithoutNotify(fog.FogColour.r);
            greenSlider.SetValueWithoutNotify(fog.FogColour.g);
            blueSlider.SetValueWithoutNotify(fog.FogColour.b);
        }
    }

    private void OnDistanceSliderChanged(object sender, EventArgs e) =>
        Effect.Distance = ((Slider)sender).Value;

    private void OnDensitySliderChanged(object sender, EventArgs e) =>
        Effect.Density = ((Slider)sender).Value;

    private void OnHeightScaleSliderChanged(object sender, EventArgs e) =>
        Effect.HeightScale = ((Slider)sender).Value;

    private void OnHeightSliderChanged(object sender, EventArgs e) =>
        Effect.Height = ((Slider)sender).Value;

    private void OnColourSliderChanged(object sender, EventArgs e) =>
        Effect.FogColour = new(redSlider.Value, blueSlider.Value, greenSlider.Value);
}
