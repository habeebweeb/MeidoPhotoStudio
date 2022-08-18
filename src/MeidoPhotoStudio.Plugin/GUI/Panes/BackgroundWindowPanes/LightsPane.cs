using System;
using System.Collections.Generic;

using UnityEngine;

using static MeidoPhotoStudio.Plugin.DragPointLight;

namespace MeidoPhotoStudio.Plugin;

public class LightsPane : BasePane
{
    private static readonly string[] LightTypes = { "normal", "spot", "point" };
    private static readonly Dictionary<LightProp, SliderProp> LightSliderProp;
    private static readonly string[,] SliderNames =
    {
        { "lights", "x" },
        { "lights", "y" },
        { "lights", "intensity" },
        { "lights", "shadow" },
        { "lights", "spot" },
        { "lights", "range" },
        { "backgroundWindow", "red" },
        { "backgroundWindow", "green" },
        { "backgroundWindow", "blue" },
    };

    private readonly LightManager lightManager;
    private readonly Dictionary<LightProp, Slider> lightSlider;
    private readonly Dropdown lightDropdown;
    private readonly Button addLightButton;
    private readonly Button deleteLightButton;
    private readonly Button clearLightsButton;
    private readonly Button resetPropsButton;
    private readonly Button resetPositionButton;
    private readonly SelectionGrid lightTypeGrid;
    private readonly Toggle colorToggle;
    private readonly Toggle disableToggle;

    private MPSLightType currentLightType;
    private string lightHeader;
    private string resetLabel;

    static LightsPane()
    {
        var rotation = LightProperty.DefaultRotation.eulerAngles;
        var range = GameMain.Instance.MainLight.GetComponent<Light>().range;

        LightSliderProp = new()
        {
            [LightProp.LightRotX] = new(0f, 360f, rotation.x, rotation.x),
            [LightProp.LightRotY] = new(0f, 360f, rotation.y, rotation.y),
            [LightProp.Intensity] = new(0f, 2f, 0.95f, 0.95f),
            [LightProp.ShadowStrength] = new(0f, 1f, 0.098f, 0.098f),
            [LightProp.Range] = new(0f, 150f, range, range),
            [LightProp.SpotAngle] = new(0f, 150f, 50f, 50f),
            [LightProp.Red] = new(0f, 1f, 1f, 1f),
            [LightProp.Green] = new(0f, 1f, 1f, 1f),
            [LightProp.Blue] = new(0f, 1f, 1f, 1f),
        };
    }

    public LightsPane(LightManager lightManager)
    {
        this.lightManager = lightManager;
        this.lightManager.Rotate += (_, _) =>
            UpdateRotation();

        this.lightManager.Scale += (_, _) =>
            UpdateScale();

        this.lightManager.Select += (_, _) =>
            UpdateCurrentLight();

        this.lightManager.ListModified += (_, _) =>
            UpdateList();

        lightTypeGrid = new(Translation.GetArray("lightType", LightTypes));
        lightTypeGrid.ControlEvent += (_, _) =>
            SetCurrentLightType();

        lightDropdown = new(new[] { "Main" });
        lightDropdown.SelectionChange += (_, _) =>
            SetCurrentLight();

        addLightButton = new("+");
        addLightButton.ControlEvent += (_, _) =>
            lightManager.AddLight();

        deleteLightButton = new(Translation.Get("lightsPane", "delete"));
        deleteLightButton.ControlEvent += (_, _) =>
            lightManager.DeleteActiveLight();

        disableToggle = new(Translation.Get("lightsPane", "disable"));
        disableToggle.ControlEvent += (_, _) =>
            lightManager.CurrentLight.IsDisabled = disableToggle.Value;

        clearLightsButton = new(Translation.Get("lightsPane", "clear"));
        clearLightsButton.ControlEvent += (_, _) =>
            ClearLights();

        var numberOfLightProps = Enum.GetNames(typeof(LightProp)).Length;

        lightSlider = new(numberOfLightProps);

        for (var i = 0; i < numberOfLightProps; i++)
        {
            var lightProp = (LightProp)i;
            var sliderProp = LightSliderProp[lightProp];

            var slider = new Slider(Translation.Get(SliderNames[i, 0], SliderNames[i, 1]), sliderProp)
            {
                HasTextField = true,
                HasReset = true,
            };

            if (lightProp <= LightProp.LightRotY)
                slider.ControlEvent += (_, _) =>
                    SetLightRotation();
            else
                slider.ControlEvent += (_, _) =>
                    SetLightProp(lightProp, slider.Value);

            lightSlider[lightProp] = slider;
        }

        colorToggle = new(Translation.Get("lightsPane", "colour"));
        colorToggle.ControlEvent += (_, _) =>
            SetColourMode();

        resetPropsButton = new(Translation.Get("lightsPane", "resetProperties"));
        resetPropsButton.ControlEvent += (_, _) =>
            ResetLightProps();

        resetPositionButton = new(Translation.Get("lightsPane", "resetPosition"));
        resetPositionButton.ControlEvent += (_, _) =>
            lightManager.CurrentLight.ResetLightPosition();

        lightHeader = Translation.Get("lightsPane", "header");
        resetLabel = Translation.Get("lightsPane", "resetLabel");
    }

