using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;

namespace MeidoPhotoStudio.Plugin;

public class CopyPosePane : BasePane
{
    private readonly Toggle paneHeader;
    private readonly Dropdown otherCharacterDropdown;
    private readonly Button copyPoseButton;
    private readonly Button copyBothHandsButton;
    private readonly Button copyLeftHandToLeftButton;
    private readonly Button copyLeftHandToRightButton;
    private readonly Button copyRightHandToLeftButton;
    private readonly Button copyRightHandToRightButton;
    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> characterSelectionController;

    private string copyHandHeader = string.Empty;

    public CopyPosePane(CharacterService characterService, SelectionController<CharacterController> characterSelectionController)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;

        paneHeader = new(Translation.Get("copyPosePane", "header"), true);

        otherCharacterDropdown = new([Translation.Get("systemMessage", "noMaids")]);

        copyPoseButton = new(Translation.Get("copyPosePane", "copyButton"));
        copyPoseButton.ControlEvent += OnCopyPoseButtonPushed;

        copyBothHandsButton = new(Translation.Get("copyPosePane", "copyBothHands"));
        copyBothHandsButton.ControlEvent += OnCopyBothHandsButtonPushed;

        copyLeftHandToLeftButton = new(Translation.Get("copyPosePane", "copyLeftHandToLeft"));
        copyLeftHandToLeftButton.ControlEvent += OnCopyLefHandToLeftButtonPushed;

        copyLeftHandToRightButton = new(Translation.Get("copyPosePane", "copyLeftHandToRight"));
        copyLeftHandToRightButton.ControlEvent += OnCopyLefHandToRightButtonPushed;

        copyRightHandToLeftButton = new(Translation.Get("copyPosePane", "copyRightHandToLeft"));
        copyRightHandToLeftButton.ControlEvent += OnCopyRightHandToLeftButtonPushed;

        copyRightHandToRightButton = new(Translation.Get("copyPosePane", "copyRightHandToRight"));
        copyRightHandToRightButton.ControlEvent += OnCopyRightHandToRightButtonPushed;

        copyHandHeader = Translation.Get("copyPosePane", "copyHandHeader");
    }

    private CharacterController OtherCharacter =>
        characterService.Count > 0
            ? characterService[otherCharacterDropdown.SelectedItemIndex]
            : null;

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        GUI.enabled = CurrentCharacter is not null;

        paneHeader.Draw();
        MpsGui.WhiteLine();

        if (!paneHeader.Value)
            return;

        DrawDropdown(otherCharacterDropdown);

        if (CurrentCharacter != OtherCharacter)
        {
            MpsGui.BlackLine();

            copyPoseButton.Draw();
        }

        GUILayout.Label(copyHandHeader);
        MpsGui.BlackLine();

        if (CurrentCharacter != OtherCharacter)
        {
            copyBothHandsButton.Draw();

            GUILayout.BeginHorizontal();

            copyRightHandToRightButton.Draw();
            copyRightHandToLeftButton.Draw();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            copyLeftHandToRightButton.Draw();
            copyLeftHandToLeftButton.Draw();

            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.BeginHorizontal();

            copyRightHandToLeftButton.Draw();
            copyLeftHandToRightButton.Draw();

            GUILayout.EndHorizontal();
        }

        static void DrawDropdown(Dropdown dropdown)
        {
            GUILayout.BeginHorizontal();

            const float dropdownButtonWidth = 175f;

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.Step(1);

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("copyPosePane", "header");
        copyPoseButton.Label = Translation.Get("copyPosePane", "copyButton");
        copyBothHandsButton.Label = Translation.Get("copyPosePane", "copyBothHands");
        copyLeftHandToLeftButton.Label = Translation.Get("copyPosePane", "copyLeftHandToLeft");
        copyLeftHandToRightButton.Label = Translation.Get("copyPosePane", "copyLeftHandToRight");
        copyRightHandToLeftButton.Label = Translation.Get("copyPosePane", "copyRightHandToLeft");
        copyRightHandToRightButton.Label = Translation.Get("copyPosePane", "copyRightHandToRight");
        copyHandHeader = Translation.Get("copyPosePane", "copyHandHeader");
    }

    private void OnCharactersCalled(object sender, EventArgs e)
    {
        if (characterService.Count is 0)
        {
            otherCharacterDropdown.SetDropdownItemsWithoutNotify([Translation.Get("systemMessage", "noMaids")]);

            return;
        }

        otherCharacterDropdown.SetDropdownItemsWithoutNotify(
            characterService
                .Select(character => $"{character.Slot + 1}: {CharacterName(character)}")
                .ToArray(),
            0);

        static string CharacterName(CharacterController character) =>
            character.CharacterModel.FullName();
    }

    private void OnCopyPoseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyPoseFrom(OtherCharacter);
    }

    private void OnCopyBothHandsButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandRight, HandOrFootType.HandRight);
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandLeft, HandOrFootType.HandLeft);
    }

    private void OnCopyRightHandToRightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandRight, HandOrFootType.HandRight);
    }

    private void OnCopyRightHandToLeftButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandRight, HandOrFootType.HandLeft);
    }

    private void OnCopyLefHandToLeftButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandLeft, HandOrFootType.HandLeft);
    }

    private void OnCopyLefHandToRightButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandLeft, HandOrFootType.HandRight);
    }
}
