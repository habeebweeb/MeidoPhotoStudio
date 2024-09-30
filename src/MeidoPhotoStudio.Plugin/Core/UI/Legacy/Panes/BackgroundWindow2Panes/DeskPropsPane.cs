using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
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

    public DeskPropsPane(PropService propService, DeskPropRepository deskPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.deskPropRepository = deskPropRepository ?? throw new ArgumentNullException(nameof(deskPropRepository));

        var categories = this.deskPropRepository.CategoryIDs.OrderBy(id => id).ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(
            this.deskPropRepository[propCategoryDropdown.SelectedItem],
            formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        noPropsLabel = new(Translation.Get("propsPane", "noProps"));

        static LabelledDropdownItem CategoryFormatter(int id, int index) =>
            new(Translation.Get("deskpropCategories", id.ToString()));

        static LabelledDropdownItem PropFormatter(DeskPropModel prop, int index) =>
            new(prop.Name);
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        if (deskPropRepository[propCategoryDropdown.SelectedItem].Count is 0)
        {
            noPropsLabel.Draw();

            return;
        }

        DrawDropdown(propDropdown);

        MpsGui.BlackLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        propCategoryDropdown.Reformat();
        propDropdown.Reformat();

        addPropButton.Label = Translation.Get("propsPane", "addProp");
        noPropsLabel.Text = Translation.Get("propsPane", "noProps");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(deskPropRepository[propCategoryDropdown.SelectedItem]);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
