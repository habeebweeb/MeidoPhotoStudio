using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class OtherPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly OtherPropRepository otherPropRepository;
    private readonly Dropdown<string> propCategoryDropdown;
    private readonly Dropdown<OtherPropModel> propDropdown;
    private readonly Button addPropButton;

    public OtherPropsPane(PropService propService, OtherPropRepository otherPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.otherPropRepository = otherPropRepository ?? throw new ArgumentNullException(nameof(otherPropRepository));

        var categories = otherPropRepository.Categories.ToArray();

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(this.otherPropRepository[propCategoryDropdown.SelectedItem], formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        static string CategoryFormatter(string category, int index) =>
            Translation.Get("otherPropCategories", category);

        static string PropFormatter(OtherPropModel prop, int index) =>
            prop.Name;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);
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
        propCategoryDropdown.Reformat();
        propDropdown.Reformat();
        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        propDropdown.SetItems(otherPropRepository[propCategoryDropdown.SelectedItem]);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
