using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static DragPointLight;
    internal class LightsPane : BasePane
    {
        private static readonly string[] lightTypes = { "normal", "spot", "point" };
        private readonly LightManager lightManager;
        private readonly Dictionary<LightProp, Slider> LightSlider;
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
        private static readonly Dictionary<LightProp, SliderProp> LightSliderProp =
            new Dictionary<LightProp, SliderProp>()
            {
                [LightProp.LightRotX] = new SliderProp(0f, 360f, LightProperty.DefaultRotation.eulerAngles.x),
                [LightProp.LightRotY] = new SliderProp(0f, 360f, LightProperty.DefaultRotation.eulerAngles.y),
                [LightProp.Intensity] = new SliderProp(0f, 2f, 0.95f),
                [LightProp.ShadowStrength] = new SliderProp(0f, 1f, 0.098f),
                [LightProp.Range] = new SliderProp(0f, 150f, GameMain.Instance.MainLight.GetComponent<Light>().range),
                [LightProp.SpotAngle] = new SliderProp(0f, 150f, 50f),
                [LightProp.Red] = new SliderProp(0f, 1f, 1f),
                [LightProp.Green] = new SliderProp(0f, 1f, 1f),
                [LightProp.Blue] = new SliderProp(0f, 1f, 1f),
            };
        private static readonly string[,] sliderNames = {
            { "lights", "x" }, { "lights", "y" }, { "lights", "intensity" }, { "lights", "shadow" },
            { "lights", "spot" }, { "lights", "range" }, { "backgroundWindow", "red" }, { "backgroundWindow", "green" },
            { "backgroundWindow", "blue" }
        };

        public LightsPane(LightManager lightManager)
        {
            lightHeader = Translation.Get("lightsPane", "header");

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
            addLightButton.ControlEvent += (s, a) => AddLight();

            deleteLightButton = new Button(Translation.Get("lightsPane", "delete"));
            deleteLightButton.ControlEvent += (s, a) => DeleteCurrentLight();

            disableToggle = new Toggle(Translation.Get("lightsPane", "disable"));
            disableToggle.ControlEvent += (s, a) => SetCurrentLightActive();

            clearLightsButton = new Button(Translation.Get("lightsPane", "clear"));
            clearLightsButton.ControlEvent += (s, a) => ClearLights();

            int numberOfLightProps = Enum.GetNames(typeof(LightProp)).Length;
            LightSlider = new Dictionary<LightProp, Slider>(numberOfLightProps);

            for (int i = 0; i < numberOfLightProps; i++)
            {
                LightProp lightProp = (LightProp)i;
                SliderProp sliderProp = LightSliderProp[lightProp];
                Slider slider = new Slider(Translation.Get(sliderNames[i, 0], sliderNames[i, 1]), sliderProp);
                if (lightProp == LightProp.LightRotX || lightProp == LightProp.LightRotY)
                {
                    slider.ControlEvent += (s, a) => SetLightRotation();
                }
                else
                {
                    slider.ControlEvent += (s, a) => SetLightProp(lightProp, slider.Value);
                }
                LightSlider[lightProp] = slider;
            }

            colorToggle = new Toggle(Translation.Get("lightsPane", "colour"));
            colorToggle.ControlEvent += (s, a) => SetColourMode();

            resetPropsButton = new Button(Translation.Get("lightsPane", "resetProperties"));
            resetPropsButton.ControlEvent += (s, a) => ResetLightProps();

            resetPositionButton = new Button(Translation.Get("lightsPane", "resetPosition"));
            resetPositionButton.ControlEvent += (s, a) => ResetLightPosition();
        }

        protected override void ReloadTranslation()
        {
            updating = true;
            lightHeader = Translation.Get("lightsPane", "header");
            lightTypeGrid.SetItems(Translation.GetArray("lightType", lightTypes));
            lightDropdown.SetDropdownItems(lightManager.LightNameList);
            deleteLightButton.Label = Translation.Get("lightsPane", "delete");
            disableToggle.Label = Translation.Get("lightsPane", "disable");
            clearLightsButton.Label = Translation.Get("lightsPane", "clear");
            for (LightProp lightProp = LightProp.LightRotX; lightProp <= LightProp.Blue; lightProp++)
            {
                LightSlider[lightProp].Label =
                    Translation.Get(sliderNames[(int)lightProp, 0], sliderNames[(int)lightProp, 1]);
            }
            colorToggle.Label = Translation.Get("lightsPane", "colour");
            resetPropsButton.Label = Translation.Get("lightsPane", "resetProperties");
            resetPositionButton.Label = Translation.Get("lightsPane", "resetPosition");
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

        private void ResetLightPosition() => lightManager.CurrentLight.ResetLightPosition();

        private void AddLight() => lightManager.AddLight();

        private void DeleteCurrentLight() => lightManager.DeleteActiveLight();

        private void SetCurrentLightActive() => lightManager.CurrentLight.IsDisabled = disableToggle.Value;

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
            float lightRotX = LightSlider[LightProp.LightRotX].Value;
            float lightRotY = LightSlider[LightProp.LightRotY].Value;
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
            LightSlider[LightProp.LightRotX].Value = prop.Rotation.eulerAngles.x;
            LightSlider[LightProp.LightRotY].Value = prop.Rotation.eulerAngles.y;
            updating = false;
        }

        private void UpdateScale()
        {
            updating = true;
            LightSlider[LightProp.SpotAngle].Value = lightManager.CurrentLight.CurrentLightProperty.SpotAngle;
            LightSlider[LightProp.Range].Value = lightManager.CurrentLight.CurrentLightProperty.Range;
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
            LightSlider[LightProp.LightRotX].Value = currentLight.Rotation.eulerAngles.x;
            LightSlider[LightProp.LightRotY].Value = currentLight.Rotation.eulerAngles.y;
            LightSlider[LightProp.Intensity].Value = currentLight.Intensity;
            LightSlider[LightProp.ShadowStrength].Value = currentLight.ShadowStrength;
            LightSlider[LightProp.Range].Value = currentLight.Range;
            LightSlider[LightProp.SpotAngle].Value = currentLight.SpotAngle;
            LightSlider[LightProp.Red].Value = currentLight.LightColour.r;
            LightSlider[LightProp.Green].Value = currentLight.LightColour.g;
            LightSlider[LightProp.Blue].Value = currentLight.LightColour.b;
            updating = false;
        }

        public override void Draw()
        {
            bool isMain = lightManager.SelectedLightIndex == 0;

            MpsGui.Header(lightHeader);
            MpsGui.WhiteLine();

            GUILayout.BeginHorizontal();
            lightDropdown.Draw(GUILayout.Width(84));
            addLightButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.FlexibleSpace();
            GUI.enabled = !isMain;
            deleteLightButton.Draw(GUILayout.ExpandWidth(false));
            GUI.enabled = true;
            clearLightsButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            bool isDisabled = !isMain && lightManager.CurrentLight.IsDisabled;
            GUILayout.BeginHorizontal();
            GUI.enabled = !isDisabled;
            lightTypeGrid.Draw(GUILayout.ExpandWidth(false));
            if (!isMain)
            {
                GUI.enabled = true;
                disableToggle.Draw();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !isDisabled;

            if (currentLightType != MPSLightType.Point)
            {
                LightSlider[LightProp.LightRotX].Draw();
                LightSlider[LightProp.LightRotY].Draw();
            }

            LightSlider[LightProp.Intensity].Draw();

            if (currentLightType == MPSLightType.Normal)
            {
                LightSlider[LightProp.ShadowStrength].Draw();
            }
            else
            {
                LightSlider[LightProp.Range].Draw();
            }

            if (currentLightType == MPSLightType.Spot)
            {
                LightSlider[LightProp.SpotAngle].Draw();
            }

            GUILayoutOption sliderWidth = MpsGui.HalfSlider;
            GUILayout.BeginHorizontal();
            LightSlider[LightProp.Red].Draw(sliderWidth);
            LightSlider[LightProp.Green].Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            LightSlider[LightProp.Blue].Draw(sliderWidth);
            if ((lightManager.SelectedLightIndex == 0) && (currentLightType == MPSLightType.Normal))
            {
                colorToggle.Draw();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            resetPropsButton.Draw(GUILayout.ExpandWidth(false));
            resetPositionButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }
    }
}
