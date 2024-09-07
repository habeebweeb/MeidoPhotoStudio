using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterSwitcherPane : BasePane
{
    private const float BoxSize = 70;
    private const int Margin = (int)(BoxSize / 2.8f);
    private const int FontSize = 13;

    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;
    private readonly LazyStyle buttonStyle = new(
        FontSize,
        () => new(GUI.skin.button)
        {
            margin = { top = Margin },
        });

    private readonly LazyStyle horizontalStyle = new(
        0,
        () => new()
        {
            padding = new(4, 4, 0, 0),
        });

    private readonly LazyStyle labelStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            margin = { top = Margin },
        });

    private readonly LazyStyle slotStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            padding = { right = 5 },
            normal = { textColor = Color.white },
        });

    private readonly Button previousButton;
    private readonly Button nextButton;

    private bool editingCharacter;
    private string editToggleLabel;

    private CharacterController preCallCharacter;

    public CharacterSwitcherPane(
        CharacterService characterService,
        SelectionController<CharacterController> characterSelectionController,
        CustomMaidSceneService customMaidSceneService,
        EditModeMaidService editModeMaidService)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.editModeMaidService = editModeMaidService ?? throw new ArgumentNullException(nameof(editModeMaidService));

        this.characterService.CallingCharacters += OnCallingCharacters;
        this.characterService.CalledCharacters += OnCharactersCalled;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        previousButton = new("<");
        previousButton.ControlEvent += (_, _) =>
            PreviousCharacter();

        nextButton = new(">");
        nextButton.ControlEvent += (_, _) =>
            NextCharacter();

        editToggleLabel = Translation.Get("characterSwitcher", "editToggle");
    }

    public override void Draw()
    {
        var buttonOptions = new[]
        {
            GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(false),
        };

        var boxLayoutOptions = new[]
        {
            GUILayout.Height(Utility.GetPix(BoxSize)),
            GUILayout.Width(Utility.GetPix(BoxSize)),
        };

        var guiEnabled = characterService.Count > 0;

        GUI.enabled = guiEnabled;

        GUILayout.BeginHorizontal(horizontalStyle, GUILayout.Height(Utility.GetPix(BoxSize)));

        previousButton.Draw(buttonStyle, buttonOptions);

        GUILayout.Space(20);

        var character = characterSelectionController.Current;

        if (characterService.Count > 0)
        {
            if (GUILayout.Button(character.CharacterModel.Portrait, boxLayoutOptions))
                character.FocusOnBody();

            var label = character.CharacterModel.FullName("{0}\n{1}");

            GUILayout.Label(label, labelStyle, GUILayout.ExpandWidth(false));
        }

        GUILayout.FlexibleSpace();

        nextButton.Draw(buttonStyle, buttonOptions);

        GUILayout.EndHorizontal();

        var previousRect = GUILayoutUtility.GetLastRect();

        if (customMaidSceneService.EditScene)
        {
            GUI.enabled = guiEnabled && !editingCharacter;

            var value = GUI.Toggle(new Rect(previousRect.x + 4f, previousRect.y, 40f, 20f), editingCharacter, editToggleLabel);

            if (value != editingCharacter)
            {
                editingCharacter = value;

                ChangeEditCharacter();
            }

            GUI.enabled = guiEnabled;
        }

        var labelRect = new Rect(previousRect.width - 45f, previousRect.y, 40f, 20f);

        if (characterService.Count > 0)
            GUI.Label(labelRect, $"{character.Slot + 1}", slotStyle);
    }

    protected override void ReloadTranslation() =>
        editToggleLabel = Translation.Get("characterSwitcher", "editToggle");

    private static int Wrap(int value, int min, int max) =>
        value < min ? max : value > max ? min : value;

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (!customMaidSceneService.EditScene)
            return;

        if (characterSelectionController.Current is null)
            return;

        editingCharacter = editModeMaidService.EditingCharacter == characterSelectionController.Current.CharacterModel;
    }

    private void ChangeEditCharacter()
    {
        if (!customMaidSceneService.EditScene)
            return;

        if (characterSelectionController.Current is null)
            return;

        editModeMaidService.SetEditingCharacter(characterSelectionController.Current.CharacterModel);
    }

    private void OnCallingCharacters(object sender, CharacterServiceEventArgs e) =>
        preCallCharacter = characterSelectionController.Current;

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        if (e.LoadedCharacters.Length is 0)
            return;

        if (preCallCharacter is null || !e.LoadedCharacters.Contains(preCallCharacter))
            characterSelectionController.Select(0);
        else
            characterSelectionController.Select(preCallCharacter);
    }

    private void NextCharacter()
    {
        var nextIndex = Wrap(characterSelectionController.CurrentIndex + 1, 0, characterService.Count - 1);

        characterSelectionController.Select(nextIndex);
    }

    private void PreviousCharacter()
    {
        var previousIndex = Wrap(characterSelectionController.CurrentIndex - 1, 0, characterService.Count - 1);

        characterSelectionController.Select(previousIndex);
    }
}
