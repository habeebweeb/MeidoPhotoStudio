using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class MyRoomPropsPane : BasePane, IVirtualListHandler
{
    private readonly PropService propService;
    private readonly MyRoomPropRepository myRoomPropRepository;
    private readonly IconCache iconCache;
    private readonly VirtualList virtualList;
    private readonly LazyStyle buttonStyle = new(
        13,
        () => new(GUI.skin.button)
        {
            padding = new(0, 0, 0, 0),
        });

    private readonly Dropdown<int> propCategoryDropdown;
    private readonly SearchBar<MyRoomPropModel> searchBar;

    private Vector2 buttonSize;
    private Vector2 scrollPosition;
    private IList<MyRoomPropModel> currentPropList = [];

    public MyRoomPropsPane(
        PropService propService, MyRoomPropRepository myRoomPropRepository, IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.myRoomPropRepository = myRoomPropRepository ?? throw new ArgumentNullException(nameof(myRoomPropRepository));
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        int[] categories = [-1, .. myRoomPropRepository.CategoryIDs.OrderBy(id => id)];

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        searchBar = new(SearchSelector, PropFormatter)
        {
            Placeholder = Translation.Get("myRoomPropsPane", "searchBarPlaceholder"),
        };

        searchBar.SelectedValue += OnSearchSelected;

        UpdateCurrentPropList();

        virtualList = new()
        {
            Handler = this,
            Grid = true,
        };

        static LabelledDropdownItem CategoryFormatter(int category, int index) =>
            new(Translation.Get("myRoomPropCategories", category.ToString()));

        IEnumerable<MyRoomPropModel> SearchSelector(string query) =>
            this.myRoomPropRepository
                .Where(model => model.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || model.AssetName.Contains(query, StringComparison.OrdinalIgnoreCase));

        IconDropdownItem PropFormatter(MyRoomPropModel model, int index) =>
            new($"{model.Name}\n{model.AssetName}", () => iconCache.GetMyRoomIcon(model), 75);
    }

    int IVirtualListHandler.Count =>
        currentPropList.Count;

    public override void Draw()
    {
        DrawTextFieldWithScrollBarOffset(searchBar);

        DrawDropdown(propCategoryDropdown);

        if (propCategoryDropdown.SelectedItem is not -1)
        {
            MpsGui.BlackLine();

            DrawPropList();
        }

        void DrawPropList()
        {
            buttonSize = Vector2.one * Mathf.Min(80f, (parent.WindowRect.width - 20f) / 3);

            var scrollRect = GUILayoutUtility.GetRect(0f, parent.WindowRect.width, 100f, parent.WindowRect.height);

            scrollPosition = virtualList.BeginScrollView(scrollRect, scrollPosition);

            var xOffset = (scrollRect.width - buttonSize.x * virtualList.ColumnCount) / 2f;

            foreach (var (i, offset) in virtualList)
            {
                var prop = currentPropList[i];

                var image = iconCache.GetMyRoomIcon(prop);

                var buttonRect = new Rect(
                    scrollRect.x + offset.x + xOffset,
                    scrollRect.y + offset.y,
                    buttonSize.x,
                    buttonSize.y);

                var clicked = image
                    ? GUI.Button(buttonRect, image, buttonStyle)
                    : GUI.Button(buttonRect, prop.Name, buttonStyle);

                if (clicked)
                    propService.Add(prop);
            }

            GUI.EndScrollView();
        }
    }

    Vector2 IVirtualListHandler.ItemDimensions(int index) =>
        buttonSize;

    protected override void ReloadTranslation()
    {
        propCategoryDropdown.Reformat();
        searchBar.Placeholder = Translation.Get("myRoomPropsPane", "searchBarPlaceholder");
        searchBar.Reformat();
    }

    private void OnSearchSelected(object sender, SearchBarSelectionEventArgs<MyRoomPropModel> e) =>
        propService.Add(e.Item);

    private void UpdateCurrentPropList()
    {
        var currentCategory = propCategoryDropdown.SelectedItem;

        if (currentCategory is -1)
        {
            currentPropList = [];

            return;
        }

        scrollPosition = Vector2.zero;

        currentPropList = myRoomPropRepository[currentCategory];
    }

    private void OnPropCategoryDropdownChanged(object sender, EventArgs e) =>
        UpdateCurrentPropList();
}
