using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Framework;

namespace MeidoPhotoStudio.Plugin;

public class LightsPane : BasePane
{
    private static Light mainLight;

    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly Dropdown<LightController> lightDropdown;
    private readonly Dictionary<LightController, string> lightNames = [];
    private readonly SelectionGrid lightTypeGrid;
    private readonly Toggle lightOnToggle;
    private readonly Button addLightButton;
    private readonly Button deleteLightButton;
    private readonly Button clearLightsButton;
    private readonly Slider xRotationSlider;
    private readonly Slider yRotationSlider;
    private readonly Slider intensitySlider;
    private readonly Slider shadowStrengthSlider;
    private readonly Slider rangeSlider;
    private readonly Slider spotAngleSlider;
    private readonly Slider redSlider;
    private readonly Slider greenSlider;
    private readonly Slider blueSlider;
    private readonly Button resetPositionButton;
    private readonly Button resetPropertiesButton;
    private readonly PaneHeader paneHeader;
    private readonly Header resetHeader;
    private readonly Label noLightsLabel;

    private string noLights;
    private bool sliderChangedTransform;

    public LightsPane(LightRepository lightRepository, SelectionController<LightController> lightSelectionController)
    {
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));

        lightRepository.AddedLight += OnAddedLight;
        lightRepository.RemovedLight += OnRemovedLight;

        lightSelectionController.Selecting += OnSelectingLight;
        lightSelectionController.Selected += OnSelectedLight;

        paneHeader = new(Translation.Get("lightsPane", "header"), true);
        resetHeader = new(Translation.Get("lightsPane", "resetLabel"));
        noLightsLabel = new(Translation.Get("lightsPane", "noLights"));

        lightDropdown = new(formatter: LightNameFormatter);
        lightDropdown.SelectionChanged += LightDropdownSelectionChanged;

        lightTypeGrid = new SelectionGrid(Translation.GetArray("lightType", new[] { "normal", "spot", "point" }));
        lightTypeGrid.ControlEvent += OnLightTypeChanged;

        addLightButton = new Button(Translation.Get("lightsPane", "add"));
        addLightButton.ControlEvent += OnAddLightButtonPressed;

        deleteLightButton = new Button(Translation.Get("lightsPane", "delete"));
        deleteLightButton.ControlEvent += OnDeleteButtonPressed;

        clearLightsButton = new Button(Translation.Get("lightsPane", "clear"));
        clearLightsButton.ControlEvent += OnClearButtonPressed;

        lightOnToggle = new Toggle(Translation.Get("lightsPane", "on"), true);
        lightOnToggle.ControlEvent += OnLightOnToggleChanged;

        var defaultRotation = LightProperties.DefaultRotation.eulerAngles;

        xRotationSlider = new Slider(Translation.Get("lights", "x"), 0f, 360f, defaultRotation.x, defaultRotation.x)
        {
            HasTextField = true,
            HasReset = true,
        };

        xRotationSlider.ControlEvent += OnRotationSlidersChanged;

        yRotationSlider = new Slider(Translation.Get("lights", "y"), 0f, 360f, defaultRotation.y, defaultRotation.y)
        {
            HasTextField = true,
            HasReset = true,
        };

        yRotationSlider.ControlEvent += OnRotationSlidersChanged;

        intensitySlider = new Slider(Translation.Get("lights", "intensity"), 0f, 2f, 0.95f, 0.95f)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnIntensitySliderChanged;

        shadowStrengthSlider = new Slider(Translation.Get("lights", "shadow"), 0f, 1f, 0.10f, 0.10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        shadowStrengthSlider.ControlEvent += OnShadowStrenthSliderChanged;

        rangeSlider = new Slider(Translation.Get("lights", "range"), 0f, 150f, 10f, 10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        rangeSlider.ControlEvent += OnRangeSliderChanged;

        spotAngleSlider = new Slider(Translation.Get("lights", "spot"), 0f, 150f, 50f, 50f)
        {
            HasTextField = true,
            HasReset = true,
        };

        spotAngleSlider.ControlEvent += OnSpotAngleSliderChanged;

        redSlider = new Slider(Translation.Get("lights", "red"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new Slider(Translation.Get("lights", "green"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new Slider(Translation.Get("lights", "blue"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;

        resetPropertiesButton = new Button(Translation.Get("lightsPane", "resetProperties"));
        resetPropertiesButton.ControlEvent += OnResetPropertiesButtonPressed;

        resetPositionButton = new Button(Translation.Get("lightsPane", "resetPosition"));
        resetPositionButton.ControlEvent += OnResetPositionButtonPressed;

        string LightNameFormatter(LightController light, int index) =>
            lightNames[light];
    }

    public static Light MainLight =>
        mainLight ? mainLight : mainLight = GameMain.Instance.MainLight.GetComponent<Light>();

    private LightController CurrentLightController =>
        lightSelectionController.Current;

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawTopBar();

        if (CurrentLightController == null)
        {
            noLightsLabel.Draw();

            return;
        }

        DrawLightType();

        var enabled = GUI.enabled;

        GUI.enabled = lightOnToggle.Value;

        if (CurrentLightController.Type is LightType.Directional)
            DrawDirectionalLightControls();
        else if (CurrentLightController.Type is LightType.Spot)
            DrawSpotLightControls();
        else
            DrawPointLightControls();

        MpsGui.BlackLine();

        DrawColourControls();

        DrawReset();

        GUI.enabled = enabled;

        void DrawTopBar()
        {
            GUI.enabled = lightRepository.Count > 0;

            GUILayout.BeginHorizontal();

            lightDropdown.Draw(GUILayout.Width(84f));

            var noExpandWidth = GUILayout.ExpandWidth(false);

            GUI.enabled = true;

            addLightButton.Draw(noExpandWidth);

            GUI.enabled = lightRepository.Count > 0;

            GUILayout.FlexibleSpace();

            deleteLightButton.Draw(noExpandWidth);
            clearLightsButton.Draw(noExpandWidth);

            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        void DrawLightType()
        {
            GUILayout.BeginHorizontal();

            var enabled = GUI.enabled;

            GUI.enabled = lightOnToggle.Value;

            lightTypeGrid.Draw();

            GUI.enabled = enabled;

            GUILayout.FlexibleSpace();

            lightOnToggle.Draw();

            GUILayout.EndHorizontal();
        }

        void DrawDirectionalLightControls()
        {
            xRotationSlider.Draw();
            yRotationSlider.Draw();
            intensitySlider.Draw();
            shadowStrengthSlider.Draw();
        }

        void DrawSpotLightControls()
        {
            xRotationSlider.Draw();
            yRotationSlider.Draw();
            intensitySlider.Draw();
            rangeSlider.Draw();
            spotAngleSlider.Draw();
        }

        void DrawPointLightControls()
        {
            intensitySlider.Draw();
            rangeSlider.Draw();
        }

        void DrawColourControls()
        {
            redSlider.Draw();
            greenSlider.Draw();
            blueSlider.Draw();
        }

        void DrawReset()
        {
            resetHeader.Draw();
            MpsGui.BlackLine();

            GUILayout.BeginHorizontal();

            resetPropertiesButton.Draw();
            resetPositionButton.Draw();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("lightsPane", "header");
        resetHeader.Text = Translation.Get("lightsPane", "resetLabel");
        noLights = Translation.Get("lightsPane", "noLights");
        noLightsLabel.Text = noLights;
        lightTypeGrid.SetItemsWithoutNotify(Translation.GetArray("lightType", new[] { "normal", "spot", "point" }));
        addLightButton.Label = Translation.Get("lightsPane", "add");
        deleteLightButton.Label = Translation.Get("lightsPane", "delete");
        clearLightsButton.Label = Translation.Get("lightsPane", "clear");
        lightOnToggle.Label = Translation.Get("lightsPane", "on");
        xRotationSlider.Label = Translation.Get("lights", "x");
        yRotationSlider.Label = Translation.Get("lights", "y");
        intensitySlider.Label = Translation.Get("lights", "intensity");
        shadowStrengthSlider.Label = Translation.Get("lights", "shadow");
        rangeSlider.Label = Translation.Get("lights", "range");
        spotAngleSlider.Label = Translation.Get("lights", "spot");
        redSlider.Label = Translation.Get("lights", "red");
        greenSlider.Label = Translation.Get("lights", "green");
        blueSlider.Label = Translation.Get("lights", "blue");
        resetPropertiesButton.Label = Translation.Get("lightsPane", "resetProperties");
        resetPositionButton.Label = Translation.Get("lightsPane", "resetPosition");
    }

    private void OnSelectingLight(object sender, SelectionEventArgs<LightController> e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.PropertyChanged -= OnChangedLightProperties;
        CurrentLightController.ChangedLightType -= OnChangedLightType;
    }

    private void OnSelectedLight(object sender, SelectionEventArgs<LightController> e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.PropertyChanged += OnChangedLightProperties;
        CurrentLightController.ChangedLightType += OnChangedLightType;

        lightDropdown.SetSelectedIndexWithoutNotify(e.Index);

        UpdateControls();
    }

    private void OnRemovedLight(object sender, LightRepositoryEventArgs e)
    {
        if (lightRepository.Count is 0)
        {
            lightDropdown.Clear();
            lightNames.Clear();

            return;
        }

        var lightIndex = lightDropdown.SelectedItemIndex >= lightRepository.Count
            ? lightRepository.Count - 1
            : lightDropdown.SelectedItemIndex;

        lightNames.Remove(e.LightController);
        lightDropdown.SetItems(lightRepository, lightIndex);
    }

    private void OnAddedLight(object sender, LightRepositoryEventArgs e)
    {
        lightNames[e.LightController] = GetNewLightName(e.LightController);
        lightDropdown.SetItems(lightRepository, lightRepository.Count - 1);

        string GetNewLightName(LightController lightController)
        {
            var nameSet = new HashSet<string>(lightNames.Values);

            var lightName = LightName(lightController.Light);
            var newLightName = lightName;
            var index = 1;

            while (nameSet.Contains(newLightName))
            {
                index++;
                newLightName = $"{lightName} ({index})";
            }

            return newLightName;

            static string LightName(Light light) =>
                light == MainLight ? Translation.Get("lightType", "main") : Translation.Get("lightType", "light");
        }
    }

    private void OnChangedLightProperties(object sender, PropertyChangedEventArgs e)
    {
        var light = (LightController)sender;

        if (e.PropertyName is nameof(LightController.Enabled))
        {
            lightOnToggle.SetEnabledWithoutNotify(light.Enabled);
        }
        else if (e.PropertyName is nameof(LightController.Rotation))
        {
            if (sliderChangedTransform)
            {
                sliderChangedTransform = false;

                return;
            }

            var rotation = light.Rotation.eulerAngles;

            xRotationSlider.SetValueWithoutNotify(rotation.x);
            yRotationSlider.SetValueWithoutNotify(rotation.y);
        }
        else if (e.PropertyName is nameof(LightController.Intensity))
        {
            intensitySlider.SetValueWithoutNotify(light.Intensity);
        }
        else if (e.PropertyName is nameof(LightController.Range))
        {
            rangeSlider.SetValueWithoutNotify(light.Range);
        }
        else if (e.PropertyName is nameof(LightController.SpotAngle))
        {
            spotAngleSlider.SetValueWithoutNotify(light.SpotAngle);
        }
        else if (e.PropertyName is nameof(LightController.ShadowStrength))
        {
            shadowStrengthSlider.SetValueWithoutNotify(light.ShadowStrength);
        }
        else if (e.PropertyName is nameof(LightController.Colour))
        {
            redSlider.SetValueWithoutNotify(light.Colour.r);
            greenSlider.SetValueWithoutNotify(light.Colour.g);
            blueSlider.SetValueWithoutNotify(light.Colour.b);
        }
    }

    private void UpdateControls()
    {
        if (CurrentLightController is null)
            return;

        lightTypeGrid.SetValueWithoutNotify(CurrentLightController.Type switch
        {
            LightType.Directional => 0,
            LightType.Spot => 1,
            LightType.Point => 2,
            LightType.Area or _ => 0,
        });

        lightOnToggle.SetEnabledWithoutNotify(CurrentLightController.Enabled);

        var rotation = CurrentLightController.Rotation.eulerAngles;

        xRotationSlider.SetValueWithoutNotify(rotation.x);
        yRotationSlider.SetValueWithoutNotify(rotation.y);
        intensitySlider.SetValueWithoutNotify(CurrentLightController.Intensity);
        shadowStrengthSlider.SetValueWithoutNotify(CurrentLightController.ShadowStrength);
        rangeSlider.SetValueWithoutNotify(CurrentLightController.Range);
        spotAngleSlider.SetValueWithoutNotify(CurrentLightController.SpotAngle);
        redSlider.SetValueWithoutNotify(CurrentLightController.Colour.r);
        greenSlider.SetValueWithoutNotify(CurrentLightController.Colour.g);
        blueSlider.SetValueWithoutNotify(CurrentLightController.Colour.b);
    }

    private void LightDropdownSelectionChanged(object sender, EventArgs e)
    {
        if (lightRepository.Count is 0)
            return;

        lightSelectionController.Select(lightDropdown.SelectedItem);
    }

    private void OnLightTypeChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Type = lightTypeGrid.SelectedItemIndex switch
        {
            0 => LightType.Directional,
            1 => LightType.Spot,
            2 => LightType.Point,
            _ => LightType.Directional,
        };
    }

    private void OnChangedLightType(object sender, KeyedPropertyChangeEventArgs<LightType> e) =>
        lightTypeGrid.SetValueWithoutNotify(e.Key switch
        {
            LightType.Directional => 0,
            LightType.Spot => 1,
            LightType.Point => 2,
            LightType.Area or _ => 0,
        });

    private void OnAddLightButtonPressed(object sender, EventArgs e) =>
        lightRepository.AddLight();

    private void OnDeleteButtonPressed(object sender, EventArgs e)
    {
        if (lightRepository.Count is 0)
            return;

        if (CurrentLightController is null)
            return;

        if (CurrentLightController.Light == GameMain.Instance.MainLight.GetComponent<Light>())
            return;

        lightRepository.RemoveLight(lightRepository.IndexOf(CurrentLightController));
    }

    private void OnClearButtonPressed(object sender, EventArgs e)
    {
        for (var i = lightRepository.Count - 1; i > 0; i--)
            lightRepository.RemoveLight(i);
    }

    private void OnLightOnToggleChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Enabled = lightOnToggle.Value;
    }

    private void OnRotationSlidersChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        sliderChangedTransform = true;

        CurrentLightController.Rotation = Quaternion.Euler(xRotationSlider.Value, yRotationSlider.Value, 0f);
    }

    private void OnIntensitySliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Intensity = intensitySlider.Value;
    }

    private void OnShadowStrenthSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ShadowStrength = shadowStrengthSlider.Value;
    }

    private void OnRangeSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Range = rangeSlider.Value;
    }

    private void OnSpotAngleSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.SpotAngle = spotAngleSlider.Value;
    }

    private void OnColourSliderChanged(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Colour = new(redSlider.Value, greenSlider.Value, blueSlider.Value);
    }

    private void OnResetPositionButtonPressed(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.Position = LightController.DefaultPosition;
    }

    private void OnResetPropertiesButtonPressed(object sender, EventArgs e)
    {
        if (CurrentLightController is null)
            return;

        CurrentLightController.ResetCurrentLightProperties();
    }
}
