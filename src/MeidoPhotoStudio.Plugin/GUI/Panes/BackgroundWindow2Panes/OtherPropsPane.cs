using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class OtherPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly OtherPropRepository otherPropRepository;
    private readonly Dropdown propCategoryDropdown;
    private readonly Dropdown propDropdown;
    private readonly Button addPropButton;
    private readonly string[] categories;

    public OtherPropsPane(PropService propService, OtherPropRepository otherPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.otherPropRepository = otherPropRepository ?? throw new ArgumentNullException(nameof(otherPropRepository));

        categories = otherPropRepository.Categories.ToArray();

        propCategoryDropdown = new(Translation.GetArray("otherPropCategories", categories));
        propCategoryDropdown.SelectionChange += OnPropCategoryDropdownChanged;

        propDropdown = new(PropList(0));

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);
        DrawDropdown(propDropdown);

        MpsGui.BlackLine();

        addPropButton.Draw();

        static void DrawDropdown(Dropdown dropdown)
        {
            var arrowLayoutOptions = new[]
            {
                GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false),
            };

            const float dropdownButtonWidth = 185f;

            var dropdownLayoutOptions = new[]
            {
                GUILayout.Width(dropdownButtonWidth),
            };

            GUILayout.BeginHorizontal();
            dropdown.Draw(dropdownLayoutOptions);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.Step(-1);

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.Step(1);

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        propCategoryDropdown.SetDropdownItemsWithoutNotify(Translation.GetArray("otherPropCategories", categories));
        propDropdown.SetDropdownItemsWithoutNotify(PropList(propCategoryDropdown.SelectedItemIndex));
        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetDropdownItems(PropList(propCategoryDropdown.SelectedItemIndex), 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(otherPropRepository[categories[propCategoryDropdown.SelectedItemIndex]][propDropdown.SelectedItemIndex]);

    private string[] PropList(int category) =>
        otherPropRepository[categories[category]]
            .Select(prop => prop.Name)
            .ToArray();
}
