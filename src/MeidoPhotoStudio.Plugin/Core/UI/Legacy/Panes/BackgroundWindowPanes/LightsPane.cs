using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Lighting;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class LightsPane : BasePane
{
    private static Light mainLight;
    private readonly Translation translation;
    private readonly LightRepository lightRepository;
    private readonly SelectionController<LightController> lightSelectionController;
    private readonly Dropdown<LightController> lightDropdown;
    private readonly Dictionary<LightController, string> lightNames = [];
    private readonly Dictionary<LightType, Toggle> lightTypeToggles;
    private readonly Toggle.Group lightTypeGroup;
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
    private readonly SubPaneHeader transformInputToggle;
    private readonly TransformInputPane transformInputPane;
    private readonly Button resetPositionButton;
    private readonly Button resetPropertiesButton;
    private readonly Header resetHeader;
    private readonly Label noLightsLabel;
    private readonly LazyStyle noLightsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private bool sliderChangedTransform;

    public LightsPane(
        Translation translation,
        LightRepository lightRepository,
        SelectionController<LightController> lightSelectionController,
        TransformClipboard transformClipboard)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.lightRepository = lightRepository ?? throw new ArgumentNullException(nameof(lightRepository));
        this.lightSelectionController = lightSelectionController ?? throw new ArgumentNullException(nameof(lightSelectionController));
        _ = transformClipboard ?? throw new ArgumentNullException(nameof(transformClipboard));

        lightRepository.AddedLight += OnAddedLight;
        lightRepository.RemovedLight += OnRemovedLight;

        lightSelectionController.Selecting += OnSelectingLight;
        lightSelectionController.Selected += OnSelectedLight;

        resetHeader = new(new LocalizableGUIContent(translation, "lightsPane", "resetLabel"));
        noLightsLabel = new(new LocalizableGUIContent(translation, "lightsPane", "noLights"));

        lightDropdown = new(formatter: LightNameFormatter);
        lightDropdown.SelectionChanged += LightDropdownSelectionChanged;

        var normalLightTypeToggle = new Toggle(new LocalizableGUIContent(translation, "lightType", "normal"), true);

        normalLightTypeToggle.ControlEvent += OnLightTypeToggleChanged(LightType.Directional);

        var spotLightTypeToggle = new Toggle(new LocalizableGUIContent(translation, "lightType", "spot"));

        spotLightTypeToggle.ControlEvent += OnLightTypeToggleChanged(LightType.Spot);

        var pointLightTypeToggle = new Toggle(new LocalizableGUIContent(translation, "lightType", "point"));

        pointLightTypeToggle.ControlEvent += OnLightTypeToggleChanged(LightType.Point);

        lightTypeGroup = [normalLightTypeToggle, spotLightTypeToggle, pointLightTypeToggle];

        lightTypeToggles = new()
        {
            [LightType.Directional] = normalLightTypeToggle,
            [LightType.Spot] = spotLightTypeToggle,
            [LightType.Point] = pointLightTypeToggle,
        };

        addLightButton = new(new LocalizableGUIContent(translation, "lightsPane", "add"));
        addLightButton.ControlEvent += OnAddLightButtonPressed;

        deleteLightButton = new(new LocalizableGUIContent(translation, "lightsPane", "delete"));
        deleteLightButton.ControlEvent += OnDeleteButtonPressed;

        clearLightsButton = new(new LocalizableGUIContent(translation, "lightsPane", "clear"));
        clearLightsButton.ControlEvent += OnClearButtonPressed;

        lightOnToggle = new(new LocalizableGUIContent(translation, "lightsPane", "on"), true);
        lightOnToggle.ControlEvent += OnLightOnToggleChanged;

        var defaultRotation = LightProperties.DefaultRotation.eulerAngles;

        xRotationSlider = new(
            new LocalizableGUIContent(translation, "lights", "x"), 0f, 360f, defaultRotation.x, defaultRotation.x)
        {
            HasTextField = true,
            HasReset = true,
        };

        xRotationSlider.ControlEvent += OnRotationSlidersChanged;

        yRotationSlider = new(
            new LocalizableGUIContent(translation, "lights", "y"), 0f, 360f, defaultRotation.y, defaultRotation.y)
        {
            HasTextField = true,
            HasReset = true,
        };

        yRotationSlider.ControlEvent += OnRotationSlidersChanged;

        intensitySlider = new(new LocalizableGUIContent(translation, "lights", "intensity"), 0f, 2f, 0.95f, 0.95f)
        {
            HasTextField = true,
            HasReset = true,
        };

        intensitySlider.ControlEvent += OnIntensitySliderChanged;

        shadowStrengthSlider = new(new LocalizableGUIContent(translation, "lights", "shadow"), 0f, 1f, 0.10f, 0.10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        shadowStrengthSlider.ControlEvent += OnShadowStrenthSliderChanged;

        rangeSlider = new(new LocalizableGUIContent(translation, "lights", "range"), 0f, 150f, 10f, 10f)
        {
            HasTextField = true,
            HasReset = true,
        };

        rangeSlider.ControlEvent += OnRangeSliderChanged;

        spotAngleSlider = new(new LocalizableGUIContent(translation, "lights", "spot"), 0f, 150f, 50f, 50f)
        {
            HasTextField = true,
            HasReset = true,
        };

        spotAngleSlider.ControlEvent += OnSpotAngleSliderChanged;

        redSlider = new(new LocalizableGUIContent(translation, "lights", "red"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        redSlider.ControlEvent += OnColourSliderChanged;

        greenSlider = new(new LocalizableGUIContent(translation, "lights", "green"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        greenSlider.ControlEvent += OnColourSliderChanged;

        blueSlider = new(new LocalizableGUIContent(translation, "lights", "blue"), 0f, 1f, 1f, 1f)
        {
            HasTextField = true,
            HasReset = true,
        };

        blueSlider.ControlEvent += OnColourSliderChanged;

        resetPropertiesButton = new(new LocalizableGUIContent(translation, "lightsPane", "resetProperties"));
        resetPropertiesButton.ControlEvent += OnResetPropertiesButtonPressed;

        resetPositionButton = new(new LocalizableGUIContent(translation, "lightsPane", "resetPosition"));
        resetPositionButton.ControlEvent += OnResetPositionButtonPressed;

        transformInputToggle = new(new LocalizableGUIContent(translation, "lightsPane", "preciseTransformToggle"), false);

        transformInputPane = new(translation, transformClipboard)
        {
            EnableScale = false,
        };

        Add(transformInputPane);

        LabelledDropdownItem LightNameFormatter(LightController light, int index) =>
            new(lightNames[light]);

        EventHandler OnLightTypeToggleChanged(LightType type) =>
            (sender, _) =>
            {
                if (sender is not Toggle { Value: true })
                    return;

                if (CurrentLightController is not LightController controller)
                    return;

                controller.Type = type;
            };
    }

    public static Light MainLight =>
        mainLight ? mainLight : mainLight = GameMain.Instance.MainLight.GetComponent<Light>();

    private LightController CurrentLightController =>
        lightSelectionController.Current;

    public override void Draw()
    {
        var enabled = Parent.Enabled;

        DrawTopBar();

        UIUtility.DrawBlackLine();

        if (CurrentLightController == null)
        {
            noLightsLabel.Draw(noLightsLabelStyle);

            return;
        }

        DrawLightType();

        UIUtility.DrawBlackLine();

        GUI.enabled = enabled && lightOnToggle.Value;

        if (CurrentLightController.Type is LightType.Directional)
            DrawDirectionalLightControls();
        else if (CurrentLightController.Type is LightType.Spot)
            DrawSpotLightControls();
        else
            DrawPointLightControls();

        UIUtility.DrawBlackLine();

        DrawColourControls();

        UIUtility.DrawBlackLine();

        transformInputToggle.Draw();

        if (transformInputToggle.Enabled)
            transformInputPane.Draw();

        DrawReset();

        void DrawTopBar()
        {
            GUI.enabled = enabled && lightRepository.Count > 0;

            GUILayout.BeginHorizontal();

            lightDropdown.Draw(GUILayout.Width(Parent.WindowRect.width - UIUtility.Scaled(185)));

            var noExpandWidth = GUILayout.ExpandWidth(false);

            GUI.enabled = enabled;

            addLightButton.Draw(noExpandWidth);

            GUI.enabled = enabled && lightRepository.Count > 0;

            GUILayout.FlexibleSpace();

            deleteLightButton.Draw(noExpandWidth);
            clearLightsButton.Draw(noExpandWidth);

            GUILayout.EndHorizontal();
        }

        void DrawLightType()
        {
            GUILayout.BeginHorizontal();

            GUI.enabled = enabled && lightOnToggle.Value;

            GUILayout.BeginHorizontal();

            foreach (var lightTypeToggle in lightTypeGroup)
                lightTypeToggle.Draw();

            GUILayout.EndHorizontal();

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

            GUILayout.BeginHorizontal();

            resetPropertiesButton.Draw();
            resetPositionButton.Draw();

            GUILayout.EndHorizontal();
        }
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
        transformInputPane.Target = e.Selected;

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

            string LightName(Light light) =>
                translation["lightType", light == MainLight ? "main" : "light"];
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
        if (CurrentLightController is not LightController controller)
            return;

        if (lightTypeToggles.TryGetValue(controller.Type, out var lightTypeToggle))
            lightTypeToggle.SetEnabledWithoutNotify(true);

        lightOnToggle.SetEnabledWithoutNotify(controller.Enabled);

        var rotation = controller.Rotation.eulerAngles;

        xRotationSlider.SetValueWithoutNotify(rotation.x);
        yRotationSlider.SetValueWithoutNotify(rotation.y);
        intensitySlider.SetValueWithoutNotify(controller.Intensity);
        shadowStrengthSlider.SetValueWithoutNotify(controller.ShadowStrength);
        rangeSlider.SetValueWithoutNotify(controller.Range);
        spotAngleSlider.SetValueWithoutNotify(controller.SpotAngle);
        redSlider.SetValueWithoutNotify(controller.Colour.r);
        greenSlider.SetValueWithoutNotify(controller.Colour.g);
        blueSlider.SetValueWithoutNotify(controller.Colour.b);
    }

    private void LightDropdownSelectionChanged(object sender, EventArgs e)
    {
        if (lightRepository.Count is 0)
            return;

        lightSelectionController.Select(lightDropdown.SelectedItem);
    }

    private void OnChangedLightType(object sender, KeyedPropertyChangeEventArgs<LightType> e)
    {
        if (!lightTypeToggles.TryGetValue(e.Key, out var lightTypeToggle))
            return;

        lightTypeToggle.SetEnabledWithoutNotify(true);
    }

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
