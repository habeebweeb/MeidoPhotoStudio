using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class GamePropsPane : BasePane
{
    private readonly PropService propService;
    private readonly PhotoBgPropRepository gamePropRepository;
    private readonly Dropdown<string> propCategoryDropdown;
    private readonly Dropdown<PhotoBgPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label noPropsLabel;
    private readonly SearchBar<PhotoBgPropModel> searchBar;
    private readonly LazyStyle noPropsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    public GamePropsPane(Translation translation, PropService propService, PhotoBgPropRepository gamePropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.gamePropRepository = gamePropRepository ?? throw new ArgumentNullException(nameof(gamePropRepository));

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "gamePropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        var categories = this.gamePropRepository.Categories.Where(category => gamePropRepository[category].Any()).ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(this.gamePropRepository[propCategoryDropdown.SelectedItem], formatter: PropFormatter);

        addPropButton = new(new LocalizableGUIContent(translation, "propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        noPropsLabel = new(new LocalizableGUIContent(translation, "propsPane", "noProps"));

        LabelledDropdownItem CategoryFormatter(string category, int index) =>
            new(translation["gamePropCategories", category]);

        static LabelledDropdownItem PropFormatter(PhotoBgPropModel prop, int index) =>
            new(prop.Name);

        IEnumerable<PhotoBgPropModel> SearchSelector(string query) =>
            gamePropRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            propCategoryDropdown.Reformat();
            propDropdown.Reformat();
            searchBar.Reformat();
        }
    }

    public override void Draw()
    {
        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propCategoryDropdown);

        if (gamePropRepository[propCategoryDropdown.SelectedItem].Count is 0)
        {
            noPropsLabel.Draw(noPropsLabelStyle);

            return;
        }

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        addPropButton.Draw();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<PhotoBgPropModel> e) =>
        propService.Add(e.Item);

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(gamePropRepository[propCategoryDropdown.SelectedItem], 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