    public override void UpdatePane()
    {
        updating = true;

        var currentLight = lightManager.CurrentLight;

        currentLightType = currentLight.SelectedLightType;
        lightTypeGrid.SelectedItemIndex = (int)currentLightType;
        disableToggle.Value = currentLight.IsDisabled;
        lightSlider[LightProp.LightRotX].Value = currentLight.Rotation.eulerAngles.x;
        lightSlider[LightProp.LightRotY].Value = currentLight.Rotation.eulerAngles.y;
        lightSlider[LightProp.Intensity].Value = currentLight.Intensity;
        lightSlider[LightProp.ShadowStrength].Value = currentLight.ShadowStrength;
        lightSlider[LightProp.Range].Value = currentLight.Range;
        lightSlider[LightProp.SpotAngle].Value = currentLight.SpotAngle;
        lightSlider[LightProp.Red].Value = currentLight.LightColour.r;
        lightSlider[LightProp.Green].Value = currentLight.LightColour.g;
        lightSlider[LightProp.Blue].Value = currentLight.LightColour.b;

        updating = false;
    }

    public override void Draw()
    {
        var isMain = lightManager.SelectedLightIndex is 0;
        var noExpandWidth = GUILayout.ExpandWidth(false);

        MpsGui.Header(lightHeader);
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        lightDropdown.Draw(GUILayout.Width(84));
        addLightButton.Draw(noExpandWidth);

        GUILayout.FlexibleSpace();
        GUI.enabled = !isMain;
        deleteLightButton.Draw(noExpandWidth);
        GUI.enabled = true;
        clearLightsButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        var isDisabled = !isMain && lightManager.CurrentLight.IsDisabled;

        GUILayout.BeginHorizontal();
        GUI.enabled = !isDisabled;
        lightTypeGrid.Draw(noExpandWidth);

        if (!isMain)
        {
            GUI.enabled = true;
            disableToggle.Draw();
        }

        if (lightManager.SelectedLightIndex is 0 && currentLightType is MPSLightType.Normal)
            colorToggle.Draw();

        GUILayout.EndHorizontal();

        GUI.enabled = !isDisabled;

        if (currentLightType is not MPSLightType.Point)
        {
            lightSlider[LightProp.LightRotX].Draw();
            lightSlider[LightProp.LightRotY].Draw();
        }

        lightSlider[LightProp.Intensity].Draw();

        if (currentLightType is MPSLightType.Normal)
            lightSlider[LightProp.ShadowStrength].Draw();
        else
            lightSlider[LightProp.Range].Draw();

        if (currentLightType is MPSLightType.Spot)
            lightSlider[LightProp.SpotAngle].Draw();

        MpsGui.BlackLine();

        lightSlider[LightProp.Red].Draw();
        lightSlider[LightProp.Green].Draw();
        lightSlider[LightProp.Blue].Draw();

        GUILayout.BeginHorizontal();
        GUILayout.Label(resetLabel, noExpandWidth);
        resetPropsButton.Draw(noExpandWidth);
        resetPositionButton.Draw(noExpandWidth);
        GUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        updating = true;

        lightTypeGrid.SetItems(Translation.GetArray("lightType", LightTypes));
        lightDropdown.SetDropdownItems(lightManager.LightNameList);
        deleteLightButton.Label = Translation.Get("lightsPane", "delete");
        disableToggle.Label = Translation.Get("lightsPane", "disable");
        clearLightsButton.Label = Translation.Get("lightsPane", "clear");

        for (var lightProp = LightProp.LightRotX; lightProp <= LightProp.Blue; lightProp++)
            lightSlider[lightProp].Label =
                Translation.Get(SliderNames[(int)lightProp, 0], SliderNames[(int)lightProp, 1]);

        colorToggle.Label = Translation.Get("lightsPane", "colour");
        resetPropsButton.Label = Translation.Get("lightsPane", "resetProperties");
        resetPositionButton.Label = Translation.Get("lightsPane", "resetPosition");
        lightHeader = Translation.Get("lightsPane", "header");
        resetLabel = Translation.Get("lightsPane", "resetLabel");

        updating = false;
    }

