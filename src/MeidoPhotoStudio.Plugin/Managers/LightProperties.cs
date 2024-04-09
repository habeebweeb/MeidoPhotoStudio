namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public readonly struct LightProperties
{
    public static readonly Quaternion DefaultRotation = Quaternion.Euler(40f, 180f, 0f);

    public LightProperties()
    {
    }

    public Quaternion Rotation { get; init; } = DefaultRotation;

    public float Intensity { get; init; } = 0.95f;

    public float Range { get; init; } = 10f;

    public float SpotAngle { get; init; } = 50f;

    public float ShadowStrength { get; init; } = 0.098f;

    public Color Colour { get; init; } = Color.white;
}
