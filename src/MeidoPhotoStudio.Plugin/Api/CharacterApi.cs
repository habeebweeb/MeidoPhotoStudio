using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Api;

public class CharacterApi : ApiBase
{
    private readonly CharacterService characterService;

    private readonly EditModeMaidService editModeMaidService;

    private readonly IKDragHandleService ikDragHandleService;

    private readonly SelectionController<CharacterController> characterSelectionController;

    public CharacterApi(
        PluginCore pluginCore,
        CharacterService characterService,
        EditModeMaidService editModeMaidService,
        IKDragHandleService ikDragHandleService,
        SelectionController<CharacterController> characterSelectionController)
        : base(pluginCore)
    {
        this.characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
        this.editModeMaidService = editModeMaidService ?? throw new ArgumentNullException(nameof(editModeMaidService));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterService.CallingCharacters += OnCharactersCalling;
        this.characterService.CalledCharacters += OnCharactersCalled;
        this.editModeMaidService.ChangingEditMaid += OnEditMaidChanging;
        this.editModeMaidService.ChangedEditMaid += OnEditMaidChanged;
    }

    public event EventHandler<CharacterServiceEventArgs> CallingCharacters;

    public event EventHandler<CharacterServiceEventArgs> CalledCharacters;

    public event EventHandler<EditModeMaidServiceEventArgs> ChangingEditMaid;

    public event EventHandler<EditModeMaidServiceEventArgs> ChangedEditMaid;

    public bool Busy
    {
        get
        {
            base.Valid();

            return characterService.Busy;
        }
    }

    public IEnumerable<CharacterController> ActiveCharacters
    {
        get
        {
            Valid();

            return characterService;
        }
    }

    public CharacterController SelectedCharacter
    {
        get
        {
            Valid();

            return characterSelectionController.Current;
        }

        set
        {
            Valid();

            _ = value ?? throw new ArgumentNullException(nameof(value));

            characterSelectionController.Select(value);
        }
    }

    public Maid OriginalEditModeMaid
    {
        get
        {
            base.Valid();

            return editModeMaidService.OriginalEditingCharacter?.Maid;
        }
    }

    public int SelectedCharacterIndex
    {
        get
        {
            Valid();

            return characterSelectionController.CurrentIndex;
        }

        set
        {
            Valid();

            if ((uint)value >= characterService.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            characterSelectionController.Select(value);
        }
    }

    public int CharacterCount
    {
        get
        {
            Valid();

            return characterService.Count;
        }
    }

    public CharacterController this[int index]
    {
        get
        {
            Valid();

            return (uint)index >= characterService.Count
                ? throw new ArgumentOutOfRangeException(nameof(index))
                : characterService[index];
        }
    }

    public CharacterController GetActiveCharacterByMaid(Maid maid)
    {
        Valid();

        _ = maid ? maid : throw new ArgumentNullException(nameof(maid));

        return characterService.FirstOrDefault(character => character.Maid.ValueEquals(maid));
    }

    public CharacterController GetCharacterControllerByID(string guid)
    {
        Valid();

        return string.IsNullOrEmpty(guid)
            ? throw new ArgumentException($"'{nameof(guid)}' cannot be null or empty.", nameof(guid))
            : characterService.FirstOrDefault(character => string.Equals(character.ID, guid, StringComparison.OrdinalIgnoreCase));
    }

    public IKDragHandleController GetIKDragHandleController(CharacterController controller)
    {
        Valid();

        _ = controller ?? throw new ArgumentNullException(nameof(controller));

        return ikDragHandleService[controller];
    }

    protected override void Valid()
    {
        base.Valid();

        if (characterService.Busy)
            throw new InvalidOperationException("Characters are busy");
    }

    private void OnCharactersCalling(object sender, CharacterServiceEventArgs e) =>
        CallingCharacters?.Invoke(this, e);

    private void OnCharactersCalled(object sender, CharacterServiceEventArgs e) =>
        CalledCharacters?.Invoke(this, e);

    private void OnEditMaidChanging(object sender, EditModeMaidServiceEventArgs e) =>
        ChangingEditMaid?.Invoke(this, e);

    private void OnEditMaidChanged(object sender, EditModeMaidServiceEventArgs e) =>
        ChangedEditMaid?.Invoke(this, e);
}
