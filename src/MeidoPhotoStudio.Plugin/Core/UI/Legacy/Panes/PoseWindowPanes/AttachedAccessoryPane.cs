using System.ComponentModel;

using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class AttachedAccessoryPane : BasePane
{
    private const int NoAccessoryIndex = 0;

    private readonly MenuPropRepository menuPropRepository;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly Dropdown<MenuFilePropModel> accessoryDropdown;
    private readonly Toggle.Group accessoryTypeGroup;
    private readonly Button detachAllAccessoriesButton;
    private readonly Label initializingLabel;
    private readonly Label noAccessoriesLabel;

    private bool hasAccessories;
    private bool menuDatabaseBusy;

    public AttachedAccessoryPane(
        Translation translation,
        MenuPropRepository menuPropRepository,
        SelectionController<CharacterController> characterSelectionController)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selecting += OnCharacterSelectionChanging;
        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        translation.Initialized += OnTranslationInitialized;

        var upperToggle = new Toggle(
            new LocalizableGUIContent(translation, "attachMpnPropPane", "upperAccessoryTab"), true);

        upperToggle.ControlEvent += OnAccessoryTypeToggleChanged(SafeMpn.kousoku_upper);

        var lowerToggle = new Toggle(new LocalizableGUIContent(translation, "attachMpnPropPane", "lowerAccessoryTab"));

        lowerToggle.ControlEvent += OnAccessoryTypeToggleChanged(SafeMpn.kousoku_lower);

        accessoryTypeGroup = [upperToggle, lowerToggle];

        accessoryDropdown = new((model, _) =>
            new LabelledDropdownItem(model is null
                ? translation["attachMpnPropPane", "noAccessory"]
                : translation["mpnAttachPropNames", model.Filename]));

        accessoryDropdown.SelectionChanged += OnAccessoryChanged;

        detachAllAccessoriesButton = new(
            new LocalizableGUIContent(translation, "attachMpnPropPane", "detachAllButton"));

        detachAllAccessoriesButton.ControlEvent += OnDetachAllButtonPressed;

        initializingLabel = new(new LocalizableGUIContent(translation, "systemMessage", "initializing"));
        noAccessoriesLabel = new(new LocalizableGUIContent(translation, "attachMpnPropPane", "noAccessories"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        void OnMenuDatabaseReady(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;

            Initialize();

            menuPropRepository.InitializedProps -= OnMenuDatabaseReady;
        }

        void Initialize() =>
            accessoryDropdown.SetItems(AccessoryList());

        EventHandler OnAccessoryTypeToggleChanged(MPN mpn) =>
            (sender, _) =>
            {
                if (sender is not Toggle { Value: true })
                    return;

                CurrentCategory = mpn;
                ChangeAccessoryType();
            };

        void OnTranslationInitialized(object sender, EventArgs e) =>
            accessoryDropdown.Reformat();
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    private MPN CurrentCategory { get; set; } = SafeMpn.kousoku_upper;

    public override void Draw()
    {
        var enabled = Parent.Enabled && characterSelectionController.Current is not null;

        GUI.enabled = enabled;

        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

        GUILayout.BeginHorizontal();

        foreach (var accessoryType in accessoryTypeGroup)
            accessoryType.Draw();

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        if (hasAccessories)
            DrawDropdown(accessoryDropdown);
        else
            noAccessoriesLabel.Draw();

        UIUtility.DrawBlackLine();

        detachAllAccessoriesButton.Draw();
    }

    private void ChangeAccessoryType()
    {
        if (menuPropRepository.Busy)
            return;

        accessoryDropdown.SetItemsWithoutNotify(AccessoryList());

        UpdateAccessoryDropdownSelection();
    }

    private void OnAccessoryChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        if (CurrentClothing is null)
            return;

        if (accessoryDropdown.SelectedItemIndex is NoAccessoryIndex)
        {
            if (CurrentCategory == SafeMpn.kousoku_lower)
                CurrentClothing.DetachLowerAccessory();
            else
                CurrentClothing.DetachUpperAccessory();
        }
        else
        {
            CurrentClothing.AttachAccessory(accessoryDropdown.SelectedItem);
        }
    }

    private void OnDetachAllButtonPressed(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.DetachAllAccessories();

        accessoryDropdown.SetSelectedIndexWithoutNotify(NoAccessoryIndex);
    }

    private void OnCharacterSelectionChanging(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged -= OnClothingPropertyChanged;
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        e.Selected.Clothing.PropertyChanged += OnClothingPropertyChanged;

        UpdateAccessoryDropdownSelection();
    }

    private void OnClothingPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var controller = (ClothingController)sender;

        (var changedCategory, var changedAccessory) = e.PropertyName switch
        {
            nameof(ClothingController.AttachedLowerAccessory) => (SafeMpn.kousoku_lower, controller.AttachedLowerAccessory),
            nameof(ClothingController.AttachedUpperAccessory) => (SafeMpn.kousoku_upper, controller.AttachedUpperAccessory),
            _ => (MPN.null_mpn, null),
        };

        if (changedCategory is MPN.null_mpn || CurrentCategory != changedCategory)
            return;

        if (changedAccessory is null)
        {
            accessoryDropdown.SetSelectedIndexWithoutNotify(0);

            return;
        }

        var accessoryIndex = accessoryDropdown.Skip(1).IndexOf(changedAccessory);

        if (accessoryIndex < 0)
            return;

        accessoryDropdown.SetSelectedIndexWithoutNotify(accessoryIndex + 1);
    }

    private void UpdateAccessoryDropdownSelection()
    {
        if (CurrentClothing is null)
            return;

        var currentAccessory = CurrentCategory == SafeMpn.kousoku_lower
            ? CurrentClothing.AttachedLowerAccessory
            : CurrentClothing.AttachedUpperAccessory;

        if (currentAccessory is null)
        {
            accessoryDropdown.SetSelectedIndexWithoutNotify(NoAccessoryIndex);

            return;
        }

        var accessoryIndex = accessoryDropdown.Skip(1).IndexOf(currentAccessory);

        if (accessoryIndex < 0)
            return;

        accessoryDropdown.SetSelectedIndexWithoutNotify(accessoryIndex + 1);
    }

    private IEnumerable<MenuFilePropModel> AccessoryList()
    {
        var category = CurrentCategory;
        var accessories = !menuPropRepository.Busy && menuPropRepository.ContainsCategory(category)
            ? new MenuFilePropModel[] { null }
                .Concat(menuPropRepository[category].OrderBy(static model => model.Filename))
            : [];

        hasAccessories = accessories.Any();

        return accessories;
    }
}
