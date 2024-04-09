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

    public CopyPosePane(CharacterService characterService, SelectionController<CharacterController> characterSelectionController)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;

        paneHeader = new("Copy Pose", true);

        otherCharacterDropdown = new(["No Characters"]);

        copyPoseButton = new("Copy Pose");
        copyPoseButton.ControlEvent += OnCopyPoseButtonPushed;

        copyBothHandsButton = new("Copy Both Hands");
        copyBothHandsButton.ControlEvent += OnCopyBothHandsButtonPushed;

        copyLeftHandToLeftButton = new("L > L");
        copyLeftHandToLeftButton.ControlEvent += OnCopyLefHandToLeftButtonPushed;

        copyLeftHandToRightButton = new("L > R");
        copyLeftHandToRightButton.ControlEvent += OnCopyLefHandToRightButtonPushed;

        copyRightHandToLeftButton = new("R > L");
        copyRightHandToLeftButton.ControlEvent += OnCopyRightHandToLeftButtonPushed;

        copyRightHandToRightButton = new("R > R");
        copyRightHandToRightButton.ControlEvent += OnCopyRightHandToRightButtonPushed;
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

        GUILayout.Label("Copy Hand");
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

    private void OnCharactersCalled(object sender, EventArgs e)
    {
        if (characterService.Count is 0)
        {
            otherCharacterDropdown.SetDropdownItemsWithoutNotify(["No Characters"]);

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
