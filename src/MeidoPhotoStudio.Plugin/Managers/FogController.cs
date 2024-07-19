namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class FogController(UnityEngine.Camera camera) : EffectControllerBase
{
    private readonly UnityEngine.Camera camera = camera ? camera : throw new ArgumentNullException(nameof(camera));

    private FogBackup initialFogSettings;
    private GlobalFog fog;

    public override bool Active
    {
        get => Fog.enabled;
        set
        {
            if (value == Active)
                return;

            Fog.enabled = value;

            base.Active = value;
        }
    }

    public float Distance
    {
        get => Fog.startDistance;
        set
        {
            Fog.startDistance = value;

            RaisePropertyChanged(nameof(Distance));
        }
    }

    public float Density
    {
        get => Fog.globalDensity;
        set
        {
            Fog.globalDensity = value;

            RaisePropertyChanged(nameof(Density));
        }
    }

    public float HeightScale
    {
        get => Fog.heightScale;
        set
        {
            Fog.heightScale = value;

            RaisePropertyChanged(nameof(HeightScale));
        }
    }

    public float Height
    {
        get => Fog.height;
        set
        {
            Fog.height = value;

            RaisePropertyChanged(nameof(Height));
        }
    }

    public Color FogColour
    {
        get => Fog.globalFogColor;
        set
        {
            Fog.globalFogColor = value;

            RaisePropertyChanged(nameof(FogColour));
        }
    }

    private GlobalFog Fog
    {
        get
        {
            if (fog)
                return fog;

            fog = camera.GetOrAddComponent<GlobalFog>();

            if (!fog.fogShader)
                fog.fogShader = Shader.Find("Hidden/GlobalFog");

            initialFogSettings = FogBackup.Create(fog);

            return fog;
        }
    }

    public override void Reset() =>
        ApplyBackup(initialFogSettings);

    private void ApplyBackup(FogBackup backup) =>
        (Distance, Density, HeightScale, Height, FogColour) = backup;

    private readonly record struct FogBackup(
        float Distance, float Density, float HeightScale, float Height, Color Colour)
    {
        public static FogBackup Create(GlobalFog fog) =>
            new(fog.startDistance, fog.globalDensity, fog.heightScale, fog.height, fog.globalFogColor);
    }
}
