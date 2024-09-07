using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Effects;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class EffectPane<T> : BasePane
    where T : EffectControllerBase
{
    protected readonly Toggle effectActiveToggle;
    protected readonly Button resetEffectButton;

    public EffectPane(T effectController)
    {
        Effect = effectController ?? throw new ArgumentNullException(nameof(effectController));

        effectActiveToggle = new(Translation.Get("effectsPane", "onToggle"));
        effectActiveToggle.ControlEvent += OnEffectActiveToggleChanged;

        resetEffectButton = new(Translation.Get("effectsPane", "reset"));
        resetEffectButton.ControlEvent += OnResetEffectButtonPushed;

        Effect.PropertyChanged += OnEffectPropertyChanged;
    }

    protected T Effect { get; }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        effectActiveToggle.Draw();

        GUILayout.FlexibleSpace();

        GUI.enabled = Effect.Active;

        resetEffectButton.Draw();

        GUILayout.EndHorizontal();
    }

    protected virtual void OnEffectPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EffectControllerBase.Active))
            effectActiveToggle.SetEnabledWithoutNotify(((EffectControllerBase)sender).Active);
    }

    protected override void ReloadTranslation()
    {
        effectActiveToggle.Label = Translation.Get("effectsPane", "onToggle");
        resetEffectButton.Label = Translation.Get("effectsPane", "reset");
    }

    private void OnEffectActiveToggleChanged(object sender, EventArgs e) =>
        Effect.Active = ((Toggle)sender).Value;

    private void OnResetEffectButtonPushed(object sender, EventArgs e) =>
        Effect.Reset();
}