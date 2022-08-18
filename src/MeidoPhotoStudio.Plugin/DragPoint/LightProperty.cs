using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class LightProperty
{
    public static readonly Vector3 DefaultPosition = new(0f, 1.9f, 0.4f);
    public static readonly Quaternion DefaultRotation = Quaternion.Euler(40f, 180f, 0f);

    public Quaternion Rotation { get; set; } = DefaultRotation;

    public float Intensity { get; set; } = 0.95f;

    public float Range { get; set; } = GameMain.Instance.MainLight.GetComponent<Light>().range;

    public float SpotAngle { get; set; } = 50f;

    public float ShadowStrength { get; set; } = 0.10f;

    public Color LightColour { get; set; } = Color.white;
}
