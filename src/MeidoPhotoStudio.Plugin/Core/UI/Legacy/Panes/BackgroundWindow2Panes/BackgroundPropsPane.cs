using MeidoPhotoStudio.Plugin.Core.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BackgroundPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly BackgroundPropRepository backgroundPropRepository;
    private readonly Dropdown<BackgroundCategory> propCategoryDropdown;
    private readonly Dropdown<BackgroundPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly SearchBar<BackgroundPropModel> searchBar;

    public BackgroundPropsPane(PropService propService, BackgroundPropRepository backgroundPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.backgroundPropRepository = backgroundPropRepository ?? throw new ArgumentNullException(nameof(backgroundPropRepository));

        searchBar = new(SearchSelector, PropFormatter)
        {
            Placeholder = Translation.Get("backgroundPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        var categories = backgroundPropRepository.Categories.OrderBy(category => category).ToArray();

        propCategoryDropdown = new(categories, Array.IndexOf(categories, BackgroundCategory.COM3D2), CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(
            this.backgroundPropRepository[propCategoryDropdown.SelectedItem],
            formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        static LabelledDropdownItem CategoryFormatter(BackgroundCategory category, int index)
        {
            var translationKey = category switch
            {
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new NotSupportedException($"{nameof(category)} is not supported"),
            };

            return new(Translation.Get("backgroundSource", translationKey));
        }

        static LabelledDropdownItem PropFormatter(BackgroundPropModel prop, int index) =>
            new(prop.Name);

        IEnumerable<BackgroundPropModel> SearchSelector(string query) =>
            backgroundPropRepository
                .Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    model.AssetName.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public override void Draw()
    {
        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propCategoryDropdown);
        DrawDropdown(propDropdown);

        MpsGui.WhiteLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        propCategoryDropdown.Reformat();
        propDropdown.Reformat();
        addPropButton.Label = Translation.Get("propsPane", "addProp");
        searchBar.Placeholder = Translation.Get("backgroundPropsPane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<BackgroundPropModel> e) =>
        propService.Add(e.Item);

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(backgroundPropRepository[propCategoryDropdown.SelectedItem], 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
