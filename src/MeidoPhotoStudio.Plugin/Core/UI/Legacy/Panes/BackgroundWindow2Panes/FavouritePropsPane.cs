using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class FavouritePropsPane : BasePane, IVirtualListHandler
{
    private static readonly MPN[] IgnoredMpn = [.. SafeMpn.GetValues(nameof(MPN.handitem), nameof(MPN.kousoku_lower), nameof(MPN.kousoku_upper))];

    private readonly PropService propService;
    private readonly FavouritePropRepository favouritePropRepository;
    private readonly IconCache iconCache;
    private readonly VirtualList virtualList;
    private readonly RenameModal renameModal;
    private readonly Dictionary<int, Vector2> itemSizes = [];
    private readonly Dictionary<int, GUIContent> itemContent = [];
    private readonly List<FavouritePropModel> favouriteProps = [];
    private readonly Label noFavouritePropsLabel;
    private readonly Dropdown<SortType> sortTypeDropdown;
    private readonly Toggle descendingToggle;
    private readonly TextField searchBar;
    private readonly Toggle editModeToggle;
    private readonly Button refreshButton;
    private readonly LazyStyle removeFavouriteButtonStyle = new(13, () => new(GUI.skin.button));
    private readonly LazyStyle favouritePropButtonStyle = new(
        13,
        () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            wordWrap = true,
        });

    private readonly LazyStyle noFavouritePropsLabelStyle = new(
        13,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

    private string searchQuery;
    private Comparison<FavouritePropModel> currentComparer = CompareName;
    private Vector2 scrollPosition;

    public FavouritePropsPane(PropService propService, FavouritePropRepository favouritePropRepository, IconCache iconCache)
    {
        this.propService = propService ?? throw new ArgumentNullException(nameof(propService));
        this.favouritePropRepository = favouritePropRepository ?? throw new ArgumentNullException(nameof(favouritePropRepository));
        this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

        this.favouritePropRepository.AddedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.RemovedFavouriteProp += OnFavouritePropAddedOrRemoved;
        this.favouritePropRepository.Refreshed += OnFavouritePropsRefreshed;

        noFavouritePropsLabel = new(Translation.Get("favouritePropsPane", "noFavouritePropsLabel"));

        sortTypeDropdown = new(
            (SortType[])Enum.GetValues(typeof(SortType)),
            formatter: (sortType, _) => new LabelledDropdownItem(Translation.Get("favouritePropsSortTypes", sortType.ToLower())));

        sortTypeDropdown.SelectionChanged += OnSortTypeChanged;

        descendingToggle = new(Translation.Get("favouritePropsPane", "descendingToggle"));
        descendingToggle.ControlEvent += OnDescendingToggleChanged;

        searchBar = new()
        {
            Placeholder = Translation.Get("favouritePropsPane", "searchBarPlaceholder"),
        };

        searchBar.ChangedValue += OnSearchChanged;

        refreshButton = new(Translation.Get("favouritePropsPane", "refreshButton"));
        refreshButton.ControlEvent += OnRefreshButtonPushed;

        editModeToggle = new(Translation.Get("favouritePropsPane", "editModeToggle"));
        editModeToggle.ControlEvent += OnEditModeToggleChanged;

        virtualList = new()
        {
            Handler = this,
            Spacing = new(0f, 2f),
        };

        renameModal = new(this, this.iconCache);

        favouriteProps.AddRange(favouritePropRepository);

        OnPropListUpdated();
    }

    private enum SortType
    {
        Name,
        DateAdded,
    }

    int IVirtualListHandler.Count =>
        Count;

    private int Count =>
        favouriteProps.Count;

    public override void Draw()
    {
        if (favouritePropRepository.Count is 0)
        {
            refreshButton.Draw();

            MpsGui.BlackLine();

            noFavouritePropsLabel.Draw(noFavouritePropsLabelStyle);

            return;
        }

        DrawTextFieldWithScrollBarOffset(searchBar);

        GUILayout.BeginHorizontal();

        sortTypeDropdown.Draw(GUILayout.Width(parent.WindowRect.width - Utility.GetPix(150)));

        GUILayout.FlexibleSpace();

        descendingToggle.Draw();

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        GUILayout.BeginHorizontal();

        editModeToggle.Draw();

        refreshButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        MpsGui.BlackLine();

        var windowRect = parent.WindowRect;
        var scrollRect = GUILayoutUtility.GetRect(0f, windowRect.width, 100f, windowRect.height);

        scrollPosition = virtualList.BeginScrollView(scrollRect, scrollPosition);

        foreach (var (i, offset) in virtualList)
        {
            var propDimensions = ItemDimensions(i);
            var windowWidth = windowRect.width - 25f;

            if (editModeToggle.Value)
            {
                var closeButtonSize = Utility.GetPix(20f);
                var propButtonRect = new Rect(scrollRect.x, scrollRect.y + offset.y, windowWidth - closeButtonSize, propDimensions.y);
                var removeFavouiteRect = new Rect(scrollRect.x + windowWidth - closeButtonSize, scrollRect.y + offset.y, closeButtonSize, propDimensions.y);

                if (GUI.Button(propButtonRect, GetContent(i), favouritePropButtonStyle))
                    renameModal.Rename(favouriteProps[i]);

                if (GUI.Button(removeFavouiteRect, "X", removeFavouriteButtonStyle))
                {
                    favouritePropRepository.Remove(favouriteProps[i].PropModel);

                    break;
                }
            }
            else
            {
                var propButtonRect = new Rect(scrollRect.x, scrollRect.y + offset.y, windowWidth, propDimensions.y);

                if (GUI.Button(propButtonRect, GetContent(i), favouritePropButtonStyle))
                    propService.Add(favouriteProps[i].PropModel);
            }
        }

        GUI.EndScrollView();
    }

    Vector2 IVirtualListHandler.ItemDimensions(int index) =>
        ItemDimensions(index);

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        virtualList.Invalidate();
        itemSizes.Clear();
    }

    protected override void ReloadTranslation()
    {
        sortTypeDropdown.Reformat();
        noFavouritePropsLabel.Text = Translation.Get("favouritePropsPane", "noFavouritePropsLabel");
        descendingToggle.Label = Translation.Get("favouritePropsPane", "descendingToggle");
        searchBar.Placeholder = Translation.Get("favouritePropsPane", "searchBarPlaceholder");
        refreshButton.Label = Translation.Get("favouritePropsPane", "refreshButton");
        editModeToggle.Label = Translation.Get("favouritePropsPane", "editModeToggle");
    }

    private static int CompareName(FavouritePropModel a, FavouritePropModel b) =>
        string.Compare(a.Name, b.Name);

    private static int CompareDate(FavouritePropModel a, FavouritePropModel b) =>
        DateTime.Compare(a.DateAdded, b.DateAdded);

    private void OnFavouritePropAddedOrRemoved(object sender, FavouritePropRepositoryEventArgs e)
    {
        if (favouritePropRepository.Count is 0)
            editModeToggle.SetEnabledWithoutNotify(false);

        Search(searchQuery);
    }

    private void OnFavouritePropsRefreshed(object sender, EventArgs e) =>
        Search(searchQuery);

    private void OnSearchChanged(object sender, EventArgs e)
    {
        if (string.Equals(searchBar.Value, searchQuery))
            return;

        Search(searchBar.Value);
    }

    private void OnSortTypeChanged(object sender, DropdownEventArgs<SortType> e)
    {
        if (e.SelectedItemIndex == e.PreviousSelectedItemIndex)
            return;

        Comparison<FavouritePropModel> newComparer = e.Item switch
        {
            SortType.Name => CompareName,
            SortType.DateAdded => CompareDate,
            _ => throw new NotSupportedException($"'{e.Item}' is not supported"),
        };

        currentComparer = newComparer;

        OnPropListUpdated();
    }

    private void OnRefreshButtonPushed(object sender, EventArgs e) =>
        favouritePropRepository.Refresh();

    private void OnEditModeToggleChanged(object sender, EventArgs e)
    {
        virtualList.Invalidate();
        itemSizes.Clear();
    }

    private void OnDescendingToggleChanged(object sender, EventArgs e) =>
        OnPropListUpdated();

    private void OnFavouritePropRenamed(FavouritePropModel model, string newName)
    {
        model.Name = newName;

        OnPropListUpdated();
    }

    private void OnRenamingCancelled()
    {
    }

    private void Search(string query)
    {
        searchQuery = query;

        favouriteProps.Clear();

        favouriteProps.AddRange(string.IsNullOrEmpty(searchQuery)
            ? favouritePropRepository
            : favouritePropRepository.Where(NameContainsSearch));

        OnPropListUpdated();

        bool NameContainsSearch(FavouritePropModel model) =>
            model.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            || model.PropModel.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
    }

    private void OnPropListUpdated()
    {
        favouriteProps.Sort(descendingToggle.Value
            ? (a, b) => currentComparer(b, a)
            : (a, b) => currentComparer(a, b));

        virtualList.Invalidate();
        itemSizes.Clear();
        itemContent.Clear();
    }

    private GUIContent GetContent(int index)
    {
        if (itemContent.TryGetValue(index, out var content))
            return content;

        var favouriteProp = favouriteProps[index];

        var icon = favouriteProp.PropModel switch
        {
            MenuFilePropModel model when !IgnoredMpn.Any(mpn => model.CategoryMpn == mpn) => iconCache.GetMenuIcon(model),
            MyRoomPropModel model => iconCache.GetMyRoomIcon(model),
            _ => null,
        };

        content = new(favouriteProp.Name, icon);

        itemContent[index] = content;

        return content;
    }

    private Vector2 ItemDimensions(int index)
    {
        if (itemSizes.TryGetValue(index, out var size))
            return size;

        var iconHeight = Utility.GetPix(75f);
        var windowWidth = parent.WindowRect.width - 25f;

        var buttonWidth = editModeToggle.Value
            ? windowWidth - Utility.GetPix(20f)
            : windowWidth;

        var itemHeight = favouriteProps[index].PropModel switch
        {
            MenuFilePropModel menuFile when !IgnoredMpn.Any(mpn => menuFile.CategoryMpn == mpn) => iconHeight,
            MyRoomPropModel => iconHeight,
            _ => favouritePropButtonStyle.Style.CalcHeight(GetContent(index), buttonWidth),
        };

        size = itemSizes[index] = new(buttonWidth, itemHeight);

        return size;
    }

    private class RenameModal : BaseWindow
    {
        private readonly FavouritePropsPane favouritePropsPane;
        private readonly IconCache iconCache;
        private readonly Header renamingHeader;
        private readonly TextField renamingTextField;
        private readonly Button renameButton;
        private readonly Button cancelButton;

        private Texture icon;
        private FavouritePropModel renamingModel;

        public RenameModal(FavouritePropsPane favouritePropsPane, IconCache iconCache)
        {
            this.favouritePropsPane = favouritePropsPane ?? throw new ArgumentNullException(nameof(favouritePropsPane));
            this.iconCache = iconCache ?? throw new ArgumentNullException(nameof(iconCache));

            renamingHeader = new(Translation.Get("renameFavouritePropModal", "renameHeader"));

            renamingTextField = new()
            {
                Placeholder = Translation.Get("renameFavouritePropModal", "renameTextFieldPlaceholder"),
            };

            renameButton = new(Translation.Get("renameFavouritePropModal", "renameButton"));
            renameButton.ControlEvent += OnRenameButtonClicked;

            cancelButton = new(Translation.Get("renameFavouritePropModal", "cancelButton"));
            cancelButton.ControlEvent += OnCancelButtonClicked;
        }

        public void Rename(FavouritePropModel favouriteProp)
        {
            renamingModel = favouriteProp ?? throw new ArgumentNullException(nameof(favouriteProp));

            icon = renamingModel.PropModel switch
            {
                MenuFilePropModel model => iconCache.GetMenuIcon(model),
                MyRoomPropModel model => iconCache.GetMyRoomIcon(model),
                _ => null,
            };

            renamingHeader.Text = string.Format(
                Translation.Get("renameFavouritePropModal", "renameHeader"), renamingModel.PropModel.Name);

            renamingTextField.Value = renamingModel.Name;

            var width = ScaledMinimum(450);
            var height = ScaledMinimum(150);

            WindowRect = new(
                Screen.width / 2f - width / 2f,
                Screen.height / 2f - height / 2f,
                width,
                height);

            Modal.Show(this);

            static int ScaledMinimum(float value) =>
                Mathf.Min(Utility.GetPix(Mathf.RoundToInt(value)), (int)value);
        }

        public override void Draw()
        {
            GUILayout.BeginArea(new(10, 10, WindowRect.width - 10 * 2, WindowRect.height - 10 * 2));

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.Height(Utility.GetPix(75)));
            {
                if (icon != null)
                    GUILayout.Box(icon, GUILayout.Width(Utility.GetPix(75)), GUILayout.Height(Utility.GetPix(75)));

                GUILayout.BeginVertical();
                {
                    GUILayout.FlexibleSpace();

                    renamingHeader.Draw();

                    GUILayout.Space(5);

                    var width = icon != null
                        ? GUILayout.MaxWidth(WindowRect.width - Utility.GetPix(75) - 30)
                        : GUILayout.MaxWidth(WindowRect.width - 20);

                    renamingTextField.Draw(width, GUILayout.Height(Utility.GetPix(22)));

                    GUILayout.FlexibleSpace();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            renameButton.Draw(GUILayout.ExpandWidth(false));
            cancelButton.Draw(GUILayout.MinWidth(Utility.GetPix(110)));

            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        protected override void ReloadTranslation()
        {
            renamingTextField.Placeholder = Translation.Get("renameFavouritePropModal", "renameTextFieldPlaceholder");
            renameButton.Label = Translation.Get("renameFavouritePropModal", "renameButton");
            cancelButton.Label = Translation.Get("renameFavouritePropModal", "cancelButton");
        }

        private void OnCancelButtonClicked(object sender, EventArgs e)
        {
            Modal.Close();
            favouritePropsPane.OnRenamingCancelled();
        }

        private void OnRenameButtonClicked(object sender, EventArgs e)
        {
            Modal.Close();
            favouritePropsPane.OnFavouritePropRenamed(renamingModel, renamingTextField.Value);
        }
    }
}
