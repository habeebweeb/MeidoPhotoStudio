namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public class LabelledDropdownItem(string label) : IDropdownItem
{
    private GUIContent formatted;

    public string Label { get; } = label;

    public bool HasIcon { get; } = false;

    public int IconSize { get; }

    public Texture Icon { get; }

    public GUIContent Formatted =>
        formatted ??= new(Label);

    public void Dispose()
    {
    }
}
