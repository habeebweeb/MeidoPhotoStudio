namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class BloomController(UnityEngine.Camera camera) : EffectControllerBase(camera)
{
    private BloomBackup initialBloomSettings;
    private Bloom bloom;
    private bool active = true;
    private int initialGameBloomValue;
    private int bloomValue;

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

            GameMain.Instance.CMSystem.BloomValue = active ? bloomValue : 0;

            base.Active = value;
        }
    }

    public int BloomValue
    {
        get => bloomValue;
        set
        {
            bloomValue = value;

            if (active)
                GameMain.Instance.CMSystem.BloomValue = bloomValue;

            RaisePropertyChanged(nameof(BloomValue));
        }
    }

    public int BlurIterations
    {
        get => Bloom.bloomBlurIterations;
        set
        {
            Bloom.bloomBlurIterations = value;

            RaisePropertyChanged(nameof(BlurIterations));
        }
    }

    public Color BloomThresholdColour
    {
        get => Bloom.bloomThreshholdColor;
        set
        {
            Bloom.bloomThreshholdColor = value;

            RaisePropertyChanged(nameof(BloomThresholdColour));
        }
    }

    public bool HDR
    {
        get => Bloom.hdr is Bloom.HDRBloomMode.On;
        set
        {
            Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;

            RaisePropertyChanged(nameof(HDR));
        }
    }

    private Bloom Bloom
    {
        get
        {
            if (bloom)
                return bloom;

            bloom = GetOrAddEffect<Bloom>();
            initialBloomSettings = BloomBackup.Create(bloom);

            return bloom;
        }
    }

    public override void Reset() =>
        ApplyBackup(initialBloomSettings);

    protected override void Activate()
    {
        base.Activate();

        initialGameBloomValue = GameMain.Instance.CMSystem.BloomValue;

        BloomValue = initialGameBloomValue;

        Active = true;
    }

    protected override void Deactivate()
    {
        base.Deactivate();

        ApplyBackup(initialBloomSettings);

        GameMain.Instance.CMSystem.BloomValue = initialGameBloomValue;
    }

    private void ApplyBackup(BloomBackup backup) =>
        (BloomValue, BlurIterations, BloomThresholdColour, HDR) = backup;

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
