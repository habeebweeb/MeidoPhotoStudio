using System;
using UnityEngine;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    internal class DragPointLight : DragPointGeneral
    {
        public static EnvironmentManager EnvironmentManager { private get; set; }
        private Light light;
        public enum MPSLightType
        {
            Normal, Spot, Point, Disabled
        }
        public enum LightProp
        {
            LightRotX, LightRotY, Intensity, ShadowStrength, SpotAngle, Range, Red, Green, Blue
        }

        public bool IsActiveLight { get; set; } = false;
        public string Name { get; private set; } = String.Empty;
        public bool IsMain { get; set; } = false;
        public MPSLightType SelectedLightType { get; private set; } = MPSLightType.Normal;
        public LightProperty CurrentLightProperty => LightProperties[(int)SelectedLightType];
        private readonly LightProperty[] LightProperties = new LightProperty[]
        {
            new LightProperty(),
            new LightProperty(),
            new LightProperty()
        };
        private bool isDisabled = false;
        public bool IsDisabled
        {
            get => isDisabled;
            set
            {
                isDisabled = value;
                light.gameObject.SetActive(!isDisabled);
            }
        }
        private bool isColourMode = false;
        public bool IsColourMode
        {
            get => IsMain && isColourMode && SelectedLightType == MPSLightType.Normal;
            set
            {
                if (!IsMain) return;
                light.color = value ? Color.white : LightColour;
                camera.backgroundColor = value ? LightColour : Color.black;
                isColourMode = value;
                LightColour = isColourMode ? camera.backgroundColor : light.color;
                EnvironmentManager.BGVisible = !IsColourMode;
            }
        }
        public Quaternion Rotation
        {
            get => CurrentLightProperty.Rotation;
            set => light.transform.rotation = CurrentLightProperty.Rotation = value;
        }
        public float Intensity
        {
            get => CurrentLightProperty.Intensity;
            set => light.intensity = CurrentLightProperty.Intensity = value;
        }
        public float Range
        {
            get => CurrentLightProperty.Range;
            set => light.range = CurrentLightProperty.Range = value;
        }
        public float SpotAngle
        {
            get => CurrentLightProperty.SpotAngle;
            set
            {
                light.spotAngle = CurrentLightProperty.SpotAngle = value;
                light.transform.localScale = Vector3.one * value;
            }
        }
        public float ShadowStrength
        {
            get => CurrentLightProperty.ShadowStrength;
            set => light.shadowStrength = CurrentLightProperty.ShadowStrength = value;
        }
        public float LightColorRed
        {
            get => IsColourMode ? camera.backgroundColor.r : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : light.color;
                LightColour = new Color(value, color.g, color.b);
            }
        }
        public float LightColorGreen
        {
            get => IsColourMode ? camera.backgroundColor.g : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : light.color;
                LightColour = new Color(color.r, value, color.b);
            }
        }
        public float LightColorBlue
        {
            get => IsColourMode ? camera.backgroundColor.b : CurrentLightProperty.LightColour.r;
            set
            {
                Color color = IsColourMode ? camera.backgroundColor : light.color;
                LightColour = new Color(color.r, color.g, value);
            }
        }
        public Color LightColour
        {
            get => IsColourMode ? camera.backgroundColor : CurrentLightProperty.LightColour;
            set
            {
                Color colour = CurrentLightProperty.LightColour = value;
                if (IsColourMode) camera.backgroundColor = colour;
                else light.color = colour;
            }
        }

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            foreach (LightProperty lightProperty in LightProperties)
            {
                lightProperty.Serialize(binaryWriter);
            }
            binaryWriter.WriteVector3(MyObject.position);
            binaryWriter.Write((int)SelectedLightType);
            binaryWriter.Write(IsColourMode);
            binaryWriter.Write(IsDisabled);
        }

        public void Deserialize(System.IO.BinaryReader binaryReader)
        {
            for (int i = 0; i < LightProperties.Length; i++)
            {
                LightProperties[i] = LightProperty.Deserialize(binaryReader);
            }
            MyObject.position = binaryReader.ReadVector3();
            SetLightType((MPSLightType)binaryReader.ReadInt32());
            IsColourMode = binaryReader.ReadBoolean();
            IsDisabled = binaryReader.ReadBoolean();
        }

        public static void SetLightProperties(Light light, LightProperty prop)
        {
            light.transform.rotation = prop.Rotation;
            light.intensity = prop.Intensity;
            light.range = prop.Range;
            light.spotAngle = prop.SpotAngle;
            light.shadowStrength = prop.ShadowStrength;
            light.color = prop.LightColour;
            if (light.type == LightType.Spot) light.transform.localScale = Vector3.one * prop.SpotAngle;
            else if (light.type == LightType.Point) light.transform.localScale = Vector3.one * prop.Range;
        }

        public override void Set(Transform myObject)
        {
            base.Set(myObject);
            light = myObject.gameObject.GetOrAddComponent<Light>();

            light.transform.position = LightProperty.DefaultPosition;
            light.transform.rotation = LightProperty.DefaultRotation;

            SetLightType(MPSLightType.Normal);
            ScaleFactor = 50f;
        }

        protected override void OnDestroy()
        {
            if (!IsMain) Destroy(light.gameObject);
            base.OnDestroy();
        }

        protected override void OnRotate()
        {
            CurrentLightProperty.Rotation = light.transform.rotation;
            base.OnRotate();
        }

        protected override void OnScale()
        {
            float value = light.transform.localScale.x;
            if (SelectedLightType == MPSLightType.Point) Range = value;
            else if (SelectedLightType == MPSLightType.Spot) SpotAngle = value;
            base.OnScale();
        }

        protected override void ApplyDragType()
        {
            if (Selecting || Moving) ApplyProperties(true, true, false);
            else if (SelectedLightType != MPSLightType.Point && Rotating) ApplyProperties(true, true, false);
            else if (SelectedLightType != MPSLightType.Normal && Scaling) ApplyProperties(true, true, false);
            else if (!IsMain && Deleting) ApplyProperties(true, true, false);
            else ApplyProperties(false, false, false);

            ApplyColours();
        }

        public void SetLightType(MPSLightType type)
        {
            LightType lightType = LightType.Directional;

            string name = "normal";
            SelectedLightType = type;

            if (type == MPSLightType.Spot)
            {
                lightType = LightType.Spot;
                name = "spot";
            }
            else if (type == MPSLightType.Point)
            {
                lightType = LightType.Point;
                name = "point";
            }

            light.type = lightType;
            Name = IsMain ? "main" : name;

            if (IsMain)
            {
                EnvironmentManager.BGVisible = !(IsColourMode && SelectedLightType == MPSLightType.Normal);
            }

            SetProps();
            ApplyDragType();
        }

        public void SetRotation(float x, float y) => Rotation = Quaternion.Euler(x, y, Rotation.eulerAngles.z);

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

        public void ResetLightPosition() => light.transform.position = LightProperty.DefaultPosition;

        private void SetProps()
        {
            SetLightProperties(light, CurrentLightProperty);
            if (IsColourMode)
            {
                light.color = Color.white;
                camera.backgroundColor = CurrentLightProperty.LightColour;
            }
        }
    }

    internal class LightProperty
    {
        public static readonly Vector3 DefaultPosition = new Vector3(0f, 1.9f, 0.4f);
        public static readonly Quaternion DefaultRotation = Quaternion.Euler(40f, 180f, 0f);
        public Quaternion Rotation { get; set; } = DefaultRotation;
        public float Intensity { get; set; } = 0.95f;
        public float Range { get; set; } = GameMain.Instance.MainLight.GetComponent<Light>().range;
        public float SpotAngle { get; set; } = 50f;
        public float ShadowStrength { get; set; } = 0.10f;
        public Color LightColour { get; set; } = Color.white;

        public void Serialize(System.IO.BinaryWriter binaryWriter)
        {
            binaryWriter.WriteQuaternion(Rotation);
            binaryWriter.Write(Intensity);
            binaryWriter.Write(Range);
            binaryWriter.Write(SpotAngle);
            binaryWriter.Write(ShadowStrength);
            binaryWriter.WriteColour(LightColour);
        }

        public static LightProperty Deserialize(System.IO.BinaryReader binaryReader)
        {
            return new LightProperty()
            {
                Rotation = binaryReader.ReadQuaternion(),
                Intensity = binaryReader.ReadSingle(),
                Range = binaryReader.ReadSingle(),
                SpotAngle = binaryReader.ReadSingle(),
                ShadowStrength = binaryReader.ReadSingle(),
                LightColour = binaryReader.ReadColour()
            };
        }
    }
}
