namespace MeidoPhotoStudio.Plugin.Core.Effects;

public class SepiaToneController(UnityEngine.Camera camera) : EffectControllerBase(camera)
{
    private SepiaToneEffect sepiaTone;

    public override bool Active
    {
        get => SepiaTone.enabled;
        set
        {
            if (value == Active)
                return;

            SepiaTone.enabled = value;

            base.Active = value;
        }
    }

    private SepiaToneEffect SepiaTone
    {
        get
        {
            if (sepiaTone)
                return sepiaTone;

            sepiaTone = GetOrAddEffect<SepiaToneEffect>();

            if (!sepiaTone.shader)
                sepiaTone.shader = Shader.Find("Hidden/Sepiatone Effect");

            return sepiaTone;
        }
    }

    public override void Reset() =>
        Active = false;
}
