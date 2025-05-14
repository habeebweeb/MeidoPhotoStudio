using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterCallPane : BasePane, IVirtualListHandler
{
    private readonly CallController characterCallController;
    private readonly LazyStyle labelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly LazyStyle selectedIndexStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.UpperRight,
        });

    private readonly LazyStyle selectedLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly Button clearSelectedButton;
    private readonly Button callButton;
    private readonly Toggle activeCharacterToggle;
    private readonly TextField searchBar;
    private readonly Dropdown<CallController.SortType> sortTypeDropdown;
    private readonly Toggle descendingToggle;
    private readonly Header header;
    private readonly VirtualList virtualList;

    private Vector2 buttonSize;
    private Vector2 charactersListScrollPosition;
    private Vector2 activeCharactersListScrollPosition;

    public CharacterCallPane(Translation translation, CallController characterCallController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterCallController = characterCallController ?? throw new ArgumentNullException(nameof(characterCallController));
        this.characterCallController.PropertyChanged += OnCallControllerPropertyChanged;

        clearSelectedButton = new(new LocalizableGUIContent(translation, "maidCallWindow", "clearButton"));
        clearSelectedButton.ControlEvent += OnClearMaidsButttonPushed;

        callButton = new(new LocalizableGUIContent(translation, "maidCallWindow", "callButton"));
        callButton.ControlEvent += OnCallButtonPushed;

        activeCharacterToggle = new(new LocalizableGUIContent(translation, "maidCallWindow", "activeOnlyToggle"));
        activeCharacterToggle.ControlEvent += OnActiveCharactersToggleChanged;

        searchBar = new()
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "maidCallWindow", "searchBarPlaceholder"),
        };

        searchBar.ChangedValue += OnSearchSubmitted;

        sortTypeDropdown = new(
            (CallController.SortType[])Enum.GetValues(typeof(CallController.SortType)), formatter: SortTypeFormatter);

        sortTypeDropdown.SelectionChanged += OnSortTypeChanged;

        translation.Initialized += OnTranslationInitialized;

        descendingToggle = new(new LocalizableGUIContent(translation, "maidCallWindow", "descendingToggle"));
        descendingToggle.ControlEvent += OnDescendingChanged;

        header = new(new LocalizableGUIContent(translation, "maidCallWindow", "header"));

        virtualList = new()
        {
            Handler = this,
        };

        LabelledDropdownItem SortTypeFormatter(CallController.SortType sortType, int index) =>
            new(translation["characterSortTypeDropdown", sortType.ToLower()]);

        void OnTranslationInitialized(object sender, EventArgs e) =>
            sortTypeDropdown.Reformat();
    }

    int IVirtualListHandler.Count =>
        characterCallController.Count;

    public override void Draw()
    {
        header.Draw();

        DrawTextFieldMaxWidth(searchBar);

        GUILayout.BeginHorizontal();

        sortTypeDropdown.Draw(GUILayout.Width(Parent.WindowRect.width - UIUtility.Scaled(125)));

        GUILayout.FlexibleSpace();

        descendingToggle.Draw();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUI.enabled = Parent.Enabled && characterCallController.HasActiveCharacters;

        activeCharacterToggle.Draw();

        GUI.enabled = Parent.Enabled;

        clearSelectedButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        callButton.Draw();

        var windowRect = Parent.WindowRect;
        var buttonWidth = windowRect.width - 25f;

        buttonSize = new(buttonWidth, UIUtility.Scaled(85f));

        var buttonHeight = buttonSize.y;

        var scrollRect = GUILayoutUtility.GetRect(0f, windowRect.width, 0f, windowRect.height);

        if (characterCallController.ActiveOnly)
            activeCharactersListScrollPosition = virtualList.BeginScrollView(scrollRect, activeCharactersListScrollPosition);
        else
            charactersListScrollPosition = virtualList.BeginScrollView(scrollRect, charactersListScrollPosition);

        foreach (var (i, offset) in virtualList)
        {
            var character = characterCallController[i];
            var y = scrollRect.y + offset.y;

            if (GUI.Button(new(scrollRect.x, y, buttonWidth, buttonHeight), string.Empty))
                characterCallController.Select(character);

            var characterSelected = characterCallController.CharacterSelected(character);

            if (characterSelected)
            {
                var selectedIndex = characterCallController.IndexOfSelectedCharacter(character) + 1;

                GUI.DrawTexture(new(scrollRect.x + 5f, y + 5f, buttonWidth - 10f, buttonHeight - 10f), Texture2D.whiteTexture);

                GUI.Label(new(scrollRect.x, y + 5f, buttonWidth - 10f, buttonHeight), selectedIndex.ToString(), selectedIndexStyle);
            }

            if (character.Portrait)
                GUI.DrawTexture(new(scrollRect.x + 5f, y, buttonHeight, buttonHeight), character.Portrait);

            GUI.Label(
                new(scrollRect.x + buttonHeight + 5f, y, buttonWidth - scrollRect.x + buttonHeight + 5f, buttonHeight),
                character.FullName("{0}\n{1}"),
                characterSelected ? selectedLabelStyle : labelStyle);
        }

        GUI.EndScrollView();
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        virtualList.Invalidate();
    }

    Vector2 IVirtualListHandler.ItemDimensions(int index) =>
        buttonSize;

    public override void Activate()
    {
        charactersListScrollPosition = Vector2.zero;
        activeCharactersListScrollPosition = Vector2.zero;
        searchBar.Value = string.Empty;
    }

    private void OnCallControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CallController.ActiveOnly))
            activeCharacterToggle.SetEnabledWithoutNotify(characterCallController.ActiveOnly);
        else if (e.PropertyName is nameof(CallController.Sort))
            sortTypeDropdown.SetSelectedIndexWithoutNotify(sortTypeDropdown.IndexOf(characterCallController.Sort));
        else if (e.PropertyName is nameof(CallController.Descending))
            descendingToggle.SetEnabledWithoutNotify(characterCallController.Descending);
    }

    private void OnClearMaidsButttonPushed(object sender, EventArgs e) =>
        characterCallController.ClearSelected();

    private void OnCallButtonPushed(object sender, EventArgs e) =>
        characterCallController.Call();

    private void OnActiveCharactersToggleChanged(object sender, EventArgs e) =>
        characterCallController.ActiveOnly = activeCharacterToggle.Value;

    private void OnSearchSubmitted(object sender, EventArgs e) =>
        characterCallController.Search(searchBar.Value);

    private void OnSortTypeChanged(object sender, DropdownEventArgs<CallController.SortType> e) =>
        characterCallController.Sort = e.Item;

    private void OnDescendingChanged(object sender, EventArgs e) =>
        characterCallController.Descending = descendingToggle.Value;
}
