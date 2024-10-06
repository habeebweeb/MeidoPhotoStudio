using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterSwitcherPane : BasePane
{
    private const float BoxSize = 70;
    private const int FontSize = 13;

    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;
    private readonly LazyStyle buttonStyle = new(FontSize, () => new(GUI.skin.button));

    private readonly LazyStyle slotStyle = new(
        FontSize,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            padding = { right = 5 },
            normal = { textColor = Color.white },
        });

    private readonly Dropdown<CharacterController> characterDropdown;
    private readonly Toggle editToggle;
    private readonly Button focusBodyButton;
    private readonly Button focusFaceButton;

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

        characterDropdown = new([], formatter: CharacterFormatter);
        characterDropdown.SelectionChanged += OnSelectionChanged;

        editToggle = new(Translation.Get("characterSwitcher", "editToggle"));
        editToggle.ControlEvent += OnEditToggleChanged;

        focusBodyButton = new(Translation.Get("characterSwitcher", "focusBodyButton"));
        focusBodyButton.ControlEvent += OnFocusBodyButtonPushed;

        focusFaceButton = new(Translation.Get("characterSwitcher", "focusFaceButton"));
        focusFaceButton.ControlEvent += OnFocusFaceButtonPushed;

        static CharacterDropdownItem CharacterFormatter(CharacterController character, int index) =>
            new(character);
    }

    public override void Draw()
    {
        if (characterService.Count is 0)
            return;

        var buttonHeight = GUILayout.Height(Utility.GetPix(BoxSize));

        var buttonOptions = new[]
        {
            buttonHeight, GUILayout.ExpandWidth(false),
        };

        var guiEnabled = characterService.Count > 0;

        GUILayout.BeginHorizontal();

        if (customMaidSceneService.EditScene)
        {
            GUI.enabled = guiEnabled && characterSelectionController.Current?.CharacterModel != editModeMaidService.EditingCharacter;

            editToggle.Draw();

            GUI.enabled = guiEnabled;
        }

        GUILayout.FlexibleSpace();

        GUI.enabled = guiEnabled;

        GUILayout.Label($"{characterSelectionController.Current?.Slot + 1}", slotStyle);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("<", buttonStyle, buttonOptions))
            characterDropdown.CyclePrevious();

        GUILayout.FlexibleSpace();

        var windowWidth = parent.WindowRect.width;
        var dropdownWidth = windowWidth - 95f;

        characterDropdown.Draw(GUILayout.Width(dropdownWidth), GUILayout.Height(Utility.GetPix(BoxSize)));

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(">", buttonStyle, buttonOptions))
            characterDropdown.CycleNext();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        focusBodyButton.Draw();

        focusFaceButton.Draw();

        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        editToggle.Label = Translation.Get("characterSwitcher", "editToggle");
        focusBodyButton.Label = Translation.Get("characterSwitcher", "focusBodyButton");
        focusFaceButton.Label = Translation.Get("characterSwitcher", "focusFaceButton");
    }

    private void OnEditToggleChanged(object sender, EventArgs e)
    {
        if (!customMaidSceneService.EditScene)
            return;

        if (characterSelectionController.Current is null)
            return;

        editModeMaidService.SetEditingCharacter(characterSelectionController.Current.CharacterModel);
    }

    private void OnFocusBodyButtonPushed(object sender, EventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        characterSelectionController.Current.FocusOnBody();
    }

    private void OnFocusFaceButtonPushed(object sender, EventArgs e)
    {
        if (characterSelectionController.Current is null)
            return;

        characterSelectionController.Current.FocusOnFace();
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        characterDropdown.SetSelectedIndexWithoutNotify(characterSelectionController.CurrentIndex);

        if (!customMaidSceneService.EditScene)
            return;

        if (characterSelectionController.Current is null)
            return;

        editToggle.SetEnabledWithoutNotify(editModeMaidService.EditingCharacter == characterSelectionController.Current.CharacterModel);
    }

    private void OnSelectionChanged(object sender, DropdownEventArgs<CharacterController> e) =>
        characterSelectionController.Select(e.Item);

    private void OnCallingCharacters(object sender, CharacterServiceEventArgs e) =>
        preCallCharacter = characterSelectionController.Current;

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e)
    {
        if (e.LoadedCharacters.Length is 0)
            return;

        characterDropdown.SetItemsWithoutNotify(characterService);

        if (preCallCharacter is null || !e.LoadedCharacters.Contains(preCallCharacter))
        {
            characterSelectionController.Select(0);
            characterDropdown.SetSelectedIndexWithoutNotify(0);
        }
        else
        {
            characterSelectionController.Select(preCallCharacter);
            characterDropdown.SetSelectedIndexWithoutNotify(characterSelectionController.CurrentIndex);
        }
    }

    private class CharacterDropdownItem(CharacterController characterController) : IDropdownItem
    {
        private GUIContent formatted;

        public string Label { get; } = characterController.CharacterModel.FullName("{0}\n{1}");

        public bool HasIcon { get; } = true;

        public int IconSize { get; } = (int)BoxSize;

        public Texture Icon =>
            characterController.CharacterModel.Portrait;

        public GUIContent Formatted =>
            formatted ??= new(Label, Icon);

        public void Dispose()
        {
        }
    }
}
