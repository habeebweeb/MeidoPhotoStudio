using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

internal static class DropdownHelper
{
    private static readonly LazyStyle WindowStyle =
        new(0, () => new(GUI.skin.box)
        {
            padding = new(0, 0, 0, 0),
            alignment = TextAnchor.UpperRight,
        });

    private static readonly VirtualList VirtualList = new();

    private static IDropdownHandler dropdownHandler;
    private static Rect buttonRect;
    private static Rect dropdownWindow;
    private static Rect dropdownScrollRect;

    public static LazyStyle DefaultDropdownStyle { get; } = new(
        13,
        () =>
        {
            var whiteBackground = new Texture2D(2, 2);

            return new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new(0, 0, 0, 0),
                padding =
                {
                    top = 2,
                    bottom = 2,
                },
                normal = { background = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.5f)) },
                hover =
                {
                    background = whiteBackground,
                    textColor = Color.black,
                },
                onHover =
                {
                    background = whiteBackground,
                    textColor = Color.black,
                },
                onNormal =
                {
                    background = whiteBackground,
                    textColor = Color.black,
                },
            };
        });

    public static bool Visible { get; private set; }

    public static Vector2 CalculateItemDimensions(GUIContent content)
    {
        _ = content ?? throw new ArgumentNullException(nameof(content));

        return ((GUIStyle)DefaultDropdownStyle).CalcSize(content);
    }

    public static void OpenDropdown(IDropdownHandler dropdownHandler, Rect buttonRect)
    {
        DropdownHelper.dropdownHandler = dropdownHandler ?? throw new ArgumentNullException(nameof(dropdownHandler));

        var buttonPosition = GUIUtility.GUIToScreenPoint(new(buttonRect.x, buttonRect.y));

        DropdownHelper.buttonRect = buttonRect with { x = buttonPosition.x, y = buttonPosition.y };

        VirtualList.Handler = dropdownHandler;

        var (scrollViewWidth, scrollViewHeight) = (0f, 0f);

        for (var i = 0; i < dropdownHandler.Count; i++)
        {
            var item = dropdownHandler.ItemDimensions(i);

            if (item.x > scrollViewWidth)
                scrollViewWidth = item.x;

            scrollViewHeight += item.y;
        }

        var heightAbove = DropdownHelper.buttonRect.y - 15f;
        var heightBelow = Screen.height - DropdownHelper.buttonRect.yMax - 15f;

        var windowWidth = Mathf.Max(scrollViewWidth, DropdownHelper.buttonRect.width);
        var windowHeight = Mathf.Min(scrollViewHeight, Mathf.Max(heightAbove, heightBelow));
        var windowX = Mathf.Clamp(DropdownHelper.buttonRect.x, 0f, Screen.width - windowWidth);
        var windowY = scrollViewHeight > heightBelow && heightAbove > heightBelow
            ? DropdownHelper.buttonRect.y - windowHeight
            : DropdownHelper.buttonRect.yMax;

        dropdownWindow = new(windowX, windowY, windowWidth, windowHeight);
        dropdownScrollRect = dropdownWindow with { x = 0f, y = 0f };

        Visible = true;

        GUI.BringWindowToFront(765);
    }

    public static void CloseDropdown() =>
        CloseDropdown(false);

    internal static void DrawDropdown()
    {
        if (!Visible)
            return;

        dropdownWindow = GUI.Window(765, dropdownWindow, DropdownWindow, string.Empty, WindowStyle);

        if (Visible && UInput.mouseScrollDelta.y is not 0f && dropdownWindow.Contains(Event.current.mousePosition))
            UInput.ResetInputAxes();
    }

    private static void DropdownWindow(int windowId)
    {
        dropdownHandler.ScrollPosition = VirtualList
            .BeginScrollView(dropdownScrollRect, dropdownHandler.ScrollPosition);

        foreach (var (index, offset) in VirtualList)
        {
            var value = GUI.Toggle(
                new(
                    dropdownScrollRect.x,
                    dropdownScrollRect.y + offset.y,
                    dropdownScrollRect.width,
                    dropdownHandler.ItemDimensions(index).y),
                dropdownHandler.SelectedItemIndex == index,
                dropdownHandler.FormattedItem(index),
                DefaultDropdownStyle);

            if (value != (dropdownHandler.SelectedItemIndex == index))
            {
                dropdownHandler.OnItemSelected(index);
                CloseDropdown();
            }
        }

        GUI.EndScrollView();

        if (AnyMouseDown() && Event.current.type is EventType.Repaint)
        {
            var mousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            if (!dropdownWindow.Contains(mousePosition))
                CloseDropdown(buttonRect.Contains(mousePosition));
        }

        static bool AnyMouseDown() =>
            UInput.GetMouseButtonDown(0) || UInput.GetMouseButtonDown(1) || UInput.GetMouseButtonDown(2);
    }

    private static void CloseDropdown(bool clickedButton = false)
    {
        dropdownHandler?.OnDropdownClosed(clickedButton);
        Visible = false;
    }
}
