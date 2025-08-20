using MeidoPhotoStudio.Plugin.Core.Configuration;
using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class MenuPropsPane : BasePane, IVirtualListHandler
{
    private static readonly MPN[] IgnoredMpn = [SafeMpn.handitem, SafeMpn.kousoku_lower, SafeMpn.kousoku_upper];

    private readonly LazyStyle propButtonStyle = new(
        11,
        static () => new(GUI.skin.button)
        {
            alignment = TextAnchor.UpperLeft,
            margin = new(0, 0, 0, 0),
            padding = new(0, 0, 0, 0),
        });

    private readonly PropService propService;
    private readonly MenuPropRepository menuPropRepository;
    private readonly PropsConfiguration propsConfiguration;
    private readonly IconCache iconCache;
    private readonly IRevealModInFileManagerHandler revealModHandler;
    private readonly Dropdown<MPN> propCategoryDropdown;
    private readonly Toggle modFilterToggle;
    private readonly Toggle baseFilterToggle;
    private readonly Label initializingLabel;
    private readonly VirtualList virtualList;
    private readonly SearchBar<MenuFilePropModel> searchBar;

    private Vector2 buttonSize;
    private MPN[] categories;
    private Vector2 scrollPosition;
    private List<MenuFilePropModel> currentPropList = [];
    private bool menuDatabaseBusy = false;
    private FilterType currentFilter;

    public MenuPropsPane(
        Translation translation,
        PropService propService,
        MenuPropRepository menuPropRepository,
        PropsConfiguration propsConfiguration,
        IconCache iconCache,
        IRevealModInFileManagerHandler revealModHandler)
    {
        _ = translation ?? throw new ArgumentNullException(nameof(translation));
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.menuPropRepository = menuPropRepository ?? throw new ArgumentNullException(nameof(menuPropRepository));
        this.propsConfiguration = propsConfiguration;
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));
        this.revealModHandler = revealModHandler ?? throw new ArgumentNullException(nameof(revealModHandler));

        this.menuPropRepository.ChangedProps += OnMenuPropRepositoryChanged;
        translation.Initialized += OnTranslationInitialized;

        propCategoryDropdown = new(formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        modFilterToggle = new(new LocalizableGUIContent(translation, "menuFilePropsPane", "modsToggle"));
        modFilterToggle.ControlEvent += OnModFilterChanged;

        baseFilterToggle = new(new LocalizableGUIContent(translation, "menuFilePropsPane", "baseToggle"));
        baseFilterToggle.ControlEvent += OnBaseFilterChanged;

        initializingLabel = new(new LocalizableGUIContent(translation, "systemMessage", "initializing"));

        virtualList = new()
        {
            Handler = this,
            Grid = true,
        };

        searchBar = new(SearchSelector, PropFormatter)
        {
            PlaceholderContent = new LocalizableGUIContent(translation, "menuFilePropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        if (menuPropRepository.Busy)
        {
            menuDatabaseBusy = true;

            menuPropRepository.InitializedProps += OnMenuDatabaseReady;
        }
        else
        {
            Initialize();
        }

        LabelledDropdownItem CategoryFormatter(MPN category, int index) =>
            new(translation["clothing", category.ToString()]);

        IEnumerable<MenuFilePropModel> SearchSelector(string query) =>
            menuDatabaseBusy
                ? []
                : menuPropRepository
                    .Where(model => !IgnoredMpn.Any(mpn => model.CategoryMpn == mpn))
                    .Where(model => modFilterToggle.Value ? !model.GameMenu : !baseFilterToggle.Value || model.GameMenu)
                    .Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        Path.GetFileNameWithoutExtension(model.Filename).Replace("_i_", string.Empty).Contains(query, StringComparison.OrdinalIgnoreCase));

        IconDropdownItem PropFormatter(MenuFilePropModel model, int index) =>
            new($"{model.Name}\n{model.Filename}", () => iconCache.GetMenuIcon(model), 75);

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
                    .Where(value => !IgnoredMpn.Any(mpn => mpn == value))
                    .OrderBy(mpn => mpn),
            ];

            propCategoryDropdown.SetItems(categories);
        }

        void OnTranslationInitialized(object sender, EventArgs e)
        {
            if (menuPropRepository.Busy)
                return;

            propCategoryDropdown.Reformat();
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

        UIUtility.DrawBlackLine();

        DrawTextFieldWithScrollBarOffset(searchBar);

        if (!propsConfiguration.IgnoreGameMenuFiles.Value)
            DrawFilterToggles();

        UIUtility.DrawBlackLine();

        if (propCategoryDropdown.SelectedItem is not MPN.null_mpn)
            DrawPropList();

        void DrawPropList()
        {
            var scrollRect = GUILayoutUtility.GetRect(0f, Parent.WindowRect.width, 100f, Parent.WindowRect.height);

            buttonSize = Vector2.one * Mathf.Min(80f, (scrollRect.width - 18f) / 4);

            scrollPosition = virtualList.BeginScrollView(scrollRect, scrollPosition);

            var xOffset = Mathf.Max(0f, (scrollRect.width - buttonSize.x * virtualList.ColumnCount) / 2f - 9f);

            foreach (var (i, offset) in virtualList)
            {
                var prop = currentPropList[i];
                var image = iconCache.GetMenuIcon(prop);

                var buttonRect = new Rect(
                    scrollRect.x + offset.x + xOffset,
                    scrollRect.y + offset.y,
                    buttonSize.x,
                    buttonSize.y);

                var clicked = image
                    ? GUI.Button(buttonRect, image, propButtonStyle)
                    : GUI.Button(buttonRect, prop.Name, propButtonStyle);

                if (clicked)
                {
                    if (Event.current.button is 1)
                        revealModHandler.Reveal(prop);
                    else
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

    public override void Deactivate()
    {
        if (menuPropRepository.Busy)
            return;

        propCategoryDropdown.SelectedItemIndex = 0;
    }

    Vector2 IVirtualListHandler.ItemDimensions(int index) =>
        buttonSize;

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<MenuFilePropModel> e) =>
        propService.Add(e.Item);

    private void OnMenuPropRepositoryChanged(object sender, MenuPropRepositoryChangedEventArgs e)
    {
        var changed = false;

        foreach (var prop in e.AddedMenuFiles)
        {
            if (prop.CategoryMpn != propCategoryDropdown.SelectedItem)
                continue;

            changed = true;
            currentPropList.Add(prop);
        }

        foreach (var prop in e.DeletedMenuFiles)
        {
            if (prop.CategoryMpn != propCategoryDropdown.SelectedItem)
                continue;

            changed = true;
            currentPropList.Remove(prop);
        }

        if (!changed)
            return;

        currentPropList.Sort(static (a, b) => a.Filename.CompareTo(b.Filename));
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

        if (!propsConfiguration.IgnoreGameMenuFiles.Value)
        {
            if (modFilterToggle.Value)
                propList = propList.Where(static prop => !prop.GameMenu);
            else if (baseFilterToggle.Value)
                propList = propList.Where(static prop => prop.GameMenu);
        }

        currentPropList =
            [.. propList.OrderByDescending(static prop => prop.GameMenu).ThenBy(static prop => prop.Filename)];
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
