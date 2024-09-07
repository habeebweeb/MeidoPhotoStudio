using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropsPane : BasePane, IEnumerable<KeyValuePair<PropsPane.PropCategory, BasePane>>
{
    private readonly Dropdown<PropCategory> propTypeDropdown;
    private readonly Dictionary<PropCategory, BasePane> propPanes = new(EnumEqualityComparer<PropCategory>.Instance);
    private readonly List<PropCategory> propTypes = [];
    private readonly PaneHeader paneHeader;

    public PropsPane()
    {
        propTypeDropdown = new(formatter: CategoryFormatter);

        paneHeader = new(Translation.Get("propsPane", "header"), true);

        static string CategoryFormatter(PropCategory category, int index) =>
            Translation.Get("propTypes", EnumToLower(category));
    }

    public enum PropCategory
    {
        Game,
        Desk,
        Other,
        Background,
        MyRoom,
        Menu,
        HandItem,
    }

    public BasePane this[PropCategory category]
    {
        get => propPanes[category];
        set => Add(category, value);
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginHorizontal();

        const int ScrollBarWidth = 23;

        var buttonAndScrollbarSize = ScrollBarWidth + Utility.GetPix(20) * 2 + 5;
        var dropdownButtonWidth = parent.WindowRect.width - buttonAndScrollbarSize;

        propTypeDropdown.Draw(GUILayout.Width(dropdownButtonWidth));

        var arrowLayoutOptions = GUILayout.ExpandWidth(false);

        if (GUILayout.Button("<", arrowLayoutOptions))
            propTypeDropdown.CyclePrevious();

        if (GUILayout.Button(">", arrowLayoutOptions))
            propTypeDropdown.CycleNext();

        GUILayout.EndHorizontal();

        MpsGui.WhiteLine();

        propPanes[propTypes[propTypeDropdown.SelectedItemIndex]].Draw();
    }

    public IEnumerator<KeyValuePair<PropCategory, BasePane>> GetEnumerator() =>
        propPanes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(PropCategory key, BasePane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        propPanes[key] = pane;

        propTypes.Add(key);

        propTypeDropdown.SetItemsWithoutNotify(propTypes, 0);
    }

    public override void SetParent(BaseWindow window)
    {
        base.SetParent(window);

        foreach (var pane in propPanes.Values)
            pane.SetParent(window);
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        propTypeDropdown.Reformat();

        paneHeader.Label = Translation.Get("propsPane", "header");
    }

    private static string EnumToLower<T>(T enumValue)
        where T : Enum
    {
        var enumString = enumValue.ToString();

        return char.ToLower(enumString[0]) + enumString.Substring(1);
    }
}
