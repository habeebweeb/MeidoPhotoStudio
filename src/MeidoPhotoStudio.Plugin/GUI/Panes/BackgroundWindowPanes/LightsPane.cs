using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin
{
    using static DragPointLight;
    public class LightsPane : BasePane
    {
        private static readonly string[] lightTypes = { "normal", "spot", "point" };
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

        private static readonly Dictionary<LightProp, SliderProp> lightSliderProp;
        private static readonly string[,] sliderNames = {
            { "lights", "x" }, { "lights", "y" }, { "lights", "intensity" }, { "lights", "shadow" },
            { "lights", "spot" }, { "lights", "range" }, { "backgroundWindow", "red" }, { "backgroundWindow", "green" },
            { "backgroundWindow", "blue" }
        };

        static LightsPane()
        {
            Vector3 rotation = LightProperty.DefaultRotation.eulerAngles;
            var range = GameMain.Instance.MainLight.GetComponent<Light>().range;
            lightSliderProp = new Dictionary<LightProp, SliderProp>
            {
                [LightProp.LightRotX] = new SliderProp(0f, 360f, rotation.x, rotation.x),
                [LightProp.LightRotY] = new SliderProp(0f, 360f, rotation.y, rotation.y),
                [LightProp.Intensity] = new SliderProp(0f, 2f, 0.95f, 0.95f),
                [LightProp.ShadowStrength] = new SliderProp(0f, 1f, 0.098f, 0.098f),
                [LightProp.Range] = new SliderProp(0f, 150f, range, range),
                [LightProp.SpotAngle] = new SliderProp(0f, 150f, 50f, 50f),
                [LightProp.Red] = new SliderProp(0f, 1f, 1f, 1f),
                [LightProp.Green] = new SliderProp(0f, 1f, 1f, 1f),
                [LightProp.Blue] = new SliderProp(0f, 1f, 1f, 1f),
            };
        }

        public LightsPane(LightManager lightManager)
        {
            this.lightManager = lightManager;
            this.lightManager.Rotate += (s, a) => UpdateRotation();
            this.lightManager.Scale += (s, a) => UpdateScale();
            this.lightManager.Select += (s, a) => UpdateCurrentLight();
            this.lightManager.ListModified += (s, a) => UpdateList();

            lightTypeGrid = new SelectionGrid(Translation.GetArray("lightType", lightTypes));
            lightTypeGrid.ControlEvent += (s, a) => SetCurrentLightType();

            lightDropdown = new Dropdown(new[] { "Main" });
            lightDropdown.SelectionChange += (s, a) => SetCurrentLight();

            addLightButton = new Button("+");
            addLightButton.ControlEvent += (s, a) => lightManager.AddLight();

            deleteLightButton = new Button(Translation.Get("lightsPane", "delete"));
            deleteLightButton.ControlEvent += (s, a) => lightManager.DeleteActiveLight();

            disableToggle = new Toggle(Translation.Get("lightsPane", "disable"));
            disableToggle.ControlEvent += (s, a) => lightManager.CurrentLight.IsDisabled = disableToggle.Value;

            clearLightsButton = new Button(Translation.Get("lightsPane", "clear"));
            clearLightsButton.ControlEvent += (s, a) => ClearLights();

            var numberOfLightProps = Enum.GetNames(typeof(LightProp)).Length;
            lightSlider = new Dictionary<LightProp, Slider>(numberOfLightProps);

            for (var i = 0; i < numberOfLightProps; i++)
            {
                var lightProp = (LightProp)i;
                SliderProp sliderProp = lightSliderProp[lightProp];
                var slider = new Slider(Translation.Get(sliderNames[i, 0], sliderNames[i, 1]), sliderProp)
                {
                    HasTextField = true,
                    HasReset = true
                };
                if (lightProp <= LightProp.LightRotY) slider.ControlEvent += (s, a) => SetLightRotation();
                else slider.ControlEvent += (s, a) => SetLightProp(lightProp, slider.Value);
                lightSlider[lightProp] = slider;
            }

            colorToggle = new Toggle(Translation.Get("lightsPane", "colour"));
            colorToggle.ControlEvent += (s, a) => SetColourMode();

            resetPropsButton = new Button(Translation.Get("lightsPane", "resetProperties"));
            resetPropsButton.ControlEvent += (s, a) => ResetLightProps();

            resetPositionButton = new Button(Translation.Get("lightsPane", "resetPosition"));
            resetPositionButton.ControlEvent += (s, a) => lightManager.CurrentLight.ResetLightPosition();

            lightHeader = Translation.Get("lightsPane", "header");
            resetLabel = Translation.Get("lightsPane", "resetLabel");
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            lightTypeGrid.SetItems(Translation.GetArray("lightType", lightTypes));
            lightDropdown.SetDropdownItems(lightManager.LightNameList);
            deleteLightButton.Label = Translation.Get("lightsPane", "delete");
            disableToggle.Label = Translation.Get("lightsPane", "disable");
            clearLightsButton.Label = Translation.Get("lightsPane", "clear");
            for (var lightProp = LightProp.LightRotX; lightProp <= LightProp.Blue; lightProp++)
            {
                lightSlider[lightProp].Label =
                    Translation.Get(sliderNames[(int)lightProp, 0], sliderNames[(int)lightProp, 1]);
            }
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
            if (updating) return;
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
            if (updating) return;

            currentLightType = (MPSLightType)lightTypeGrid.SelectedItemIndex;

            DragPointLight currentLight = lightManager.CurrentLight;

            currentLight.SetLightType(currentLightType);

            lightDropdown.SetDropdownItem(lightManager.ActiveLightName);
            UpdatePane();
        }

        private void SetLightProp(LightProp prop, float value)
        {
            if (updating) return;
            lightManager.CurrentLight.SetProp(prop, value);
        }

        private void SetLightRotation()
        {
            if (updating) return;
            var lightRotX = lightSlider[LightProp.LightRotX].Value;
            var lightRotY = lightSlider[LightProp.LightRotY].Value;
            lightManager.CurrentLight.SetRotation(lightRotX, lightRotY);
        }

        private void UpdateList()
        {
            string[] newList = lightManager.LightNameList;
            lightDropdown.SetDropdownItems(newList, lightManager.SelectedLightIndex);
            UpdatePane();
        }

        private void UpdateRotation()
        {
            updating = true;
            LightProperty prop = lightManager.CurrentLight.CurrentLightProperty;
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

        public override void UpdatePane()
        {
            updating = true;
            DragPointLight currentLight = lightManager.CurrentLight;
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
            var isMain = lightManager.SelectedLightIndex == 0;

            GUILayoutOption noExpandWidth = GUILayout.ExpandWidth(false);

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

            bool isDisabled = !isMain && lightManager.CurrentLight.IsDisabled;
            GUILayout.BeginHorizontal();
            GUI.enabled = !isDisabled;
            lightTypeGrid.Draw(noExpandWidth);
            if (!isMain)
            {
                GUI.enabled = true;
                disableToggle.Draw();
            }

            if (lightManager.SelectedLightIndex == 0 && currentLightType == MPSLightType.Normal) 
                colorToggle.Draw();
            
            GUILayout.EndHorizontal();

            GUI.enabled = !isDisabled;

            if (currentLightType != MPSLightType.Point)
            {
                lightSlider[LightProp.LightRotX].Draw();
                lightSlider[LightProp.LightRotY].Draw();
            }

            lightSlider[LightProp.Intensity].Draw();

            if (currentLightType == MPSLightType.Normal) lightSlider[LightProp.ShadowStrength].Draw();
            else lightSlider[LightProp.Range].Draw();

            if (currentLightType == MPSLightType.Spot) lightSlider[LightProp.SpotAngle].Draw();

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
    }
}
