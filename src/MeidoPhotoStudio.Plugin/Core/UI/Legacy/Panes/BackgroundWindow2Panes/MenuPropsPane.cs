using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class MenuPropsPane : BasePane, IVirtualListHandler
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
    private readonly VirtualList virtualList;

    private Vector2 buttonSize;
    private MPN[] categories;
    private Vector2 scrollPosition;
    private IList<MenuFilePropModel> currentPropList = [];
    private bool menuDatabaseBusy = false;
    private FilterType currentFilter;

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

        virtualList = new()
        {
            Handler = this,
            Grid = true,
        };

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        static LabelledDropdownItem CategoryFormatter(MPN category, int index) =>
            new(Translation.Get("clothing", category.ToString()));

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

    int IVirtualListHandler.Count =>
        currentPropList.Count;

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

        if (propCategoryDropdown.SelectedItem is not MPN.null_mpn)
            DrawPropList();

        void DrawPropList()
        {
            buttonSize = Vector2.one * ((parent.WindowRect.width - 20f) / 4);

            var scrollRect = GUILayoutUtility.GetRect(0f, parent.WindowRect.width, 100f, parent.WindowRect.height);

            scrollPosition = virtualList.BeginScrollView(scrollRect, scrollPosition);

            foreach (var (i, offset) in virtualList)
            {
                var prop = currentPropList[i];
                var image = iconCache.GetMenuIcon(prop);

                var buttonRect = new Rect(
                    scrollRect.x + offset.x,
                    scrollRect.y + offset.y,
                    buttonSize.x,
                    buttonSize.y);

                var clicked = image
                    ? GUI.Button(buttonRect, image, propButtonStyle)
                    : GUI.Button(buttonRect, prop.Name, propButtonStyle);

                if (clicked)
                    propService.Add(prop);
            }

            GUI.EndScrollView();
        }

        void DrawFilterToggles()
        {
            GUILayout.BeginHorizontal();

            modFilterToggle.Draw();
            baseFilterToggle.Draw();

            GUILayout.EndHorizontal();
        }
    }

    Vector2 IVirtualListHandler.ItemDimensions(int index) =>
        buttonSize;

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

    private void UpdateCurrentPropList(bool resetScrollPosition = true)
    {
        if (menuDatabaseBusy)
            return;

        var currentCategory = categories[propCategoryDropdown.SelectedItemIndex];

        if (currentCategory is MPN.null_mpn)
        {
            currentPropList = [];

            return;
        }

        if (resetScrollPosition)
            scrollPosition = Vector2.zero;

        IEnumerable<MenuFilePropModel> propList = menuPropRepository[currentCategory];

        if (!menuPropsConfiguration.ModMenuPropsOnly)
        {
            if (modFilterToggle.Value)
                propList = propList.Where(prop => !prop.GameMenu);
            else if (baseFilterToggle.Value)
                propList = propList.Where(prop => prop.GameMenu);
        }

        currentPropList = propList.ToArray();
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        UpdateCurrentPropList();

    private void ChangeFilter(FilterType filterType)
    {
        if (filterType == currentFilter)
            return;

        currentFilter = filterType;

        modFilterToggle.SetEnabledWithoutNotify(currentFilter is FilterType.Mod);
        baseFilterToggle.SetEnabledWithoutNotify(currentFilter is FilterType.Base);

        UpdateCurrentPropList(false);
    }

    private void OnModFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(modFilterToggle.Value
            ? FilterType.Mod
            : FilterType.None);

    private void OnBaseFilterChanged(object sender, EventArgs e) =>
        ChangeFilter(baseFilterToggle.Value
            ? FilterType.Base
            : FilterType.None);
}
