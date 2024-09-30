using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class EffectsPane : BasePane, IEnumerable<KeyValuePair<EffectsPane.EffectType, BasePane>>
{
    private readonly Dropdown<EffectType> effectTypesDropdown;
    private readonly Dictionary<EffectType, BasePane> effectsPanes = new(EnumEqualityComparer<EffectType>.Instance);
    private readonly PaneHeader paneHeader;

    public EffectsPane()
    {
        effectTypesDropdown = new((type, _) => new LabelledDropdownItem(Translation.Get("effectTypes", type.ToLower())));

        paneHeader = new(Translation.Get("effectsPane", "header"), true);
    }

    public enum EffectType
    {
        Bloom,
        DepthOfField,
        Vignette,
        Fog,
        SepiaTone,
        Blur,
    }

    public BasePane this[EffectType type]
    {
        get => effectsPanes[type];
        set => Add(type, value);
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        DrawDropdown(effectTypesDropdown);

        effectsPanes[effectTypesDropdown.SelectedItem].Draw();
    }

    public IEnumerator<KeyValuePair<EffectType, BasePane>> GetEnumerator() =>
        effectsPanes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public override void SetParent(BaseWindow window)
    {
        base.SetParent(window);

        foreach (var pane in effectsPanes.Values)
            pane.SetParent(parent);
    }

    public void Add(EffectType type, BasePane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        effectsPanes[type] = pane;

        var effects = effectTypesDropdown.Concat(new[] { type });

        effectTypesDropdown.SetItemsWithoutNotify(effects, 0);
    }

    protected override void ReloadTranslation()
    {
        effectTypesDropdown.Reformat();

        paneHeader.Label = Translation.Get("effectsPane", "header");
    }
}
