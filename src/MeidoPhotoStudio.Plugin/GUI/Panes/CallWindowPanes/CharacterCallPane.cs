using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class CharacterCallPane : BasePane
{
    private readonly CallController characterCallController;
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

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
        };

        var selectLabelStyle = new GUIStyle(labelStyle)
        {
            normal = { textColor = Color.black },
            alignment = TextAnchor.UpperRight,
        };

        var labelSelectedStyle = new GUIStyle(labelStyle)
        {
            normal = { textColor = Color.black },
        };

        var windowRect = parent.WindowRect;
        var windowHeight = windowRect.height;
        var buttonWidth = windowRect.width - 30f;

        const float buttonHeight = 85f;
        const float offsetTop = 130f;

        var positionRect = new Rect(5f, offsetTop, windowRect.width - 10f, windowHeight - (offsetTop + 35));
        var viewRect = new Rect(0f, 0f, buttonWidth, buttonHeight * characterCallController.Count + 5f);

        if (characterCallController.ActiveOnly)
            activeCharactersListScrollPosition = GUI.BeginScrollView(positionRect, activeCharactersListScrollPosition, viewRect);
        else
            charactersListScrollPosition = GUI.BeginScrollView(positionRect, charactersListScrollPosition, viewRect);

        foreach (var (i, character) in characterCallController.WithIndex())
        {
            var y = i * buttonHeight;

            if (GUI.Button(new(0f, y, buttonWidth, buttonHeight), string.Empty))
                characterCallController.Select(character);

            var characterSelected = characterCallController.CharacterSelected(character);

            if (characterSelected)
            {
                var selectedIndex = characterCallController.IndexOfSelectedCharacter(character) + 1;

                GUI.DrawTexture(new(5f, y + 5f, buttonWidth - 10f, buttonHeight - 10f), Texture2D.whiteTexture);

                GUI.Label(new(0f, y + 5f, buttonWidth - 10f, buttonHeight), selectedIndex.ToString(), selectLabelStyle);
            }

            if (character.Portrait)
                GUI.DrawTexture(new(5f, y, buttonHeight, buttonHeight), character.Portrait);

            GUI.Label(
                new(95f, y + 30f, buttonWidth - 80f, buttonHeight),
                character.FullName("{0}\n{1}"),
                characterSelected ? labelSelectedStyle : labelStyle);
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
