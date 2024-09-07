namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class VignetteController(UnityEngine.Camera camera) : EffectControllerBase
{
    private readonly UnityEngine.Camera camera = camera ? camera : throw new ArgumentNullException(nameof(camera));

    private VignetteBackup initialVignetteSettings;
    private Vignetting vignette;

    public override bool Active
    {
        get => Vignette.enabled;
        set
        {
            if (value == Active)
                return;

            Vignette.enabled = value;

            base.Active = value;
        }
    }

    public float Intensity
    {
        get => Vignette.intensity;
        set
        {
            Vignette.intensity = value;

            RaisePropertyChanged(nameof(Intensity));
        }
    }

    public float Blur
    {
        get => Vignette.blur;
        set
        {
            Vignette.blur = value;

            RaisePropertyChanged(nameof(Blur));
        }
    }

    public float BlurSpread
    {
        get => Vignette.blurSpread;
        set
        {
            Vignette.blurSpread = value;

            RaisePropertyChanged(nameof(BlurSpread));
        }
    }

    public float ChromaticAberration
    {
        get => Vignette.chromaticAberration;
        set
        {
            Vignette.chromaticAberration = value;

            RaisePropertyChanged(nameof(ChromaticAberration));
        }
    }

    private Vignetting Vignette
    {
        get
        {
            if (vignette)
                return vignette;

            vignette = camera.GetOrAddComponent<Vignetting>();
            vignette.mode = Vignetting.AberrationMode.Simple;

            initialVignetteSettings = VignetteBackup.Create(vignette);

            return vignette;
        }
    }

    public override void Reset() =>
        ApplyBackup(initialVignetteSettings);

    private void ApplyBackup(VignetteBackup backup) =>
        (Intensity, Blur, BlurSpread, ChromaticAberration) = backup;

    private readonly record struct VignetteBackup(
        float Intensity, float Blur, float BlurSpread, float ChromaticAberration)
    {
        public static VignetteBackup Create(Vignetting vignette) =>
            new(vignette.intensity, vignette.blur, vignette.blurSpread, vignette.chromaticAberration);
    }
}
