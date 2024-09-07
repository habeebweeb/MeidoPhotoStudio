using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class BackgroundPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly BackgroundPropRepository backgroundPropRepository;
    private readonly Dropdown<BackgroundCategory> propCategoryDropdown;
    private readonly Dropdown<BackgroundPropModel> propDropdown;
    private readonly Button addPropButton;

    public BackgroundPropsPane(PropService propService, BackgroundPropRepository backgroundPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.backgroundPropRepository = backgroundPropRepository ?? throw new ArgumentNullException(nameof(backgroundPropRepository));

        var categories = backgroundPropRepository.Categories.OrderBy(category => category).ToArray();

        propCategoryDropdown = new(categories, Array.IndexOf(categories, BackgroundCategory.COM3D2), CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        propDropdown = new(
            this.backgroundPropRepository[propCategoryDropdown.SelectedItem],
            formatter: PropFormatter);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        static string CategoryFormatter(BackgroundCategory category, int index)
        {
            var translationKey = category switch
            {
                BackgroundCategory.CM3D2 => "cm3d2",
                BackgroundCategory.COM3D2 => "com3d2",
                BackgroundCategory.MyRoomCustom => "myRoomCustom",
                _ => throw new NotSupportedException($"{nameof(category)} is not supported"),
            };

            return Translation.Get("backgroundSource", translationKey);
        }

        static string PropFormatter(BackgroundPropModel prop, int index) =>
            prop.Name;
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);
        DrawDropdown(propDropdown);

        MpsGui.WhiteLine();

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
        propDropdown.SetItems(backgroundPropRepository[propCategoryDropdown.SelectedItem], 0);

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
