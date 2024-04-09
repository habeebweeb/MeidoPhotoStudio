using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class HandItemPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly Dropdown propDropdown;
    private readonly Button addPropButton;

    private string initializingMessage;
    private bool menuDatabaseBusy = false;

    public HandItemPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));

        var items = PropList();

        propDropdown = new(items);

        addPropButton = new(Translation.Get("propsPane", "addProp"));
        addPropButton.ControlEvent += OnAddPropButtonPressed;

        initializingMessage = Translation.Get("systemMessage", "initializing");

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;
            menuPropRepository.InitializedProps += OnMenuDatabaseIndexed;
        }

        void OnMenuDatabaseIndexed(object sender, EventArgs e)
        {
            propDropdown.SetDropdownItems(PropList());

            menuDatabaseBusy = false;
            menuPropRepository.InitializedProps -= OnMenuDatabaseIndexed;
        }
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            GUILayout.Label(initializingMessage);

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
            propDropdown.Step(-1);

        if (GUILayout.Button(">", arrowLayoutOptions))
            propDropdown.Step(1);

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        addPropButton.Draw();
    }

    protected override void ReloadTranslation()
    {
        initializingMessage = Translation.Get("systemMessage", "initializing");
        propDropdown.SetDropdownItemsWithoutNotify(PropList(), propDropdown.SelectedItemIndex);

        addPropButton.Label = Translation.Get("propsPane", "addProp");
    }

    private void OnAddPropButtonPressed(object sender, EventArgs e) =>
        propService.Add(menuPropRepository[MPN.handitem][propDropdown.SelectedItemIndex]);

    private string[] PropList() =>
        menuPropRepository.Busy
            ? new[] { Translation.Get("systemMessage", "initializing") }
            : menuPropRepository[MPN.handitem]
                .Select(prop => prop.Name)
                .ToArray();
}
