namespace MeidoPhotoStudio.Plugin;

public class EffectsPane : BasePane
{
    private readonly Dictionary<string, BasePane> effectPanes = new();
    private readonly List<string> effectList = new();
    private readonly SelectionGrid effectToggles;

    private BasePane currentEffectPane;

    public EffectsPane()
    {
        effectToggles = new(new[] { "dummy" /* thicc */ });
        effectToggles.ControlEvent += (_, _) =>
            SetEffectPane(effectList[effectToggles.SelectedItemIndex]);
    }

    public BasePane this[string effectUI]
    {
        private get => effectPanes[effectUI];
        set
        {
            effectPanes[effectUI] = value;
            effectList.Add(effectUI);
            effectToggles.SetItems(Translation.GetArray("effectsPane", effectList), 0);
        }
    }

    public override void UpdatePane() =>
        currentEffectPane.UpdatePane();

    public override void Draw()
    {
        MpsGui.Header("Effects");
        MpsGui.WhiteLine();
        effectToggles.Draw();
        MpsGui.BlackLine();
        currentEffectPane.Draw();
    }

    protected override void ReloadTranslation() =>
        effectToggles.SetItems(Translation.GetArray("effectsPane", effectList));

    private void SetEffectPane(string effectUI)
    {
        currentEffectPane = effectPanes[effectUI];
        currentEffectPane.UpdatePane();
    }
}
