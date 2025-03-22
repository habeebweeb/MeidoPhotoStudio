using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HandItemPropsPane : BasePane
{
    private static readonly MPN HandItem = SafeMpn.GetValue(nameof(MPN.handitem));

    private readonly PropService propService;
    private readonly Dropdown<MenuFilePropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label initializingLabel;
    private readonly Label noHandItemsLabel;
    private readonly SearchBar<MenuFilePropModel> searchBar;
    private readonly LazyStyle noPropsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(Label.Style)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private bool menuDatabaseBusy = false;
    private bool hasHandItems;

    public HandItemPropsPane(
        Translation translation,
        PropService propService,
        MenuPropRepository menuPropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        _ = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "handItemPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        propDropdown = new(formatter: PropFormatter);

        addPropButton = new(new LocalizableGUIContent(translation, "propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        initializingLabel = new(new LocalizableGUIContent(translation, "systemMessage", "initializing"));
        noHandItemsLabel = new(new LocalizableGUIContent(translation, "handItemPropsPane", "noHandItems"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;
            menuPropRepository.InitializedProps += OnMenuDatabaseIndexed;
        }
        else
        {
            Initialize();
        }

        static LabelledDropdownItem PropFormatter(MenuFilePropModel prop, int index) =>
            new(prop.Name);

        void OnMenuDatabaseIndexed(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;
            menuPropRepository.InitializedProps -= OnMenuDatabaseIndexed;

            Initialize();
        }

        void Initialize()
        {
            var handItems = menuPropRepository.ContainsCategory(HandItem)
                ? (IEnumerable<MenuFilePropModel>)menuPropRepository[HandItem].OrderBy(static model => model.Filename)
                : [];

            propDropdown.SetItems(handItems);

            hasHandItems = handItems.Any();
        }

        IEnumerable<MenuFilePropModel> SearchSelector(string query) =>
            menuPropRepository.Busy || !menuPropRepository.ContainsCategory(HandItem)
                ? []
                : menuPropRepository[HandItem].Where(model =>
                    model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileNameWithoutExtension(model.Filename).Contains(query, StringComparison.OrdinalIgnoreCase));

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            propDropdown.Reformat();
            searchBar.Reformat();
        }
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

        if (!hasHandItems)
        {
            noHandItemsLabel.Draw(noPropsLabelStyle);

            return;
        }

        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        addPropButton.Draw();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<MenuFilePropModel> e) =>
        propService.Add(e.Item);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
