namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class BloomController(UnityEngine.Camera camera) : EffectControllerBase
{
    private readonly UnityEngine.Camera camera = camera ? camera : throw new ArgumentNullException(nameof(camera));

    private BloomBackup initialBloomSettings;
    private Bloom bloom;
    private bool active;
    private int initialGameBloomValue;
    private int backupBloomValue;

    // NOTE: The game will disable bloom when the bloom intensity is <= 0.01f so effect active needs to handled
    // differently.
    public override bool Active
    {
        get => active;
        set
        {
            if (value == Active)
                return;

            active = value;

            if (active)
            {
                GameMain.Instance.CMSystem.BloomValue = backupBloomValue;
            }
            else
            {
                backupBloomValue = BloomValue;
                GameMain.Instance.CMSystem.BloomValue = 0;
            }

            base.Active = value;
        }
    }

    public int BloomValue
    {
        get => GameMain.Instance.CMSystem.BloomValue;
        set => GameMain.Instance.CMSystem.BloomValue = value;
    }

    public int BlurIterations
    {
        get => Bloom.bloomBlurIterations;
        set => Bloom.bloomBlurIterations = value;
    }

    public Color BloomThresholdColour
    {
        get => Bloom.bloomThreshholdColor;
        set => Bloom.bloomThreshholdColor = value;
    }

    public bool HDR
    {
        get => Bloom.hdr is Bloom.HDRBloomMode.On;
        set => Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
    }

    private Bloom Bloom
    {
        get
        {
            if (bloom)
                return bloom;

            bloom = camera.GetOrAddComponent<Bloom>();
            initialBloomSettings = BloomBackup.Create(bloom);

            return bloom;
        }
    }

    public override void Reset()
    {
        ApplyBackup(initialBloomSettings);
        GameMain.Instance.CMSystem.BloomValue = initialGameBloomValue;
    }

    internal override void Activate()
    {
        base.Activate();

        backupBloomValue = initialGameBloomValue = GameMain.Instance.CMSystem.BloomValue;
    }

    internal override void Deactivate()
    {
        base.Deactivate();

        ApplyBackup(initialBloomSettings);

        GameMain.Instance.CMSystem.BloomValue = initialGameBloomValue;
    }

    private void ApplyBackup(BloomBackup backup) =>
        (GameMain.Instance.CMSystem.BloomValue, BlurIterations, BloomThresholdColour, HDR) = backup;

    private readonly record struct BloomBackup(int BloomValue, int BlurIterations, Color BloomThresholdColour, bool HDR)
    {
        public static BloomBackup Create(Bloom bloom) =>
            new(
                GameMain.Instance.CMSystem.BloomValue,
                bloom.bloomBlurIterations,
                bloom.bloomThreshholdColor,
                bloom.hdr is Bloom.HDRBloomMode.On);
    }
}
