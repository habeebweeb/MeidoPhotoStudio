using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public abstract class EffectPane<T> : BasePane
    where T : IEffectManager
{
    protected readonly Toggle effectToggle;
    protected readonly Button resetEffectButton;

    private bool enabled;

    protected EffectPane(EffectManager effectManager)
    {
        EffectManager = effectManager.Get<T>();

        resetEffectButton = new(Translation.Get("effectsPane", "reset"));
        resetEffectButton.ControlEvent += (_, _) =>
            ResetEffect();

        effectToggle = new(Translation.Get("effectsPane", "onToggle"));
        effectToggle.ControlEvent += (_, _) =>
            Enabled = effectToggle.Value;
    }

    public override bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;

            if (updating)
                return;

            EffectManager.SetEffectActive(enabled);
        }
    }

    protected abstract T EffectManager { get; set; }

    public override void UpdatePane()
    {
        if (!EffectManager.Ready)
            return;

        updating = true;
        effectToggle.Value = EffectManager.Active;
        UpdateControls();
        updating = false;
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();
        effectToggle.Draw();
        GUILayout.FlexibleSpace();
        GUI.enabled = Enabled;
        resetEffectButton.Draw();
        GUILayout.EndHorizontal();
        DrawPane();
        GUI.enabled = true;
    }

    protected override void ReloadTranslation()
    {
        updating = true;
        effectToggle.Label = Translation.Get("effectsPane", "onToggle");
        resetEffectButton.Label = Translation.Get("effectsPane", "reset");
        TranslatePane();
        updating = false;
    }

    protected abstract void TranslatePane();

    protected abstract void UpdateControls();

    protected abstract void DrawPane();

    private void ResetEffect()
    {
        EffectManager.Deactivate();
        EffectManager.SetEffectActive(true);
        UpdatePane();
    }
}
