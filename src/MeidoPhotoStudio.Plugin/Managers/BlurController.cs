namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class BlurController(UnityEngine.Camera camera) : EffectControllerBase
{
    private readonly UnityEngine.Camera camera = camera ? camera : throw new ArgumentNullException(nameof(camera));

    private BlurBackup initialBlurSettings;
    private Blur blur;

    public override bool Active
    {
        get => Blur.enabled;
        set
        {
            if (value == Active)
                return;

            Blur.enabled = value;

            base.Active = value;
        }
    }

    public float BlurSize
    {
        get => Blur.blurSize;
        set => Blur.blurSize = value;
    }

    public int BlurIterations
    {
        get => Blur.blurIterations;
        set => Blur.blurIterations = value;
    }

    public int Downsample
    {
        get => Blur.downsample;
        set => Blur.downsample = value;
    }

    private Blur Blur
    {
        get
        {
            if (blur)
                return blur;

            blur = camera.GetOrAddComponent<Blur>();

            initialBlurSettings = BlurBackup.Create(blur);

            return blur;
        }
    }

    public override void Reset() =>
        ApplyBackup(initialBlurSettings);

    private void ApplyBackup(BlurBackup backup) =>
        (BlurSize, BlurIterations, Downsample) = backup;

    private readonly record struct BlurBackup(
        float BlurSize, int BlurIterations, int Downsample)
    {
        public static BlurBackup Create(Blur blur) =>
            new(blur.blurSize, blur.blurIterations, blur.downsample);
    }
}
