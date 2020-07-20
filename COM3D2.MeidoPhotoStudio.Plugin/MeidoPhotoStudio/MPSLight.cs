using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class MPSLight
    {
        private static Camera camera = GameMain.Instance.MainCamera.GetComponent<Camera>();
        private Light light;
        public DragDogu DragLight { get; private set; }
        public event EventHandler Rotate;
        public event EventHandler Scale;
        public event EventHandler Delete;
        public event EventHandler Select;
        public bool isActiveLight = false;
        public bool IsMain { get; private set; } = false;
        private bool isDisabled = false;
        public string Name { get; private set; }
        public bool IsDisabled
        {
            get => isDisabled;
            set
            {
                this.isDisabled = value;
                this.light.gameObject.SetActive(!this.isDisabled);
            }
        }
        public LightProperty[] LightProperties = new LightProperty[]
        {
            new LightProperty(),
            new LightProperty(),
            new LightProperty()
        };
        private bool isColourMode = false;
        public bool IsColourMode
        {
            get => isColourMode && SelectedLightType == MPSLightType.Normal;
            set
            {
                this.light.color = value ? Color.white : LightColour;
                camera.backgroundColor = value ? LightColour : Color.black;
                this.isColourMode = value;
                LightColour = this.isColourMode ? camera.backgroundColor : light.color;
            }
        }
        public MPSLightType SelectedLightType { get; private set; } = MPSLightType.Normal;
        public LightProperty CurrentLightProperty => LightProperties[(int)SelectedLightType];
        public Quaternion Rotation
        {
            get => CurrentLightProperty.Rotation;
            set => this.light.transform.rotation = CurrentLightProperty.Rotation = value;
        }
        public float Intensity
        {
            get => CurrentLightProperty.Intensity;
            set => this.light.intensity = CurrentLightProperty.Intensity = value;
        }
        public float Range
        {
            get => CurrentLightProperty.Range;
            set => this.light.range = CurrentLightProperty.Range = value;
        }
        public float SpotAngle
        {
            get => CurrentLightProperty.SpotAngle;
            set
            {
                this.light.spotAngle = CurrentLightProperty.SpotAngle = value;
                this.light.transform.localScale = Vector3.one * value;
            }
        }
        public float ShadowStrength
        {
            get => CurrentLightProperty.ShadowStrength;
            set => this.light.shadowStrength = CurrentLightProperty.ShadowStrength = value;
        }
        public float LightColorRed
        {
            get => IsColourMode ? camera.backgroundColor.r : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : this.light.color;
                this.LightColour = new Color(value, color.g, color.b);
            }
        }
        public float LightColorGreen
        {
            get => IsColourMode ? camera.backgroundColor.g : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : this.light.color;
                this.LightColour = new Color(color.r, value, color.b);
            }
        }
        public float LightColorBlue
        {
            get => IsColourMode ? camera.backgroundColor.b : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : this.light.color;
                this.LightColour = new Color(color.r, color.g, value);
            }
        }
        public Color LightColour
        {
            get => IsColourMode ? camera.backgroundColor : CurrentLightProperty.LightColour;
            set
            {
                Color colour = CurrentLightProperty.LightColour = value;
                if (IsColourMode) camera.backgroundColor = colour;
                else this.light.color = colour;
            }
        }
        public enum LightProp
        {
            LightRotX, LightRotY, Intensity, ShadowStrength, SpotAngle, Range, Red, Green, Blue
        }

        public enum MPSLightType
        {
            Normal, Spot, Point, Disabled
        }

        public MPSLight(GameObject lightGo = null, bool isMain = false)
        {
            this.IsMain = isMain;

            GameObject gameobject = lightGo ?? new GameObject();
            this.light = gameobject.GetOrAddComponent<Light>();

            float spotAngle = CurrentLightProperty.SpotAngle;
            this.light.transform.position = LightProperty.DefaultPosition;
            this.light.transform.rotation = LightProperty.DefaultRotation;

            DragLight = BaseDrag.MakeDragPoint<DragDogu>(
                PrimitiveType.Cube, Vector3.one * 0.12f, BaseDrag.LightBlue
            ).Initialize(this.light.gameObject, this.IsMain, CustomGizmo.GizmoMode.World,
                () => this.light.transform.position,
                () => this.light.transform.eulerAngles
            );

            DragLight.scaleFactor = 50f;
            DragLight.Select += (s, a) => this.Select?.Invoke(this, EventArgs.Empty);

            if (!isMain)
            {
                DragLight.Delete += (s, a) => Delete?.Invoke(this, EventArgs.Empty);
            }

            DragLight.SetDragProp(false, false, false);

            SetLightType(LightType.Directional);
        }

        public static void SetLightProperties(Light light, LightProperty prop)
        {
            light.transform.rotation = prop.Rotation;
            light.intensity = prop.Intensity;
            light.range = prop.Range;
            light.spotAngle = prop.SpotAngle;
            light.shadowStrength = prop.ShadowStrength;
            light.color = prop.LightColour;
            if (light.type == LightType.Spot)
            {
                light.transform.localScale = Vector3.one * prop.SpotAngle;
            }
            else if (light.type == LightType.Point)
            {
                light.transform.localScale = Vector3.one * prop.Range;
            }
        }

        public void Destroy()
        {
            DragLight.Rotate -= OnRotate;
            DragLight.Scale -= OnScale;
            GameObject.Destroy(DragLight.gameObject);
            if (!IsMain) GameObject.Destroy(this.light.gameObject);
        }

        public void SetLightType(LightType type)
        {
            DragLight.Rotate -= OnRotate;
            DragLight.Rotate -= OnScale;
            string name = "normal";

            if (type == LightType.Directional)
            {
                SelectedLightType = MPSLightType.Normal;
            }
            else if (type == LightType.Spot)
            {
                name = "spot";
                SelectedLightType = MPSLightType.Spot;
                DragLight.Scale += OnScale;
                DragLight.Rotate += OnRotate;
            }
            else
            {
                name = "point";
                SelectedLightType = MPSLightType.Point;
                DragLight.Scale += OnScale;
            }

            this.light.type = type;

            SetProps();

            this.Name = IsMain ? "main" : name;
        }

        public void SetRotation(float x, float y)
        {
            this.Rotation = Quaternion.Euler(x, y, Rotation.eulerAngles.z);
        }

        public void SetProp(LightProp prop, float value)
        {
            switch (prop)
            {
                case LightProp.Intensity:
                    Intensity = value;
                    break;
                case LightProp.ShadowStrength:
                    ShadowStrength = value;
                    break;
                case LightProp.SpotAngle:
                    SpotAngle = value;
                    break;
                case LightProp.Range:
                    Range = value;
                    break;
                case LightProp.Red:
                    LightColorRed = value;
                    break;
                case LightProp.Green:
                    LightColorGreen = value;
                    break;
                case LightProp.Blue:
                    LightColorBlue = value;
                    break;
            }
        }

        public void ResetLightProps()
        {
            LightProperties[(int)SelectedLightType] = new LightProperty();
            SetProps();
        }

        public void ResetLightPosition()
        {
            this.light.transform.position = LightProperty.DefaultPosition;
        }

        private void SetProps()
        {
            SetLightProperties(this.light, CurrentLightProperty);
            if (IsColourMode)
            {
                this.light.color = Color.white;
                camera.backgroundColor = CurrentLightProperty.LightColour;
            }
        }

        private void OnRotate(object sender, EventArgs args)
        {
            CurrentLightProperty.Rotation = this.light.transform.rotation;
            OnTransformEvent(Rotate);
        }

        private void OnScale(object sender, EventArgs args)
        {
            float value = this.light.transform.localScale.x;
            if (SelectedLightType == MPSLightType.Point) Range = value;
            else if (SelectedLightType == MPSLightType.Spot) SpotAngle = value;

            OnTransformEvent(Scale);
        }

        private void OnTransformEvent(EventHandler handler)
        {
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    internal class LightProperty
    {
        public static readonly Vector3 DefaultPosition = new Vector3(0f, 1.5f, 0.4f);
        public static readonly Quaternion DefaultRotation = Quaternion.Euler(40f, 180f, 0f);
        public Quaternion Rotation { get; set; } = DefaultRotation;
        public float Intensity { get; set; } = 0.95f;
        public float Range { get; set; } = GameMain.Instance.MainLight.GetComponent<Light>().range;
        public float SpotAngle { get; set; } = 50f;
        public float ShadowStrength { get; set; } = 0.10f;
        public Color LightColour { get; set; } = Color.white;
    }
}
