using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class GamePropsPane : BasePane
{
    private readonly PropService propService;
    private readonly PhotoBgPropRepository gamePropRepository;
    private readonly Dropdown propCategoryDropdown;
    private readonly Dropdown propDropdown;
    private readonly Button addPropButton;
    private readonly string[] categories;

    private bool hasProps;

    public GamePropsPane(PropService propService, PhotoBgPropRepository gamePropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.gamePropRepository = gamePropRepository ?? throw new ArgumentNullException(nameof(gamePropRepository));

        categories = gamePropRepository.Categories.Where(category => gamePropRepository[category].Any()).ToArray();

        propCategoryDropdown = new(Translation.GetArray("gamePropCategories", categories));
        propCategoryDropdown.SelectionChange += OnPropCategoryDropdownChanged;

        propDropdown = new(PropList(0));

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        var guiEnabled = GUI.enabled;

        GUI.enabled = hasProps;

        DrawDropdown(propDropdown);

        MpsGui.BlackLine();

        addPropButton.Draw();

        GUI.enabled = guiEnabled;

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
        base.ReloadTranslation();

        propCategoryDropdown.SetDropdownItemsWithoutNotify(
            Translation.GetArray("gamePropCategories", categories));
        propDropdown.SetDropdownItemsWithoutNotify(PropList(propCategoryDropdown.SelectedItemIndex));

        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetDropdownItems(PropList(propCategoryDropdown.SelectedItemIndex), 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(gamePropRepository[categories[propCategoryDropdown.SelectedItemIndex]][propDropdown.SelectedItemIndex]);

    private string[] PropList(int category)
    {
        var propList = gamePropRepository[categories[category]]
            .Select(prop => prop.Name)
            .ToArray();

        hasProps = propList.Length is not 0;

        if (propList.Length is 0)
            propList = new[] { Translation.Get("systemMessage", "noProps") };

        return propList;
    }
}
