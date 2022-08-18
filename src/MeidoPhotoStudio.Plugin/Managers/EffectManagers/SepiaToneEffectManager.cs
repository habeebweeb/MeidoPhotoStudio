using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class SepiaToneEffectManager : IEffectManager
{
    public const string Header = "EFFECT_SEPIA";

    public bool Ready { get; private set; }

    public bool Active { get; private set; }

    private SepiaToneEffect SepiaTone { get; set; }

    public void Activate()
    {
        if (!SepiaTone)
        {
            Ready = true;
            SepiaTone = GameMain.Instance.MainCamera.GetOrAddComponent<SepiaToneEffect>();

            if (!SepiaTone.shader)
                SepiaTone.shader = Shader.Find("Hidden/Sepiatone Effect");
        }

        SetEffectActive(false);
    }

    public void Deactivate() =>
        SetEffectActive(false);

    public void SetEffectActive(bool active) =>
        SepiaTone.enabled = Active = active;

    public void Reset()
    {
    }

    public void Update()
    {
    }
}
