using MeidoPhotoStudio.Plugin.Core.Database.Character;
using MeidoPhotoStudio.Plugin.Framework.Service;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class AutoEditMaidService
{
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly EditModeMaidService editModeMaidService;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private bool enabled;

    public AutoEditMaidService(
        CustomMaidSceneService customMaidSceneService,
        EditModeMaidService editModeMaidService,
        SelectionController<CharacterController> characterSelectionController)
    {
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.editModeMaidService = editModeMaidService ?? throw new ArgumentNullException(nameof(editModeMaidService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.editModeMaidService.ChangedEditMaid += OnEditMaidChanged;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;
    }

    public bool Enabled
    {
        get => enabled;
        set
        {
            if (enabled == value)
                return;

            enabled = value;

            if (!enabled)
                return;

            if (characterSelectionController.Current is not CharacterController character)
                return;

            ChangeEditingCharacter(character.CharacterModel);
        }
    }

    private void OnEditMaidChanged(object sender, EditModeMaidServiceEventArgs e)
    {
        if (characterSelectionController.Current is not CharacterController character)
            return;

        if (Equals(e.Character, character.CharacterModel))
            return;

        ChangeEditingCharacter(character.CharacterModel);
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is not CharacterController character)
            return;

        ChangeEditingCharacter(character.CharacterModel);
    }

    private void ChangeEditingCharacter(CharacterModel character)
    {
        if (!customMaidSceneService.EditScene)
            return;

        if (!Enabled)
            return;

        if (character is null)
            return;

        if (Equals(editModeMaidService.EditingCharacter, character))
            return;

        editModeMaidService.SetEditingCharacter(character);
    }
}
