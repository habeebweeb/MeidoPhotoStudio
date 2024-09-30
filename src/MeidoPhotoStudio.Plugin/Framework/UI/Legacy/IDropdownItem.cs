namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

public interface IDropdownItem
{
    string Label { get; }

    bool HasIcon { get; }

    int IconSize { get; }

    Texture Icon { get; }

    GUIContent Formatted { get; }

    void Dispose();
}
