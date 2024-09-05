using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class GamePropsPane : BasePane
{
    private readonly PropService propService;
    private readonly PhotoBgPropRepository gamePropRepository;
    private readonly Dropdown<string> propCategoryDropdown;
    private readonly Dropdown<PhotoBgPropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label noPropsLabel;

    public GamePropsPane(PropService propService, PhotoBgPropRepository gamePropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.gamePropRepository = gamePropRepository ?? throw new ArgumentNullException(nameof(gamePropRepository));

        var categories = this.gamePropRepository.Categories.Where(category => gamePropRepository[category].Any()).ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(this.gamePropRepository[propCategoryDropdown.SelectedItem], formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        noPropsLabel = new(Translation.Get("propsPane", "noProps"));

        static string CategoryFormatter(string category, int index) =>
            Translation.Get("gamePropCategories", category);

        static string PropFormatter(PhotoBgPropModel prop, int index) =>
            prop.Name;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        if (gamePropRepository[propCategoryDropdown.SelectedItem].Count is 0)
        {
            noPropsLabel.Draw();

            return;
        }

        DrawDropdown(propDropdown);

        MpsGui.BlackLine();

        addPropButton.Draw();

        void DrawDropdown<T>(Dropdown<T> dropdown)
        {
            GUILayout.BeginHorizontal();

            const int ScrollBarWidth = 23;

            var buttonAndScrollbarSize = ScrollBarWidth + Utility.GetPix(20) * 2 + 5;
            var dropdownButtonWidth = parent.WindowRect.width - buttonAndScrollbarSize;

            dropdown.Draw(GUILayout.Width(dropdownButtonWidth));

            var arrowLayoutOptions = GUILayout.ExpandWidth(false);

            if (GUILayout.Button("<", arrowLayoutOptions))
                dropdown.CyclePrevious();

            if (GUILayout.Button(">", arrowLayoutOptions))
                dropdown.CycleNext();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        propCategoryDropdown.Reformat();
        propDropdown.Reformat();

        addPropButton.Label = Translation.Get("propsPane", "addProp");
        noPropsLabel.Text = Translation.Get("propsPane", "noProps");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(gamePropRepository[propCategoryDropdown.SelectedItem], 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
