using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class HandItemPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly Dropdown<MenuFilePropModel> propDropdown;
    private readonly Button addPropButton;
    private readonly Label initializingLabel;

    private bool menuDatabaseBusy = false;

    public HandItemPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

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

        static string PropFormatter(MenuFilePropModel prop, int index) =>
            prop.Name;

        void OnMenuDatabaseIndexed(object sender, EventArgs e)
        {
            propDropdown.SetItems(menuPropRepository[MPN.handitem]);

            menuDatabaseBusy = false;
            menuPropRepository.InitializedProps -= OnMenuDatabaseIndexed;
        }
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

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

        propDropdown.Draw(dropdownLayoutOptions);

        if (GUILayout.Button("<", arrowLayoutOptions))
            propDropdown.CyclePrevious();

        if (GUILayout.Button(">", arrowLayoutOptions))
            propDropdown.CycleNext();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
        propDropdown.Reformat();

        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(propDropdown.SelectedItem);
}
