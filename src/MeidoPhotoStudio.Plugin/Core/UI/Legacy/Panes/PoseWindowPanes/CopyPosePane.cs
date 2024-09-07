using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CopyPosePane : BasePane
{
    private readonly PaneHeader paneHeader;
    private readonly Dropdown<CharacterController> otherCharacterDropdown;
    private readonly Button copyPoseButton;
    private readonly Button copyBothHandsButton;
    private readonly Button copyLeftHandToLeftButton;
    private readonly Button copyLeftHandToRightButton;
    private readonly Button copyRightHandToLeftButton;
    private readonly Button copyRightHandToRightButton;
    private readonly CharacterService characterService;
    private readonly CharacterUndoRedoService characterUndoRedoService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Header copyHandHeader;

    public CopyPosePane(
        CharacterService characterService,
        CharacterUndoRedoService characterUndoRedoService,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterUndoRedoService = characterUndoRedoService ?? throw new ArgumentNullException(nameof(characterUndoRedoService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;

        paneHeader = new(Translation.Get("copyPosePane", "header"), true);

        otherCharacterDropdown = new(formatter: OtherCharacterFormatter);

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

        copyHandHeader = new(Translation.Get("copyPosePane", "copyHandHeader"));

        static string OtherCharacterFormatter(CharacterController character, int index) =>
            $"{character.Slot + 1}: {character.CharacterModel.FullName()}";
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

        if (!paneHeader.Enabled)
            return;

        DrawDropdown(otherCharacterDropdown);

        if (CurrentCharacter != OtherCharacter)
        {
            MpsGui.BlackLine();

            copyPoseButton.Draw();
        }

        copyHandHeader.Draw();
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

        void DrawDropdown<T>(Dropdown<T> dropdown)
        {
            GUILayout.BeginHorizontal();

            const int ScrollBarWidth = 23;

            var buttonAndScrollbarSize = ScrollBarWidth + Utility.GetPix(20) * 2 + 5;
            var dropdownButtonWidth = parent.WindowRect.width - buttonAndScrollbarSize;

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            var arrowLayoutOptions = GUILayout.ExpandWidth(false);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.CyclePrevious();

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.CycleNext();

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
        copyHandHeader.Text = Translation.Get("copyPosePane", "copyHandHeader");
    }

    private void OnCharactersCalled(object sender, EventArgs e) =>
        otherCharacterDropdown.SetItemsWithoutNotify(characterService, 0);

    private void OnCopyPoseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyPoseFrom(OtherCharacter);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }

    private void OnCopyBothHandsButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandRight, HandOrFootType.HandRight);
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, HandOrFootType.HandLeft, HandOrFootType.HandLeft);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }

    private void OnCopyRightHandToRightButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandRight, HandOrFootType.HandRight);

    private void OnCopyRightHandToLeftButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandRight, HandOrFootType.HandLeft);

    private void OnCopyLefHandToLeftButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandLeft, HandOrFootType.HandLeft);

    private void OnCopyLefHandToRightButtonPushed(object sender, EventArgs e) =>
        CopyHands(HandOrFootType.HandLeft, HandOrFootType.HandRight);

    private void CopyHands(HandOrFootType copyFrom, HandOrFootType copyTo)
    {
        if (CurrentCharacter is null || OtherCharacter is null)
            return;

        characterUndoRedoService[CurrentCharacter].StartPoseChange();
        CurrentCharacter.IK.CopyHandOrFootFrom(OtherCharacter, copyFrom, copyTo);
        characterUndoRedoService[CurrentCharacter].EndPoseChange();
    }
}
