using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class HandItemPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly Dropdown<MenuFilePropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label initializingLabel;
    private readonly SearchBar<MenuFilePropModel> searchBar;

    private bool menuDatabaseBusy = false;

    public HandItemPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

        searchBar = new(SearchSelector, PropFormatter)
        {
            Placeholder = Translation.Get("handItemPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        propDropdown = new(formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;
            menuPropRepository.InitializedProps += OnMenuDatabaseIndexed;
        }
        else
        {
            propDropdown.SetItems(menuPropRepository[MPN.handitem]);
        }

        static LabelledDropdownItem PropFormatter(MenuFilePropModel prop, int index) =>
            new(prop.Name);

        void OnMenuDatabaseIndexed(object sender, EventArgs e)
        {
            propDropdown.SetItems(menuPropRepository[MPN.handitem]);

            menuDatabaseBusy = false;
            menuPropRepository.InitializedProps -= OnMenuDatabaseIndexed;
        }

        IEnumerable<MenuFilePropModel> SearchSelector(string query) =>
            menuPropRepository.Busy
                ? []
                : menuPropRepository[MPN.handitem].Where(model =>
                    model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    Path.GetFileNameWithoutExtension(model.Filename).Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

        searchBar.Draw();

        DrawDropdown(propDropdown);

        MpsGui.BlackLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
        propDropdown.Reformat();

        addPropButton.Label = Translation.Get("propsPane", "addProp");
        searchBar.Placeholder = Translation.Get("handItemPropsPane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<MenuFilePropModel> e) =>
        propService.Add(e.Item);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
