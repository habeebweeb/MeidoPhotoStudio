using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterCallPane : BasePane
{
    private const int FontSize = 13;

    private readonly CallController characterCallController;
    private readonly LazyStyle labelStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly LazyStyle selectedIndexStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.UpperRight,
        });

    private readonly LazyStyle selectedLabelStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly Button clearSelectedButton;
    private readonly Button callButton;
    private readonly Toggle activeCharacterToggle;

    private Vector2 charactersListScrollPosition;
    private Vector2 activeCharactersListScrollPosition;

    public CharacterCallPane(CallController characterCallController)
    {
        this.characterCallController = characterCallController ?? throw new ArgumentNullException(nameof(characterCallController));

        clearSelectedButton = new(Translation.Get("maidCallWindow", "clearButton"));
        clearSelectedButton.ControlEvent += OnClearMaidsButttonPushed;

        callButton = new(Translation.Get("maidCallWindow", "callButton"));
        callButton.ControlEvent += OnCallButtonPushed;

        activeCharacterToggle = new(Translation.Get("maidCallWindow", "activeOnlyToggle"));
        activeCharacterToggle.ControlEvent += OnActiveCharactersToggleChanged;
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();
        clearSelectedButton.Draw(GUILayout.ExpandWidth(false));
        callButton.Draw();
        GUILayout.EndHorizontal();

        MpsGui.WhiteLine();

        GUI.enabled = characterCallController.HasActiveCharacters;

        activeCharacterToggle.Draw();

        GUI.enabled = true;

        var windowRect = parent.WindowRect;
        var buttonWidth = windowRect.width - 25f;

        const float buttonSize = 85f;

        var buttonHeight = Utility.GetPix(buttonSize);

        var scrollRect = GUILayoutUtility.GetRect(0f, windowRect.width, 0f, windowRect.height);
        var scrollView = new Rect(scrollRect.x, scrollRect.y, windowRect.width - 20f, buttonHeight * characterCallController.Count);

        if (characterCallController.ActiveOnly)
            activeCharactersListScrollPosition = GUI.BeginScrollView(scrollRect, activeCharactersListScrollPosition, scrollView);
        else
            charactersListScrollPosition = GUI.BeginScrollView(scrollRect, charactersListScrollPosition, scrollView);

        var scrollPosition = characterCallController.ActiveOnly
            ? activeCharactersListScrollPosition
            : charactersListScrollPosition;

        var firstVisibleIndex = Mathf.FloorToInt(scrollPosition.y / buttonHeight);
        var lastVisibleIndex = Mathf.CeilToInt((scrollPosition.y + scrollRect.height) / buttonHeight);

        if (firstVisibleIndex < 0)
            firstVisibleIndex = 0;

        if (lastVisibleIndex > characterCallController.Count)
            lastVisibleIndex = characterCallController.Count;

        for (var i = firstVisibleIndex; i < lastVisibleIndex; i++)
        {
            var character = characterCallController[i];
            var y = scrollRect.y + i * buttonHeight;

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

    public override void Activate()
    {
        characterCallController.Activate();

        charactersListScrollPosition = Vector2.zero;
        activeCharactersListScrollPosition = Vector2.zero;
    }

    protected override void ReloadTranslation()
    {
        clearSelectedButton.Label = Translation.Get("maidCallWindow", "clearButton");
        callButton.Label = Translation.Get("maidCallWindow", "callButton");
        activeCharacterToggle.Label = Translation.Get("maidCallWindow", "activeOnlyToggle");
    }

    private void OnClearMaidsButttonPushed(object sender, EventArgs e) =>
        characterCallController.ClearSelected();

    private void OnCallButtonPushed(object sender, EventArgs e) =>
        characterCallController.Call();

    private void OnActiveCharactersToggleChanged(object sender, EventArgs e) =>
        characterCallController.ActiveOnly = !characterCallController.ActiveOnly;
}