    private void SetColourMode()
    {
        lightManager.SetColourModeActive(colorToggle.Value);
        UpdatePane();
    }

    private void ClearLights()
    {
        lightManager.ClearLights();
        UpdatePane();
    }

    private void SetCurrentLight()
    {
        if (updating)
            return;

        lightManager.SelectedLightIndex = lightDropdown.SelectedItemIndex;
        UpdatePane();
    }

    private void ResetLightProps()
    {
        lightManager.CurrentLight.ResetLightProps();
        UpdatePane();
    }

    private void SetCurrentLightType()
    {
        if (updating)
            return;

        currentLightType = (MPSLightType)lightTypeGrid.SelectedItemIndex;

        var currentLight = lightManager.CurrentLight;

        currentLight.SetLightType(currentLightType);

        lightDropdown.SetDropdownItem(lightManager.ActiveLightName);
        UpdatePane();
    }

    private void SetLightProp(LightProp prop, float value)
    {
        if (updating)
            return;

        lightManager.CurrentLight.SetProp(prop, value);
    }

    private void SetLightRotation()
    {
        if (updating)
            return;

        var lightRotX = lightSlider[LightProp.LightRotX].Value;
        var lightRotY = lightSlider[LightProp.LightRotY].Value;

        lightManager.CurrentLight.SetRotation(lightRotX, lightRotY);
    }

    private void UpdateList()
    {
        var newList = lightManager.LightNameList;

        lightDropdown.SetDropdownItems(newList, lightManager.SelectedLightIndex);
        UpdatePane();
    }

    private void UpdateRotation()
    {
        updating = true;

        var prop = lightManager.CurrentLight.CurrentLightProperty;

        lightSlider[LightProp.LightRotX].Value = prop.Rotation.eulerAngles.x;
        lightSlider[LightProp.LightRotY].Value = prop.Rotation.eulerAngles.y;

        updating = false;
    }

    private void UpdateScale()
    {
        updating = true;

        lightSlider[LightProp.SpotAngle].Value = lightManager.CurrentLight.CurrentLightProperty.SpotAngle;
        lightSlider[LightProp.Range].Value = lightManager.CurrentLight.CurrentLightProperty.Range;

        updating = false;
    }

    private void UpdateCurrentLight()
    {
        updating = true;

        lightDropdown.SelectedItemIndex = lightManager.SelectedLightIndex;

        updating = false;

        UpdatePane();
    }
}
