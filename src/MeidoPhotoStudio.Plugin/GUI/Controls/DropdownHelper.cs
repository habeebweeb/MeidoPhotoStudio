using MeidoPhotoStudio.Plugin.Framework.UI;

namespace MeidoPhotoStudio.Plugin;

internal static class DropdownHelper
{
    public static Rect DropdownWindow;

    private static int dropdownID = 100;
    private static bool onScrollBar;
    private static Rect dropdownScrollRect;
    private static Rect dropdownRect;
    private static GUIStyle dropdownItemStyle;
    private static GUIStyle windowStyle;
    private static Rect buttonRect;
    private static string[] items;
    private static Vector2 scrollPos;
    private static int currentDropdownID;
    private static int selectedItemIndex;

    public static event EventHandler<DropdownSelectArgs> SelectionChange;

    public static event EventHandler<DropdownCloseArgs> DropdownClose;

    public static int DropdownID =>
        dropdownID++;

    public static LazyStyle ButtonStyle { get; } = new(
        13,
        () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        });

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

    public static bool Visible { get; set; }

    public static bool DropdownOpen { get; private set; }

    private static GUIStyle WindowStyle =>
        windowStyle ??= new(GUI.skin.box)
        {
            padding = new(0, 0, 0, 0),
            alignment = TextAnchor.UpperRight,
        };

    public static Vector2 CalculateElementSize(string item, GUIStyle style = null)
    {
        style ??= DefaultDropdownStyle;

        return style.CalcSize(new(item));
    }

    public static Vector2 CalculateElementSize(string[] list, GUIStyle style = null)
    {
        if (list.Length is 0)
            return Vector2.zero;

        style ??= DefaultDropdownStyle;

        var content = new GUIContent(list[0]);

        return list.Skip(1).Aggregate(style.CalcSize(content), (accumulate, item) =>
        {
            content.text = item;

            var newSize = style.CalcSize(content);

            return newSize.x > accumulate.x ? newSize : accumulate;
        });
    }

    public static void HandleDropdown()
    {
        DropdownWindow = GUI.Window(Constants.DropdownWindowID, DropdownWindow, GUIFunc, string.Empty, WindowStyle);

        if (UnityEngine.Input.mouseScrollDelta.y is not 0f && Visible && DropdownWindow.Contains(Event.current.mousePosition))
            UnityEngine.Input.ResetInputAxes();
    }

    public static void OpenDropdown(
        int id,
        Vector2 scrollPosition,
        string[] items,
        int selectedItemIndex,
        Rect dropdownButtonRect,
        Vector2? itemSize = null,
        GUIStyle style = null)
    {
        currentDropdownID = id;
        scrollPos = scrollPosition;
        DropdownHelper.items = items;
        DropdownHelper.selectedItemIndex = selectedItemIndex;
        buttonRect = dropdownButtonRect;
        dropdownItemStyle = style ?? DefaultDropdownStyle;

        var calculatedSize = itemSize ?? CalculateElementSize(DropdownHelper.items, dropdownItemStyle);
        var calculatedListHeight = calculatedSize.y * DropdownHelper.items.Length;
        var heightAbove = buttonRect.y;
        var heightBelow = Screen.height - heightAbove - buttonRect.height;
        var rectWidth = Mathf.Max(calculatedSize.x + 5, buttonRect.width);
        var rectHeight = Mathf.Min(calculatedListHeight, Mathf.Max(heightAbove, heightBelow));

        if (calculatedListHeight > heightBelow && heightAbove > heightBelow)
        {
            DropdownWindow = new(
                buttonRect.x,
                buttonRect.y - rectHeight,
                rectWidth + 18,
                rectHeight);
        }
        else
        {
            if (calculatedListHeight > heightBelow)
                rectHeight -= calculatedSize.y;

            DropdownWindow = new(
                buttonRect.x,
                buttonRect.y + buttonRect.height,
                rectWidth + 18,
                rectHeight);
        }

        DropdownWindow.x = Mathf.Clamp(DropdownWindow.x, 0, Screen.width - rectWidth - 18);

        dropdownScrollRect = new(0, 0, DropdownWindow.width, DropdownWindow.height);
        dropdownRect = new(0, 0, rectWidth, calculatedListHeight);

        DropdownOpen = true;
        Visible = true;
    }

    private static void GUIFunc(int id)
    {
        var clicked = false;

        if (Event.current.type is EventType.MouseUp)
            clicked = true;

        scrollPos = GUI.BeginScrollView(dropdownScrollRect, scrollPos, dropdownRect);

        var selection = GUI.SelectionGrid(dropdownRect, selectedItemIndex, items, 1, dropdownItemStyle);

        GUI.EndScrollView();

        var clickedYou = false;

        if (AnyMouseDown())
        {
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            var clickedMe = DropdownWindow.Contains(mousePos);

            onScrollBar = mousePos.x > DropdownWindow.x + DropdownWindow.width - 12f;

            if (buttonRect.Contains(mousePos))
                clickedYou = true;

            if (!clickedMe)
                DropdownOpen = false;
        }

        if (selection != selectedItemIndex || clicked && !onScrollBar)
        {
            SelectionChange?.Invoke(null, new(currentDropdownID, selection));
            DropdownOpen = false;
        }

        if (!DropdownOpen)
        {
            Visible = false;
            DropdownClose?.Invoke(null, new(currentDropdownID, scrollPos, clickedYou));
        }

        static bool AnyMouseDown() =>
            UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonDown(1) || UnityEngine.Input.GetMouseButtonDown(2);
    }

    public class DropdownEventArgs(int dropdownID) : EventArgs
    {
        public int DropdownID { get; } = dropdownID;
    }

    public class DropdownSelectArgs(int dropdownID, int selection) : DropdownEventArgs(dropdownID)
    {
        public int SelectedItemIndex { get; } = selection;
    }

    public class DropdownCloseArgs(int dropdownID, Vector2 scrollPos, bool clickedYou = false)
        : DropdownEventArgs(dropdownID)
    {
        public Vector2 ScrollPos { get; } = scrollPos;

        public bool ClickedYou { get; } = clickedYou;
    }
}
