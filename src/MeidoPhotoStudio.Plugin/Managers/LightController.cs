using System;
using System.ComponentModel;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightController
{
    public static readonly Vector3 DefaultPosition = new(0f, 1.9f, 0.4f);

    private readonly LightProperties[] lightProperties;

    private int currentLightPropertiesIndex;

    public LightController(Light light)
    {
        Light = light ? light : throw new ArgumentNullException(nameof(light));
        lightProperties = new LightProperties[] { new(), new(), new() };

        Type = LightType.Directional;

        Apply(CurrentLightProperties);
    }

    public event EventHandler ChangedProperty;

    public Light Light { get; }

    public bool Enabled
    {
        get => Light.enabled;
        set
        {
            Light.enabled = value;

            RaisePropertyChanged();
        }
    }

    public Vector3 Position
    {
        get => Light.transform.position;
        set
        {
            Light.transform.position = value;

            RaisePropertyChanged();
        }
    }

    public Quaternion Rotation
    {
        get => Light.transform.rotation;
        set
        {
            Light.transform.rotation = value;
            CurrentLightProperties = CurrentLightProperties with { Rotation = value };

            RaisePropertyChanged();
        }
    }

    public float Intensity
    {
        get => Light.intensity;
        set
        {
            Light.intensity = value;
            CurrentLightProperties = CurrentLightProperties with { Intensity = value };

            RaisePropertyChanged();
        }
    }

    public float Range
    {
        get => Light.range;
        set
        {
            Light.range = value;
            CurrentLightProperties = CurrentLightProperties with { Range = value };

            RaisePropertyChanged();
        }
    }

    public float SpotAngle
    {
        get => Light.spotAngle;
        set
        {
            Light.spotAngle = value;
            CurrentLightProperties = CurrentLightProperties with { SpotAngle = value };

            RaisePropertyChanged();
        }
    }

    public float ShadowStrength
    {
        get => Light.shadowStrength;
        set
        {
            Light.shadowStrength = value;
            CurrentLightProperties = CurrentLightProperties with { ShadowStrength = value };

            RaisePropertyChanged();
        }
    }

    public Color Colour
    {
        get => Light.color;
        set
        {
            Light.color = value;
            CurrentLightProperties = CurrentLightProperties with { Colour = value };

            RaisePropertyChanged();
        }
    }

    public LightType Type
    {
        get => Light.type;
        set
        {
            if (!ValidLightType(value))
                throw new NotSupportedException($"{value} is not supported");

            Light.type = value;
            currentLightPropertiesIndex = LightPropertiesIndex(value);

            Apply(CurrentLightProperties);

            RaisePropertyChanged();
        }
    }

    private LightProperties CurrentLightProperties
    {
        get => lightProperties[currentLightPropertiesIndex];
        set
        {
            lightProperties[currentLightPropertiesIndex] = value;

            RaisePropertyChanged();
        }
    }

    public LightProperties this[LightType lightType]
    {
        get => lightProperties[LightPropertiesIndex(lightType)];
        set
        {
            if (!ValidLightType(lightType))
                throw new NotSupportedException($"{lightType} is not supported");

            var lightPropertiesIndex = LightPropertiesIndex(lightType);

            lightProperties[lightPropertiesIndex] = value;

            if (lightPropertiesIndex == currentLightPropertiesIndex)
                Apply(lightProperties[lightPropertiesIndex]);

            RaisePropertyChanged();
        }
    }

    public void Apply(LightProperties lightProperties)
    {
        Light.transform.rotation = lightProperties.Rotation;
        Light.intensity = lightProperties.Intensity;
        Light.range = lightProperties.Range;
        Light.spotAngle = lightProperties.SpotAngle;
        Light.shadowStrength = lightProperties.ShadowStrength;
        Light.color = lightProperties.Colour;
    }

    public void ResetCurrentLightProperties()
    {
        CurrentLightProperties = new();

        Apply(CurrentLightProperties);
    }

    public void ResetAllLightProperties()
    {
        for (var i = 0; i < lightProperties.Length; i++)
        {
            lightProperties[i] = new();

            if (i == currentLightPropertiesIndex)
                Apply(lightProperties[i]);
        }
    }

    private static int LightPropertiesIndex(LightType lightType) =>
        lightType switch
        {
            LightType.Directional => 0,
            LightType.Spot => 1,
            LightType.Point => 2,
            LightType.Area => throw new NotSupportedException($"{nameof(LightType.Area)} is not supported"),
            _ => throw new InvalidEnumArgumentException(nameof(lightType), (int)lightType, typeof(LightType)),
        };

    private static bool ValidLightType(LightType lightType) =>
        lightType is LightType.Directional or LightType.Spot or LightType.Point;

    private void RaisePropertyChanged() =>
        ChangedProperty?.Invoke(this, EventArgs.Empty);
}
