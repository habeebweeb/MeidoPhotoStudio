using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightController : INotifyPropertyChanged
{
    public static readonly Vector3 DefaultPosition = new(0f, 1.9f, 0.4f);

    private readonly LightProperties[] lightProperties = [new(), new(), new()];
    private readonly TransformWatcher transformWatcher;

    private int currentLightPropertiesIndex;

    public LightController(Light light, TransformWatcher transformWatcher)
    {
        Light = light ? light : throw new ArgumentNullException(nameof(light));
        this.transformWatcher = transformWatcher ? transformWatcher : throw new ArgumentNullException(nameof(transformWatcher));
        this.transformWatcher.Subscribe(Light.transform, RaiseTransformChanged);

        Type = LightType.Directional;

        Apply(CurrentLightProperties);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event EventHandler<KeyedPropertyChangeEventArgs<LightType>> ChangedLightType;

    public Light Light { get; }

    public bool Enabled
    {
        get => Light.enabled;
        set
        {
            Light.enabled = value;

            RaisePropertyChanged(nameof(Enabled));
        }
    }

    public Vector3 Position
    {
        get => Light.transform.position;
        set => Light.transform.position = value;
    }

    public Quaternion Rotation
    {
        get => Light.transform.rotation;
        set
        {
            Light.transform.rotation = value;
            CurrentLightProperties = CurrentLightProperties with { Rotation = value };
        }
    }

    public float Intensity
    {
        get => Light.intensity;
        set
        {
            Light.intensity = value;
            CurrentLightProperties = CurrentLightProperties with { Intensity = value };

            RaisePropertyChanged(nameof(Intensity));
        }
    }

    public float Range
    {
        get => Light.range;
        set
        {
            Light.range = value;
            CurrentLightProperties = CurrentLightProperties with { Range = value };

            RaisePropertyChanged(nameof(Range));
        }
    }

    public float SpotAngle
    {
        get => Light.spotAngle;
        set
        {
            Light.spotAngle = value;
            CurrentLightProperties = CurrentLightProperties with { SpotAngle = value };

            RaisePropertyChanged(nameof(SpotAngle));
        }
    }

    public float ShadowStrength
    {
        get => Light.shadowStrength;
        set
        {
            Light.shadowStrength = value;
            CurrentLightProperties = CurrentLightProperties with { ShadowStrength = value };

            RaisePropertyChanged(nameof(ShadowStrength));
        }
    }

    public Color Colour
    {
        get => Light.color;
        set
        {
            Light.color = value;
            CurrentLightProperties = CurrentLightProperties with { Colour = value };

            RaisePropertyChanged(nameof(Colour));
        }
    }

    public LightType Type
    {
        get => Light.type;
        set
        {
            if (!ValidLightType(value))
                throw new NotSupportedException($"{value} is not supported");

            if (Light.type == value)
                return;

            Light.type = value;
            currentLightPropertiesIndex = LightPropertiesIndex(value);

            Apply(CurrentLightProperties);

            ChangedLightType?.Invoke(this, new(value));

            RaisePropertyChanged(nameof(Type));
        }
    }

    private LightProperties CurrentLightProperties
    {
        get => lightProperties[currentLightPropertiesIndex];
        set => lightProperties[currentLightPropertiesIndex] = value;
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
        }
    }

    public void Apply(LightProperties lightProperties)
    {
        Rotation = lightProperties.Rotation;
        Intensity = lightProperties.Intensity;
        Range = lightProperties.Range;
        SpotAngle = lightProperties.SpotAngle;
        ShadowStrength = lightProperties.ShadowStrength;
        Colour = lightProperties.Colour;
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

    internal void Destroy() =>
        transformWatcher.Unsubscribe(Light.transform);

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

    private void RaiseTransformChanged(TransformChangeEventArgs.TransformType type)
    {
        if (type.HasFlag(TransformChangeEventArgs.TransformType.Rotation))
            RaisePropertyChanged(nameof(Rotation));
        else if (type.HasFlag(TransformChangeEventArgs.TransformType.Position))
            RaisePropertyChanged(nameof(Position));
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
