using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class CharacterSwitcherPane : BasePane
{
    private const float BoxSize = 80f;

    private readonly CharacterService characterService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;

    private readonly LazyStyle slotStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.UpperRight,
            padding = { right = 5 },
            normal = { textColor = Color.white },
        });

    private readonly Dropdown<CharacterController> characterDropdown;
    private readonly Toggle editToggle;
    private readonly Button focusBodyButton;
    private readonly Button focusFaceButton;

    public CharacterSwitcherPane(
        Translation translation,
        CharacterService characterService,
        SelectionController<CharacterController> characterSelectionController,
        CustomMaidSceneService customMaidSceneService,
        EditModeMaidService editModeMaidService)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.editModeMaidService = editModeMaidService ?? throw new ArgumentNullException(nameof(editModeMaidService));

        this.characterService.CalledCharacters += OnCharactersCalled;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        characterDropdown = new([], formatter: CharacterFormatter);
        characterDropdown.SelectionChanged += OnSelectionChanged;

        editToggle = new(new LocalizableGUIContent(translation, "characterSwitcher", "editToggle"));
        editToggle.ControlEvent += OnEditToggleChanged;

        focusBodyButton = new(new LocalizableGUIContent(translation, "characterSwitcher", "focusBodyButton"));
        focusBodyButton.ControlEvent += OnFocusBodyButtonPushed;

        focusFaceButton = new(new LocalizableGUIContent(translation, "characterSwitcher", "focusFaceButton"));
        focusFaceButton.ControlEvent += OnFocusFaceButtonPushed;

        static CharacterDropdownItem CharacterFormatter(CharacterController character, int index) =>
            new(character);
    }

    public override void Draw()
    {
        if (characterService.Count is 0)
            return;

        var boxSize = UIUtility.Scaled(BoxSize);

        var guiEnabled = Parent.Enabled && characterService.Count > 0;

        GUILayout.BeginHorizontal();
        if (customMaidSceneService.EditScene)
        {
            var originalColour = GUI.color;

            GUI.enabled = guiEnabled && characterSelectionController.Current?.CharacterModel != editModeMaidService.EditingCharacter;

            if (Parent.Enabled)
                GUI.color = originalColour with { a = 2f };

            editToggle.Draw();

            GUI.color = originalColour;

            GUI.enabled = guiEnabled;
        }

        GUI.enabled = guiEnabled;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        focusBodyButton.Draw(GUILayout.ExpandWidth(false));
        focusFaceButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

        GUILayout.Label($"{characterSelectionController.Current?.Slot + 1}", slotStyle);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        var windowWidth = Parent.WindowRect.width;
        var dropdownWidth = windowWidth - UIUtility.Scaled(125);

        characterDropdown.Draw(GUILayout.Width(dropdownWidth), GUILayout.Height(boxSize));

        GUILayout.BeginVertical(GUILayout.Height(boxSize));

        var buttonOptions = new[]
        {
            GUILayout.Width(UIUtility.Scaled(25)), GUILayout.ExpandHeight(true),
        };

        if (GUILayout.Button(Symbols.UpChevron, Symbols.IconButtonStyle, buttonOptions))
            characterDropdown.CyclePrevious();

        if (GUILayout.Button(Symbols.DownChevron, Symbols.IconButtonStyle, buttonOptions))
            characterDropdown.CycleNext();

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();
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

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e) =>
        characterDropdown.SetItemsWithoutNotify(characterService);

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
