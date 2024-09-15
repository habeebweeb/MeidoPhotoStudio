using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class MyRoomPropsPane : BasePane
{
    private readonly PropService propService;
    private readonly MyRoomPropRepository myRoomPropRepository;
    private readonly IconCache iconCache;
    private readonly LazyStyle buttonStyle = new(
        13,
        () => new(GUI.skin.button)
        {
            padding = new(0, 0, 0, 0),
        });

    private readonly Dropdown<int> propCategoryDropdown;

    private Vector2 scrollPosition;
    private IList<MyRoomPropModel> currentPropList;

    public MyRoomPropsPane(
        PropService propService, MyRoomPropRepository myRoomPropRepository, IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.myRoomPropRepository = myRoomPropRepository ?? throw new ArgumentNullException(nameof(myRoomPropRepository));
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        int[] categories = [-1, .. myRoomPropRepository.CategoryIDs.OrderBy(id => id)];

        propCategoryDropdown = new(categories, formatter: CategoryFormatter);
        propCategoryDropdown.SelectionChanged += OnPropCategoryDropdownChanged;

        UpdateCurrentPropList();

        static string CategoryFormatter(int category, int index) =>
            Translation.Get("myRoomPropCategories", category.ToString());
    }

    public override void Draw()
    {
        DrawDropdown(propCategoryDropdown);

        if (propCategoryDropdown.SelectedItem is not -1)
        {
            MpsGui.BlackLine();

            DrawPropList();
        }

        void DrawPropList()
        {
            var gridSize = 3;
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

                    var image = iconCache.GetMyRoomIcon(prop);

                    var buttonRect = new Rect(
                        scrollRect.x + boxDimensions.x * j,
                        scrollRect.y + boxDimensions.y * (i / gridSize),
                        boxDimensions.x,
                        boxDimensions.y);

                    var clicked = image
                        ? GUI.Button(buttonRect, image, buttonStyle)
                        : GUI.Button(buttonRect, prop.Name, buttonStyle);

                    if (clicked)
                        propService.Add(prop);
                }
            }

            GUI.EndScrollView();
        }
    }

    protected override void ReloadTranslation() =>
        propCategoryDropdown.Reformat();

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
