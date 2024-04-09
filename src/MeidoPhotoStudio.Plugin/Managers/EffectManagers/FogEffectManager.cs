namespace MeidoPhotoStudio.Plugin;

public class FogEffectManager : IEffectManager
{
    public const string Header = "EFFECT_FOG";

    private readonly float initialDistance = 4f;
    private readonly float initialDensity = 1f;
    private readonly float initialHeightScale = 1f;
    private readonly float initialHeight = 0f;
    private readonly Color initialColour = Color.white;

    private float distance;
    private float density;
    private float heightScale;
    private float height;
    private Color fogColour;

    public bool Ready { get; private set; }

    public bool Active { get; private set; }

    public float Distance
    {
        get => distance;
        set => distance = Fog.startDistance = value;
    }

    public float Density
    {
        get => density;
        set => density = Fog.globalDensity = value;
    }

    public float HeightScale
    {
        get => heightScale;
        set => heightScale = Fog.heightScale = value;
    }

    public float Height
    {
        get => height;
        set => height = Fog.height = value;
    }

    public float FogColourRed
    {
        get => FogColour.r;
        set
        {
            var fogColour = FogColour;

            FogColour = new(value, fogColour.g, fogColour.b);
        }
    }

    public float FogColourGreen
    {
        get => FogColour.g;
        set
        {
            var fogColour = FogColour;

            FogColour = new(fogColour.r, value, fogColour.b);
        }
    }

    public float FogColourBlue
    {
        get => FogColour.b;
        set
        {
            var fogColour = FogColour;

            FogColour = new(fogColour.r, fogColour.g, value);
        }
    }

    public Color FogColour
    {
        get => fogColour;
        set => fogColour = Fog.globalFogColor = value;
    }

    private GlobalFog Fog { get; set; }

    public void Activate()
    {
        if (!Fog)
        {
            Ready = true;
            Fog = GameMain.Instance.MainCamera.GetOrAddComponent<GlobalFog>();

            if (!Fog.fogShader)
                Fog.fogShader = Shader.Find("Hidden/GlobalFog");

            Distance = initialDistance;
            Density = initialDensity;
            HeightScale = initialHeightScale;
            Height = initialHeight;
            FogColour = initialColour;
        }

        SetEffectActive(false);
    }

    public void Deactivate()
    {
        Distance = initialDistance;
        Density = initialDensity;
        HeightScale = initialHeightScale;
        Height = initialHeight;
        FogColour = initialColour;
        Fog.enabled = false;
        Active = false;
    }

    public void Reset()
    {
        Fog.startDistance = initialDistance;
        Fog.globalDensity = initialDensity;
        Fog.heightScale = initialHeightScale;
        Fog.height = initialHeight;
        Fog.globalFogColor = initialColour;
    }

    public void SetEffectActive(bool active)
    {
        Fog.enabled = active;

        if (Active = active)
        {
            Fog.startDistance = Distance;
            Fog.globalDensity = Density;
            Fog.heightScale = HeightScale;
            Fog.height = Height;
            Fog.globalFogColor = FogColour;
        }
        else
        {
            Reset();
        }
    }

    public void Update()
    {
    }
}
