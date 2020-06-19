using System;
using System.Collections.Generic;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using static MPSLight;
    internal class LightsPane : BasePane
    {
        private LightManager lightManager;
        private EnvironmentManager environmentManager;
        private static readonly string[] lightTypes = { "normal", "spot", "point" };
        private Dictionary<LightProp, Slider> LightSlider;
        private Dropdown lightDropdown;
        private Button addLightButton;
        private Button deleteLightButton;
        private Button clearLightsButton;
        private Button resetPropsButton;
        private Button resetPositionButton;
        private SelectionGrid lightTypeGrid;
        private Toggle colorToggle;
        private Toggle disableToggle;
        private MPSLightType currentLightType = MPSLightType.Normal;
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
        private static string[,] sliderNames = {
            { "lights", "x" }, { "lights", "y" }, { "lights", "intensity" }, { "lights", "shadow" },
            { "lights", "spot" }, { "lights", "range" }, { "backgroundWindow", "red" }, { "backgroundWindow", "green" },
            { "backgroundWindow", "blue" }
        };

        public LightsPane(EnvironmentManager environmentManager)
        {
            this.lightHeader = Translation.Get("lightsPane", "header");

            this.environmentManager = environmentManager;

            this.lightManager = this.environmentManager.LightManager;
            this.lightManager.Rotate += (s, a) => UpdateRotation();
            this.lightManager.Scale += (s, a) => UpdateScale();
            this.lightManager.Select += (s, a) => UpdateCurrentLight();
            this.lightManager.ListModified += (s, a) => UpdateList();

            this.lightTypeGrid = new SelectionGrid(Translation.GetArray("lightType", lightTypes));
            this.lightTypeGrid.ControlEvent += (s, a) => SetCurrentLightType();

            this.lightDropdown = new Dropdown(new[] { "Main" });
            this.lightDropdown.SelectionChange += (s, a) => SetCurrentLight();

            this.addLightButton = new Button("+");
            this.addLightButton.ControlEvent += (s, a) => AddLight();

            this.deleteLightButton = new Button(Translation.Get("lightsPane", "delete"));
            this.deleteLightButton.ControlEvent += (s, a) => DeleteCurrentLight();

            this.disableToggle = new Toggle(Translation.Get("lightsPane", "disable"));
            this.disableToggle.ControlEvent += (s, a) => SetCurrentLightActive();

            this.clearLightsButton = new Button(Translation.Get("lightsPane", "clear"));
            this.clearLightsButton.ControlEvent += (s, a) => ClearLights();

            int numberOfLightProps = Enum.GetNames(typeof(LightProp)).Length;
            this.LightSlider = new Dictionary<LightProp, Slider>(numberOfLightProps);

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

            this.colorToggle = new Toggle(Translation.Get("lightsPane", "colour"));
            this.colorToggle.ControlEvent += (s, a) => SetColourMode();

            this.resetPropsButton = new Button(Translation.Get("lightsPane", "resetProperties"));
            this.resetPropsButton.ControlEvent += (s, a) => ResetLightProps();

            this.resetPositionButton = new Button(Translation.Get("lightsPane", "resetPosition"));
            this.resetPositionButton.ControlEvent += (s, a) => ResetLightPosition();
        }

        protected override void ReloadTranslation()
        {
            this.updating = true;
            this.lightHeader = Translation.Get("lightsPane", "header");
            this.lightTypeGrid.SetItems(Translation.GetArray("lightType", lightTypes));
            this.lightDropdown.SetDropdownItems(this.lightManager.LightNameList);
            this.deleteLightButton.Label = Translation.Get("lightsPane", "delete");
            this.disableToggle.Label = Translation.Get("lightsPane", "disable");
            this.clearLightsButton.Label = Translation.Get("lightsPane", "clear");
            for (LightProp lightProp = LightProp.LightRotX; lightProp <= LightProp.Blue; lightProp++)
            {
                LightSlider[lightProp].Label =
                    Translation.Get(sliderNames[(int)lightProp, 0], sliderNames[(int)lightProp, 1]);
            }
            this.colorToggle.Label = Translation.Get("lightsPane", "colour");
            this.resetPropsButton.Label = Translation.Get("lightsPane", "resetProperties");
            this.resetPositionButton.Label = Translation.Get("lightsPane", "resetPosition");
            this.updating = false;
        }

        private void SetColourMode()
        {
            this.lightManager.SetColourModeActive(this.colorToggle.Value);
            this.environmentManager.BGVisible = !this.colorToggle.Value;
            this.UpdatePane();
        }

        private void ClearLights()
        {
            this.lightManager.ClearLights();
            this.UpdatePane();
        }

        private void SetCurrentLight()
        {
            if (updating) return;
            this.lightManager.SelectedLightIndex = this.lightDropdown.SelectedItemIndex;
            this.UpdatePane();
        }

        private void ResetLightProps()
        {
            this.lightManager.CurrentLight.ResetLightProps();
            this.UpdatePane();
        }

        private void ResetLightPosition()
        {
            this.lightManager.CurrentLight.ResetLightPosition();
        }

        private void AddLight()
        {
            this.lightManager.AddLight();
        }

        private void DeleteCurrentLight()
        {
            this.lightManager.DeleteActiveLight();
        }

        private void SetCurrentLightActive()
        {
            this.lightManager.CurrentLight.IsDisabled = this.disableToggle.Value;
        }

        private void SetCurrentLightType()
        {
            if (updating) return;

            currentLightType = (MPSLightType)this.lightTypeGrid.SelectedItemIndex;

            LightType lightType;
            if (currentLightType == MPSLightType.Normal)
            {
                lightType = LightType.Directional;
            }
            else if (currentLightType == MPSLightType.Spot)
            {
                lightType = LightType.Spot;
            }
            else
            {
                lightType = LightType.Point;
            }

            MPSLight currentLight = lightManager.CurrentLight;
            currentLight.SetLightType(lightType);

            if (lightManager.SelectedLightIndex == 0)
            {
                this.environmentManager.BGVisible = (currentLight.SelectedLightType != MPSLightType.Normal)
                    || !currentLight.IsColourMode;
            }

            this.lightDropdown.SetDropdownItem(lightManager.ActiveLightName);
            this.UpdatePane();
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
            string[] newList = this.lightManager.LightNameList;
            this.lightDropdown.SetDropdownItems(newList, this.lightManager.SelectedLightIndex);
            this.UpdatePane();
        }

        private void UpdateRotation()
        {
            this.updating = true;
            LightProperty prop = this.lightManager.CurrentLight.CurrentLightProperty;
            LightSlider[LightProp.LightRotX].Value = prop.Rotation.eulerAngles.x;
            LightSlider[LightProp.LightRotY].Value = prop.Rotation.eulerAngles.y;
            this.updating = false;
        }

        private void UpdateScale()
        {
            this.updating = true;
            LightSlider[LightProp.SpotAngle].Value = this.lightManager.CurrentLight.CurrentLightProperty.SpotAngle;
            LightSlider[LightProp.Range].Value = this.lightManager.CurrentLight.CurrentLightProperty.Range;
            this.updating = false;
        }

        private void UpdateCurrentLight()
        {
            this.updating = true;
            this.lightDropdown.SelectedItemIndex = this.lightManager.SelectedLightIndex;
            this.updating = false;
            this.UpdatePane();
        }

        public override void UpdatePane()
        {
            this.updating = true;
            MPSLight currentLight = this.lightManager.CurrentLight;
            this.currentLightType = currentLight.SelectedLightType;
            this.lightTypeGrid.SelectedItemIndex = (int)this.currentLightType;
            this.disableToggle.Value = currentLight.IsDisabled;
            this.LightSlider[LightProp.LightRotX].Value = currentLight.Rotation.eulerAngles.x;
            this.LightSlider[LightProp.LightRotY].Value = currentLight.Rotation.eulerAngles.y;
            this.LightSlider[LightProp.Intensity].Value = currentLight.Intensity;
            this.LightSlider[LightProp.ShadowStrength].Value = currentLight.ShadowStrength;
            this.LightSlider[LightProp.Range].Value = currentLight.Range;
            this.LightSlider[LightProp.SpotAngle].Value = currentLight.SpotAngle;
            this.LightSlider[LightProp.Red].Value = currentLight.LightColour.r;
            this.LightSlider[LightProp.Green].Value = currentLight.LightColour.g;
            this.LightSlider[LightProp.Blue].Value = currentLight.LightColour.b;
            this.updating = false;
        }

        public override void Draw()
        {
            bool isMain = this.lightManager.SelectedLightIndex == 0;

            MiscGUI.Header(lightHeader);
            MiscGUI.WhiteLine();

            GUILayout.BeginHorizontal();
            this.lightDropdown.Draw(GUILayout.Width(84));
            this.addLightButton.Draw(GUILayout.ExpandWidth(false));

            GUILayout.FlexibleSpace();
            GUI.enabled = !isMain;
            this.deleteLightButton.Draw(GUILayout.ExpandWidth(false));
            GUI.enabled = true;
            this.clearLightsButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            bool isDisabled = !isMain && this.lightManager.CurrentLight.IsDisabled;
            GUILayout.BeginHorizontal();
            GUI.enabled = !isDisabled;
            this.lightTypeGrid.Draw(GUILayout.ExpandWidth(false));
            if (!isMain)
            {
                GUI.enabled = true;
                this.disableToggle.Draw();
            }
            GUILayout.EndHorizontal();

            GUI.enabled = !isDisabled;

            if (currentLightType != MPSLightType.Point)
            {
                this.LightSlider[LightProp.LightRotX].Draw();
                this.LightSlider[LightProp.LightRotY].Draw();
            }

            this.LightSlider[LightProp.Intensity].Draw();

            if (currentLightType == MPSLightType.Normal)
            {
                this.LightSlider[LightProp.ShadowStrength].Draw();
            }
            else
            {
                this.LightSlider[LightProp.Range].Draw();
            }

            if (currentLightType == MPSLightType.Spot)
            {
                this.LightSlider[LightProp.SpotAngle].Draw();
            }

            GUILayoutOption sliderWidth = MiscGUI.HalfSlider;
            GUILayout.BeginHorizontal();
            this.LightSlider[LightProp.Red].Draw(sliderWidth);
            this.LightSlider[LightProp.Green].Draw(sliderWidth);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.LightSlider[LightProp.Blue].Draw(sliderWidth);
            if ((lightManager.SelectedLightIndex == 0) && (currentLightType == MPSLightType.Normal))
            {
                this.colorToggle.Draw();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            this.resetPropsButton.Draw(GUILayout.ExpandWidth(false));
            this.resetPositionButton.Draw(GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }
    }
}
