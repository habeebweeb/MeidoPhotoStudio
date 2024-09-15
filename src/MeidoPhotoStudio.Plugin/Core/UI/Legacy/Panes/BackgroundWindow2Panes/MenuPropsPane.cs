using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
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

        if (propCategoryDropdown.SelectedItem is not MPN.null_mpn)
            DrawPropList();

        void DrawPropList()
        {
            var gridSize = 4;
            var buttonSize = (parent.WindowRect.width - 20f) / gridSize;
            var boxDimensions = new Vector2(buttonSize, buttonSize);
            var scrollRect = GUILayoutUtility.GetRect(0f, parent.WindowRect.width, 100f, parent.WindowRect.height);
            var scrollView = new Rect(scrollRect.x, scrollRect.y, scrollRect.width - 20, boxDimensions.y * Mathf.CeilToInt((float)currentPropList.Count / gridSize));

            scrollPosition = GUI.BeginScrollView(scrollRect, scrollPosition, scrollView);

            var firstVisibleIndex = Mathf.FloorToInt(scrollPosition.y / boxDimensions.y) * gridSize;
            var lastVisibleIndex = Mathf.CeilToInt((scrollPosition.y + scrollRect.height) / boxDimensions.y) * gridSize + gridSize;

            if (firstVisibleIndex < 0)
                firstVisibleIndex = 0;

            if (lastVisibleIndex > currentPropList.Count)
                lastVisibleIndex = currentPropList.Count;

            for (var i = firstVisibleIndex; i < lastVisibleIndex; i += gridSize)
            {
                for (var j = 0; j < gridSize; j++)
                {
                    var itemIndex = i + j;

                    if (itemIndex >= currentPropList.Count)
                        break;

                    var prop = currentPropList[itemIndex];

                    var image = iconCache.GetMenuIcon(prop);

                    var buttonRect = new Rect(
                        scrollRect.x + boxDimensions.x * j,
                        scrollRect.y + boxDimensions.y * (i / gridSize),
                        boxDimensions.x,
                        boxDimensions.y);

                    var clicked = image
                        ? GUI.Button(buttonRect, image, propButtonStyle)
                        : GUI.Button(buttonRect, prop.Name, propButtonStyle);

                    if (clicked)
                        propService.Add(prop);
                }
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
