namespace MeidoPhotoStudio.Plugin;

public class VignetteEffectManager : IEffectManager
{
    public const string Header = "EFFECT_VIGNETTE";

    private float initialIntensity;
    private float initialBlur;
    private float initialBlurSpread;
    private float initialChromaticAberration;
    private float blur;
    private float blurSpread;
    private float chromaticAberration;
    private float intensity;

    public bool Ready { get; private set; }

    public bool Active { get; private set; }

    public float Intensity
    {
        get => intensity;
        set => intensity = Vignette.intensity = value;
    }

    public float Blur
    {
        get => blur;
        set => blur = Vignette.blur = value;
    }

    public float BlurSpread
    {
        get => blurSpread;
        set => blurSpread = Vignette.blurSpread = value;
    }

    public float ChromaticAberration
    {
        get => chromaticAberration;
        set => chromaticAberration = Vignette.chromaticAberration = value;
    }

    private Vignetting Vignette { get; set; }

    public void Activate()
    {
        if (!Vignette)
        {
            Ready = true;
            Vignette = GameMain.Instance.MainCamera.GetOrAddComponent<Vignetting>();
            Vignette.mode = Vignetting.AberrationMode.Simple;

            initialIntensity = Vignette.intensity;
            initialBlur = Vignette.blur;
            initialBlurSpread = Vignette.blurSpread;
            initialChromaticAberration = Vignette.chromaticAberration;
        }

        SetEffectActive(false);
    }

    public void Deactivate()
    {
        Intensity = initialIntensity;
        Blur = initialBlur;
        BlurSpread = initialBlurSpread;
        ChromaticAberration = initialChromaticAberration;
        Vignette.enabled = false;
        Active = false;
    }

    public void Reset()
    {
        Vignette.intensity = initialIntensity;
        Vignette.blur = initialBlur;
        Vignette.blurSpread = initialBlurSpread;
        Vignette.chromaticAberration = initialChromaticAberration;
    }

    public void SetEffectActive(bool active)
    {
        Vignette.enabled = active;

        if (Active = active)
        {
            Vignette.intensity = Intensity;
            Vignette.blur = Blur;
            Vignette.blurSpread = BlurSpread;
            Vignette.chromaticAberration = ChromaticAberration;
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
