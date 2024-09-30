namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

internal interface IDropdownHandler : IVirtualListHandler
{
    Vector2 ScrollPosition { get; set; }

    int SelectedItemIndex { get; }

    GUIContent FormattedItem(int index);

    void OnItemSelected(int index);

    void OnDropdownClosed(bool clickedButton);
}
