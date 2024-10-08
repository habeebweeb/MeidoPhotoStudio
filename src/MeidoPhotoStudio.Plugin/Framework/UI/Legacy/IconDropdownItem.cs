namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class IconDropdownItem(string label, Func<Texture> iconProvider = null, int iconHeight = 0, bool shouldDispose = false)
    : IDropdownItem
{
    private readonly Func<Texture> iconProvider = iconProvider;

    private GUIContent formatted;
    private Texture icon;

    public string Label { get; } = label;

    public int IconSize =>
        iconHeight;

    public bool HasIcon =>
        iconProvider is not null;

    public Texture Icon
    {
        get
        {
            if (icon)
                return icon;

            if (iconProvider is null)
                return null;

            icon = iconProvider();

            return icon;
        }
    }

    public GUIContent Formatted =>
        formatted ??= new(Label, Icon);

    public void Dispose()
    {
        if (!shouldDispose)
            return;

        if (icon)
            Object.Destroy(icon);
    }
}
