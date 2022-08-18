using System.Reflection;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BloomEffectManager : IEffectManager
{
    public const string Header = "EFFECT_BLOOM";

    private const float DefaultBloomDefIntensity = 5.7f;

    private static readonly CameraMain Camera = GameMain.Instance.MainCamera;

#pragma warning disable SA1308, SA1310, SA1311

    // TODO: Refactor reflection to using private members directly
    private static readonly float backup_m_fBloomDefIntensity;
    private static readonly FieldInfo m_fBloomDefIntensity = Utility.GetFieldInfo<CameraMain>("m_fBloomDefIntensity");
#pragma warning restore SA1308, SA1310, SA1311

    // CMSystem's bloomValue;
    private static int backupBloomValue;

    private float initialIntensity;
    private int initialBlurIterations;
    private Color initialThresholdColour;
    private Bloom.HDRBloomMode initialHDRBloomMode;
    private int blurIterations;
    private Color bloomThresholdColour;
    private bool bloomHdr;

    private float bloomValue;

    static BloomEffectManager() =>
        backup_m_fBloomDefIntensity = BloomDefIntensity;

    public bool Ready { get; private set; }

    public bool Active { get; private set; }

    public float BloomValue
    {
        get => bloomValue;
        set => GameMain.Instance.CMSystem.BloomValue = (int)(bloomValue = value);
    }

    public int BlurIterations
    {
        get => blurIterations;
        set => blurIterations = Bloom.bloomBlurIterations = value;
    }

    public float BloomThresholdColorRed
    {
        get => BloomThresholdColour.r;
        set
        {
            var colour = Bloom.bloomThreshholdColor;

            BloomThresholdColour = new(value, colour.g, colour.b);
        }
    }

    public float BloomThresholdColorGreen
    {
        get => BloomThresholdColour.g;
        set
        {
            var colour = Bloom.bloomThreshholdColor;

            BloomThresholdColour = new(colour.r, value, colour.b);
        }
    }

    public float BloomThresholdColorBlue
    {
        get => BloomThresholdColour.b;
        set
        {
            var colour = Bloom.bloomThreshholdColor;

            BloomThresholdColour = new(colour.r, colour.g, value);
        }
    }

    public Color BloomThresholdColour
    {
        get => bloomThresholdColour;
        set => bloomThresholdColour = Bloom.bloomThreshholdColor = value;
    }

    public bool BloomHDR
    {
        get => bloomHdr;
        set
        {
            Bloom.hdr = value ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;
            bloomHdr = value;
        }
    }

    private static float BloomDefIntensity
    {
        get => (float)m_fBloomDefIntensity.GetValue(Camera);
        set => m_fBloomDefIntensity.SetValue(Camera, value);
    }

    private Bloom Bloom { get; set; }

    public void Activate()
    {
        if (!Bloom)
        {
            Ready = true;
            Bloom = GameMain.Instance.MainCamera.GetComponent<Bloom>();
            initialIntensity = bloomValue = 50f;
            initialBlurIterations = blurIterations = Bloom.bloomBlurIterations;
            initialThresholdColour = bloomThresholdColour = Bloom.bloomThreshholdColor;
            initialHDRBloomMode = Bloom.hdr;
            bloomHdr = Bloom.hdr is Bloom.HDRBloomMode.On;

            backupBloomValue = GameMain.Instance.CMSystem.BloomValue;
        }

        SetEffectActive(false);
    }

    public void Deactivate()
    {
        BloomValue = initialIntensity;
        BlurIterations = initialBlurIterations;
        BloomThresholdColour = initialThresholdColour;
        BloomHDR = initialHDRBloomMode is Bloom.HDRBloomMode.On;
        BloomHDR = false;
        Active = false;

        BloomDefIntensity = backup_m_fBloomDefIntensity;
        GameMain.Instance.CMSystem.BloomValue = backupBloomValue;
    }

    public void Reset()
    {
        GameMain.Instance.CMSystem.BloomValue = backupBloomValue;
        Bloom.bloomBlurIterations = initialBlurIterations;
        Bloom.bloomThreshholdColor = initialThresholdColour;
        Bloom.hdr = initialHDRBloomMode;

        BloomDefIntensity = backup_m_fBloomDefIntensity;
    }

    public void SetEffectActive(bool active)
    {
        if (Active = active)
        {
            backupBloomValue = GameMain.Instance.CMSystem.BloomValue;
            GameMain.Instance.CMSystem.BloomValue = (int)BloomValue;
            Bloom.bloomBlurIterations = BlurIterations;
            Bloom.bloomThreshholdColor = BloomThresholdColour;
            Bloom.hdr = BloomHDR ? Bloom.HDRBloomMode.On : Bloom.HDRBloomMode.Auto;

            BloomDefIntensity = DefaultBloomDefIntensity;
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
