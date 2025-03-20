using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CopyFacialExpressionPane : BasePane
{
    private readonly FacialExpressionBuilder facialExpressionBuilder;
    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Label noOtherCharactersLabel;
    private readonly Dropdown<CharacterController> otherCharacterDropdown;
    private readonly Button copyExpressionButton;
    private readonly Button copyExpressionToEveryoneElseButton;
    private readonly LazyStyle noOtherCharactersLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    public CopyFacialExpressionPane(
        Translation translation,
        FacialExpressionBuilder facialExpressionBuilder,
        CharacterService characterService,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.facialExpressionBuilder = facialExpressionBuilder ?? throw new ArgumentNullException(nameof(facialExpressionBuilder));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CalledCharacters += OnCharactersCalled;

        noOtherCharactersLabel = new(
            new LocalizableGUIContent(translation, "copyFacialExpressionPane", "noOtherCharactersLabel"));

        otherCharacterDropdown = new(formatter: OtherCharacterFormatter);

        copyExpressionButton = new(
            new LocalizableGUIContent(translation, "copyFacialExpressionPane", "copyExpressionButton"));

        copyExpressionButton.ControlEvent += OnCopyExpressionButtonPushed;

        copyExpressionToEveryoneElseButton = new(
            new LocalizableGUIContent(translation, "copyFacialExpressionPane", "copyExpressionToEveryoneButton"));

        copyExpressionToEveryoneElseButton.ControlEvent += OnCopyExpressionToEveryoneElseButtonPushed;

        static LabelledDropdownItem OtherCharacterFormatter(CharacterController character, int index) =>
            new($"{character.Slot + 1}: {character.CharacterModel.FullName()}");
    }

    private CharacterController OtherCharacter =>
        characterService.Count > 0
            ? characterService[otherCharacterDropdown.SelectedItemIndex]
            : null;

    private CharacterController CurrentCharacter =>
        characterSelectionController.Current;

    public override void Draw()
    {
        GUI.enabled = Parent.Enabled && CurrentCharacter is not null;

        if (characterService.Count is 1)
        {
            noOtherCharactersLabel.Draw(noOtherCharactersLabelStyle);

            return;
        }

        DrawDropdown(otherCharacterDropdown);

        UIUtility.DrawBlackLine();

        if (CurrentCharacter != OtherCharacter)
            copyExpressionButton.Draw();
        else if (characterService.Count > 1)
            copyExpressionToEveryoneElseButton.Draw();
    }

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e) =>
        otherCharacterDropdown.SetItemsWithoutNotify(characterService, 0);

    private void OnCopyExpressionButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is not CharacterController current || OtherCharacter is not CharacterController other)
            return;

        ApplyExpression(facialExpressionBuilder.Build(other.Face), current);
    }

    private void OnCopyExpressionToEveryoneElseButtonPushed(object sender, EventArgs e)
    {
        if (CurrentCharacter is not CharacterController current)
            return;

        var expression = facialExpressionBuilder.Build(current.Face);

        foreach (var character in characterService.Where(character => character is not null && character != current))
            ApplyExpression(expression, character);
    }

    private void ApplyExpression(FacialExpressionSet expression, CharacterController character)
    {
        foreach (var (hash, value) in expression.Where(kvp => character.Face.ContainsShapeKey(kvp.Key)))
            character.Face[hash] = value;
    }
}
