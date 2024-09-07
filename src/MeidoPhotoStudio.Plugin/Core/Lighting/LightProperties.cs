namespace MeidoPhotoStudio.Plugin.Core.Lighting;

public readonly record struct LightProperties(
    Quaternion Rotation, float Intensity, float Range, float SpotAngle, float ShadowStrength, Color Colour)
{
    public static readonly Quaternion DefaultRotation = Quaternion.Euler(40f, 180f, 0f);

    public LightProperties()
        : this(DefaultRotation, 0.95f, 10f, 50f, 0.098f, Color.white)
    {
    }
}
