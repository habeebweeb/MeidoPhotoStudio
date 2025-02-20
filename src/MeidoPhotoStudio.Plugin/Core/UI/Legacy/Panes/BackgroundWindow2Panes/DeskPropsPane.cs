using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DeskPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly DeskPropRepository deskPropRepository;
    private readonly Dropdown<int> propCategoryDropdown;
    private readonly Dropdown<DeskPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label noPropsLabel;
    private readonly SearchBar<DeskPropModel> searchBar;
    private readonly LazyStyle noPropsLabelStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    public DeskPropsPane(Translation translation, PropService propService, DeskPropRepository deskPropRepository)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.deskPropRepository = deskPropRepository ?? throw new ArgumentNullException(nameof(deskPropRepository));

        translation.Initialized += OnTranslationInitialized;

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "deskPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        var categories = this.deskPropRepository.CategoryIDs.OrderBy(id => id).ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(
            this.deskPropRepository[propCategoryDropdown.SelectedItem],
            formatter: PropFormatter);

        addPropButton = new(new LocalizableGUIContent(translation, "propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        noPropsLabel = new(new LocalizableGUIContent(translation, "propsPane", "noProps"));

        static LabelledDropdownItem PropFormatter(DeskPropModel prop, int index) =>
            new(prop.Name);

        LabelledDropdownItem CategoryFormatter(int id, int index) =>
            new(translation["deskPropCategories", id.ToString()]);

        IEnumerable<DeskPropModel> SearchSelector(string query) =>
            deskPropRepository.Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase));

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

        if (deskPropRepository[propCategoryDropdown.SelectedItem].Count is 0)
        {
            noPropsLabel.Draw(noPropsLabelStyle);

            return;
        }

        DrawDropdown(propDropdown);

        UIUtility.DrawBlackLine();

        addPropButton.Draw();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<DeskPropModel> e) =>
        propService.Add(e.Item);

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(deskPropRepository[propCategoryDropdown.SelectedItem]);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
