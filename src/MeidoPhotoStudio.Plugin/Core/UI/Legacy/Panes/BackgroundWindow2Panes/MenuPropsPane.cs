using MeidoPhotoStudio.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class MenuPropsPane : BasePane
{
    private readonly LazyStyle propButtonStyle = new(
        11,
        () => new(GUI.skin.button)
        {
            alignment = TextAnchor.UpperLeft,
            margin = new(0, 0, 0, 0),
            padding = new(0, 0, 0, 0),
        });

    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly MenuPropsConfiguration menuPropsConfiguration;
    private readonly IconCache iconCache;
    private readonly Dropdown<MPN> propCategoryDropdown;
    private readonly Toggle modFilterToggle;
    private readonly Toggle baseFilterToggle;
    private readonly Label initializingLabel;

    private MPN[] categories;
    private Vector2 scrollPosition;
    private IEnumerable<MenuFilePropModel> currentPropList;
    private bool menuDatabaseBusy = false;

    public MenuPropsPane(
        PropService propService,
        MenuPropRepository menuPropRepository,
        MenuPropsConfiguration menuPropsConfiguration,
        IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.menuPropsConfiguration = menuPropsConfiguration;
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        propCategoryDropdown = new(formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        modFilterToggle = new(Translation.Get("background2Window", "modsToggle"));
        modFilterToggle.ControlEvent += OnModFilterChanged;

        baseFilterToggle = new(Translation.Get("background2Window", "baseToggle"));
        baseFilterToggle.ControlEvent += OnBaseFilterChanged;

        initializingLabel = new(Translation.Get("systemMessage", "initializing"));

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        static string CategoryFormatter(MPN category, int index) =>
            Translation.Get("clothing", category.ToString());

        void OnMenuDatabaseReady(object sender, EventArgs e)
        {
            menuDatabaseBusy = false;

            Initialize();

            menuPropRepository.InitializedProps -= OnMenuDatabaseReady;
        }

        void Initialize()
        {
            categories =
            [
                MPN.null_mpn, .. menuPropRepository.CategoryMpn
                    .Where(mpn => mpn is not (MPN.handitem or MPN.kousoku_lower or MPN.kousoku_upper))
                    .OrderBy(mpn => mpn),
            ];

            propCategoryDropdown.SetItems(categories);
        }
    }

    private enum FilterType
    {
        None,
        Mod,
        Base,
    }

    public override void Draw()
    {
        if (menuDatabaseBusy)
        {
            initializingLabel.Draw();

            return;
        }

        DrawDropdown(propCategoryDropdown);

        MpsGui.BlackLine();

        if (!menuPropsConfiguration.ModMenuPropsOnly)
        {
            DrawFilterToggles();

            MpsGui.BlackLine();
        }

        DrawPropList();

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

        void DrawPropList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            var propList = currentPropList;

            if (!menuPropsConfiguration.ModMenuPropsOnly)
            {
                if (modFilterToggle.Value)
                    propList = currentPropList.Where(prop => !prop.GameMenu);
                else if (baseFilterToggle.Value)
                    propList = currentPropList.Where(prop => prop.GameMenu);
            }

            var buttonSize = Utility.GetPix(55);
            var buttonLayoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(buttonSize), GUILayout.Height(buttonSize),
            };

            foreach (var propChunk in propList.Chunk(4))
            {
                GUILayout.BeginHorizontal();

                foreach (var prop in propChunk)
                {
                    var image = iconCache.GetMenuIcon(prop);
                    var clicked = image
                        ? GUILayout.Button(image, propButtonStyle, buttonLayoutOptions)
                        : GUILayout.Button(prop.Name, propButtonStyle, buttonLayoutOptions);

                    if (clicked)
                        propService.Add(prop);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        void DrawFilterToggles()
        {
            GUILayout.BeginHorizontal();

            modFilterToggle.Draw();
            baseFilterToggle.Draw();

            GUILayout.EndHorizontal();
        }
    }

    protected override void ReloadTranslation()
    {
        base.ReloadTranslation();

        if (menuPropRepository.Busy)
            return;

        propCategoryDropdown.Reformat();

        modFilterToggle.Label = Translation.Get("background2Window", "modsToggle");
        baseFilterToggle.Label = Translation.Get("background2Window", "baseToggle");

        initializingLabel.Text = Translation.Get("systemMessage", "initializing");
    }

    private void UpdateCurrentPropList()
    {
        if (menuDatabaseBusy)
            return;

        var currentCategory = categories[propCategoryDropdown.SelectedItemIndex];

        if (currentCategory is MPN.null_mpn)
        {
            currentPropList = Enumerable.Empty<MenuFilePropModel>();

            return;
        }

        scrollPosition = Vector2.zero;

        currentPropList = menuPropRepository[currentCategory];
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        UpdateCurrentPropList();

    private void ChangeFilter(FilterType filterType)
    {
        if (!modFilterToggle.Value || !baseFilterToggle.Value)
            return;

        modFilterToggle.SetEnabledWithoutNotify(filterType is FilterType.Mod);
        baseFilterToggle.SetEnabledWithoutNotify(filterType is FilterType.Base);
    }

    private void OnModFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(FilterType.Mod);

    private void OnBaseFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(FilterType.Base);
}
