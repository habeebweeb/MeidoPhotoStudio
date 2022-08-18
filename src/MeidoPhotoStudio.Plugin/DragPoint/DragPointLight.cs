using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class DragPointLight : DragPointGeneral
{
    private readonly LightProperty[] lightProperties = new LightProperty[]
    {
        new(), new(), new(),
    };

    private Light light;
    private bool isDisabled;
    private bool isColourMode;

    public enum MPSLightType
    {
        Normal,
        Spot,
        Point,
        Disabled,
    }

    public enum LightProp
    {
        LightRotX,
        LightRotY,
        Intensity,
        ShadowStrength,
        SpotAngle,
        Range,
        Red,
        Green,
        Blue,
    }

    public static EnvironmentManager EnvironmentManager { private get; set; }

    public bool IsActiveLight { get; set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsMain { get; set; }

    public MPSLightType SelectedLightType { get; private set; }

    public LightProperty CurrentLightProperty =>
        lightProperties[(int)SelectedLightType];

    public bool IsDisabled
    {
        get => isDisabled;
        set
        {
            isDisabled = value;
            light.gameObject.SetActive(!isDisabled);
        }
    }

    public bool IsColourMode
    {
        get => IsMain && isColourMode && SelectedLightType is MPSLightType.Normal;
        set
        {
            if (!IsMain)
                return;

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
            var color = IsColourMode ? camera.backgroundColor : light.color;

            LightColour = new(value, color.g, color.b);
        }
    }

    public float LightColorGreen
    {
        get => IsColourMode ? camera.backgroundColor.g : CurrentLightProperty.LightColour.r;
        set
        {
            var color = IsColourMode ? camera.backgroundColor : light.color;

            LightColour = new(color.r, value, color.b);
        }
    }

    public float LightColorBlue
    {
        get => IsColourMode ? camera.backgroundColor.b : CurrentLightProperty.LightColour.r;
        set
        {
            var color = IsColourMode ? camera.backgroundColor : light.color;

            LightColour = new(color.r, color.g, value);
        }
    }

    public Color LightColour
    {
        get => IsColourMode ? camera.backgroundColor : CurrentLightProperty.LightColour;
        set
        {
            var colour = CurrentLightProperty.LightColour = value;

            if (IsColourMode)
                camera.backgroundColor = colour;
            else
                light.color = colour;
        }
    }

    public static void SetLightProperties(Light light, LightProperty prop)
    {
        light.transform.rotation = prop.Rotation;
        light.intensity = prop.Intensity;
        light.range = prop.Range;
        light.spotAngle = prop.SpotAngle;
        light.shadowStrength = prop.ShadowStrength;
        light.color = prop.LightColour;

        if (light.type is LightType.Spot)
            light.transform.localScale = Vector3.one * prop.SpotAngle;
        else if (light.type is LightType.Point)
            light.transform.localScale = Vector3.one * prop.Range;
    }

    public override void Set(Transform myObject)
    {
        base.Set(myObject);

        light = myObject.gameObject.GetOrAddComponent<Light>();

        // TODO: Use trasnform.SetPositionAndRotation
        light.transform.position = LightProperty.DefaultPosition;
        light.transform.rotation = LightProperty.DefaultRotation;

        SetLightType(MPSLightType.Normal);

        ScaleFactor = 50f;
        DefaultRotation = LightProperty.DefaultRotation;
        DefaultPosition = LightProperty.DefaultPosition;
    }

    public void SetLightType(MPSLightType type)
    {
        const string spotName = "spot";
        const string normalName = "normal";
        const string pointName = "point";
        const string mainName = "name";

        var lightType = LightType.Directional;
        var name = normalName;

        SelectedLightType = type;

        if (type is MPSLightType.Spot)
        {
            lightType = LightType.Spot;
            name = spotName;
        }
        else if (type is MPSLightType.Point)
        {
            lightType = LightType.Point;
            name = pointName;
        }

        light.type = lightType;
        Name = IsMain ? mainName : name;

        if (IsMain)
            EnvironmentManager.BGVisible = !(IsColourMode && SelectedLightType is MPSLightType.Normal);

        SetProps();
        ApplyDragType();
    }

    public void SetRotation(float x, float y) =>
        Rotation = Quaternion.Euler(x, y, Rotation.eulerAngles.z);

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
            case LightProp.LightRotX:
            case LightProp.LightRotY:
                // Do nothing
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(prop));
        }
    }

    public void ResetLightProps()
    {
        lightProperties[(int)SelectedLightType] = new();
        SetProps();
    }

    public void ResetLightPosition() =>
        light.transform.position = LightProperty.DefaultPosition;

    protected override void OnDestroy()
    {
        if (!IsMain)
            Destroy(light.gameObject);

        base.OnDestroy();
    }

    protected override void OnRotate()
    {
        CurrentLightProperty.Rotation = light.transform.rotation;

        base.OnRotate();
    }

    protected override void OnScale()
    {
        var value = light.transform.localScale.x;

        if (SelectedLightType is MPSLightType.Point)
            Range = value;
        else if (SelectedLightType is MPSLightType.Spot)
            SpotAngle = value;

        base.OnScale();
    }

    protected override void ApplyDragType()
    {
        if (Selecting || Moving)
            ApplyProperties(true, true, false);
        else if (SelectedLightType is not MPSLightType.Point && Rotating)
            ApplyProperties(true, true, false);
        else if (SelectedLightType is not MPSLightType.Normal && Scaling)
            ApplyProperties(true, true, false);
        else if (!IsMain && Deleting)
            ApplyProperties(true, true, false);
        else
            ApplyProperties(false, false, false);

        ApplyColours();
    }

    private void SetProps()
    {
        SetLightProperties(light, CurrentLightProperty);

        if (!IsColourMode)
            return;

        light.color = Color.white;
        camera.backgroundColor = CurrentLightProperty.LightColour;
    }
}
