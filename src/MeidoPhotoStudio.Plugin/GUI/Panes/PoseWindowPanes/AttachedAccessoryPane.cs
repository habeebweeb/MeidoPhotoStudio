using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core;
using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin;

public class AttachedAccessoryPane : BasePane
{
    private const int NoAccessory = 0;
    private const string NoAccessoryTranslationKey = "noAccessory";

    private static readonly string[] AccessoryCategoryTranslationKeys = ["upperAccessoryTab", "lowerAccessoryTab"];
    private static readonly MPN[] AccessoryCategory = [MPN.kousoku_upper, MPN.kousoku_lower];

    private readonly MenuPropRepository menuPropRepository;
    private readonly SelectionController<CharacterController> characterSelectionController;
    private readonly PaneHeader paneHeader;
    private readonly Dropdown accessoryDropdown;
    private readonly SelectionGrid accessoryCategoryGrid;
    private readonly Button detachAllAccessoriesButton;

    private bool menuDatabaseBusy;

    public AttachedAccessoryPane(
        MenuPropRepository menuPropRepository, SelectionController<CharacterController> characterSelectionController)
    {
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.characterSelectionController = characterSelectionController ?? throw new ArgumentNullException(nameof(characterSelectionController));

        this.characterSelectionController.Selected += OnCharacterSelectionChanged;

        paneHeader = new(Translation.Get("attachMpnPropPane", "header"), true);

        accessoryCategoryGrid = new(Translation.GetArray("attachMpnPropPane", AccessoryCategoryTranslationKeys));
        accessoryCategoryGrid.ControlEvent += OnAccessoryCategoryChanged;

        accessoryDropdown = new([":)"]);
        accessoryDropdown.SelectionChange += OnAccessoryChanged;

        detachAllAccessoriesButton = new(Translation.Get("attachMpnPropPane", "detachAllButton"));
        detachAllAccessoriesButton.ControlEvent += OnDetachAllButtonPressed;

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
            accessoryDropdown.SetDropdownItems(AccessoryList());
    }

    private ClothingController CurrentClothing =>
        characterSelectionController.Current?.Clothing;

    private MPN CurrentCategory =>
        AccessoryCategory[accessoryCategoryGrid.SelectedItemIndex];

    public override void Draw()
    {
        GUI.enabled = !menuDatabaseBusy && characterSelectionController.Current is not null;

        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        accessoryCategoryGrid.Draw();
        MpsGui.BlackLine();

        DrawDropdown(accessoryDropdown);
        MpsGui.BlackLine();

        detachAllAccessoriesButton.Draw();

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
        paneHeader.Label = Translation.Get("attachMpnPropPane", "header");
        accessoryCategoryGrid.SetItemsWithoutNotify(Translation.GetArray("attachMpnPropPane", AccessoryCategoryTranslationKeys));
        detachAllAccessoriesButton.Label = Translation.Get("attachMpnPropPane", "detachAllButton");

        accessoryDropdown.SetDropdownItemsWithoutNotify(AccessoryList());
    }

    private void OnAccessoryCategoryChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        accessoryDropdown.SetDropdownItemsWithoutNotify(AccessoryList());

        UpdateAccessoryDropdownSelection();
    }

    private void OnAccessoryChanged(object sender, EventArgs e)
    {
        if (menuPropRepository.Busy)
            return;

        if (CurrentClothing is null)
            return;

        if (accessoryDropdown.SelectedItemIndex is NoAccessory)
            CurrentClothing.DetachAccessory(CurrentCategory);
        else
            CurrentClothing.AttachAccessory(menuPropRepository[CurrentCategory][accessoryDropdown.SelectedItemIndex - 1]);
    }

    private void OnDetachAllButtonPressed(object sender, EventArgs e)
    {
        if (CurrentClothing is null)
            return;

        CurrentClothing.DetachAllAccessories();

        accessoryDropdown.SetIndexWithoutNotify(NoAccessory);
    }

    private void OnCharacterSelectionChanged(object sender, SelectionEventArgs<CharacterController> e)
    {
        if (e.Selected is null)
            return;

        UpdateAccessoryDropdownSelection();
    }

    private void UpdateAccessoryDropdownSelection()
    {
        if (CurrentClothing is null)
            return;

        var currentAccessory = CurrentCategory is MPN.kousoku_lower
            ? CurrentClothing.AttachedLowerAccessory
            : CurrentClothing.AttachedUpperAccessory;

        if (currentAccessory is null)
        {
            accessoryDropdown.SetIndexWithoutNotify(NoAccessory);

            return;
        }

        var accessoryIndex = 0;

        foreach (var (index, accessory) in menuPropRepository[currentAccessory.CategoryMpn].WithIndex())
        {
            if (!string.Equals(accessory.ID, currentAccessory.ID, StringComparison.OrdinalIgnoreCase))
                continue;

            accessoryIndex = index;

            break;
        }

        accessoryDropdown.SetIndexWithoutNotify(accessoryIndex);
    }

    private string[] AccessoryList()
    {
        var accessoryList = menuPropRepository.Busy
            ? [Translation.Get("systemMessage", "initializing")]
            : new[] { Translation.Get("attachMpnPropPane", NoAccessoryTranslationKey) }
                .Concat(menuPropRepository[AccessoryCategory[accessoryCategoryGrid.SelectedItemIndex]]
                    .Select(prop => Translation.Get("mpnAttachPropNames", prop.Filename)))
                .ToArray();

        return accessoryList;
    }
}
