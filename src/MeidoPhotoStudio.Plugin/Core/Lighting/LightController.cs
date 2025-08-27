using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public class LightController : INotifyPropertyChanged, IObservableTransform
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

        lightProperties[LightPropertiesIndex(LightType.Directional)] = LightProperties.FromLight(Light);

        Type = LightType.Directional;
        InitialTransform = new(Light.transform);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public event EventHandler<KeyedPropertyChangeEventArgs<LightType>> ChangedLightType;

    public event EventHandler<TransformChangeEventArgs> ChangedTransform;

    public TransformBackup InitialTransform { get; }

    public Light Light { get; }

    public Transform Transform =>
        Light ? Light.transform : null;

    public bool Enabled
    {
        get => Light && Light.enabled;
        set
        {
            if (!Light)
                return;

            Light.enabled = value;

            RaisePropertyChanged(nameof(Enabled));
        }
    }

    public Vector3 Position
    {
        get => Light ? Light.transform.position : Vector3.zero;
        set
        {
            if (!Light)
                return;

            Light.transform.position = value;
        }
    }

    public Quaternion Rotation
    {
        get => Light ? Light.transform.rotation : Quaternion.identity;
        set
        {
            if (!Light)
                return;

            Light.transform.rotation = value;
            CurrentLightProperties = CurrentLightProperties with { Rotation = value };
        }
    }

    public float Intensity
    {
        get => Light ? Light.intensity : 0f;
        set
        {
            if (!Light)
                return;

            Light.intensity = value;
            CurrentLightProperties = CurrentLightProperties with { Intensity = value };

            RaisePropertyChanged(nameof(Intensity));
        }
    }

    public float Range
    {
        get => Light ? Light.range : 0f;
        set
        {
            if (!Light)
                return;

            Light.range = value;
            CurrentLightProperties = CurrentLightProperties with { Range = value };

            RaisePropertyChanged(nameof(Range));
        }
    }

    public float SpotAngle
    {
        get => Light ? Light.spotAngle : 0f;
        set
        {
            if (!Light)
                return;

            Light.spotAngle = value;
            CurrentLightProperties = CurrentLightProperties with { SpotAngle = value };

            RaisePropertyChanged(nameof(SpotAngle));
        }
    }

    public float ShadowStrength
    {
        get => Light ? Light.shadowStrength : 0f;
        set
        {
            if (!Light)
                return;

            Light.shadowStrength = value;
            CurrentLightProperties = CurrentLightProperties with { ShadowStrength = value };

            RaisePropertyChanged(nameof(ShadowStrength));
        }
    }

    public Color Colour
    {
        get => Light ? Light.color : Color.white;
        set
        {
            if (!Light)
                return;

            Light.color = value;
            CurrentLightProperties = CurrentLightProperties with { Colour = value };

            RaisePropertyChanged(nameof(Colour));
        }
    }

    public LightType Type
    {
        get => Light ? Light.type : LightType.Directional;
        set
        {
            if (!ValidLightType(value))
                throw new NotSupportedException($"{value} is not supported");

            if (!Light)
                return;

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

    internal void Destroy()
    {
        if (!Light)
            return;

        transformWatcher.Unsubscribe(Light.transform);
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

    private void RaiseTransformChanged(TransformChangeEventArgs args)
    {
        var type = args.Type;

        if (type.HasFlag(TransformChangeEventArgs.TransformType.Rotation))
            RaisePropertyChanged(nameof(Rotation));
        else if (type.HasFlag(TransformChangeEventArgs.TransformType.Position))
            RaisePropertyChanged(nameof(Position));

        ChangedTransform?.Invoke(this, args);
    }

    private void RaisePropertyChanged(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        PropertyChanged?.Invoke(this, new(name));
    }
}
