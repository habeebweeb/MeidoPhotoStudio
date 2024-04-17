using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly BackgroundPropRepository backgroundPropRepository;
    private readonly Dropdown propCategoryDropdown;
    private readonly Dropdown propDropdown;
    private readonly Button addPropButton;
    private readonly BackgroundCategory[] categories;

    public BackgroundPropsPane(PropService propService, BackgroundPropRepository backgroundPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.backgroundPropRepository = backgroundPropRepository ?? throw new ArgumentNullException(nameof(backgroundPropRepository));

        categories = [.. backgroundPropRepository.Categories.OrderBy(category => category)];

        propCategoryDropdown = new(
            categories
                .Select(category => Translation.Get("backgroundSource", category.ToString()))
                .ToArray(),
            Array.IndexOf(categories, BackgroundCategory.COM3D2));

        propCategoryDropdown.SelectionChange += OnPropCategoryDropdownChanged;

        propDropdown = new(PropList(categories[propCategoryDropdown.SelectedItemIndex]));

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);
        DrawDropdown(propDropdown);

        MpsGui.WhiteLine();

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
        propCategoryDropdown.SetDropdownItemsWithoutNotify(
            categories.Select(category => Translation.Get("backgroundSource", category.ToString())).ToArray());
        propDropdown.SetDropdownItemsWithoutNotify(PropList(categories[propCategoryDropdown.SelectedItemIndex]));
        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetDropdownItems(PropList(categories[propCategoryDropdown.SelectedItemIndex]), 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(backgroundPropRepository[categories[propCategoryDropdown.SelectedItemIndex]][propDropdown.SelectedItemIndex]);

    private string[] PropList(BackgroundCategory category) =>
        backgroundPropRepository[category].Select(prop => prop.Name).ToArray();
}
