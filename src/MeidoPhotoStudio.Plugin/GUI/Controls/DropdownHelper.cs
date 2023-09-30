using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public static class DropdownHelper
{
    public static Rect DropdownWindow;

    private static int dropdownID = 100;
    private static GUIStyle defaultDropdownStyle;
    private static bool onScrollBar;
    private static Rect dropdownScrollRect;
    private static Rect dropdownRect;
    private static GUIStyle dropdownStyle;
    private static GUIStyle windowStyle;
    private static Rect buttonRect;
    private static string[] dropdownList;
    private static Vector2 scrollPos;
    private static int currentDropdownID;
    private static int selectedItemIndex;
    private static bool initialized;

    public static event EventHandler<DropdownSelectArgs> SelectionChange;

    public static event EventHandler<DropdownCloseArgs> DropdownClose;

    public static int DropdownID =>
        dropdownID++;

    public static GUIStyle DefaultDropdownStyle
    {
        get
        {
            if (!initialized)
                InitializeStyle();

            return defaultDropdownStyle;
        }
    }

    public static bool Visible { get; set; }

    public static bool DropdownOpen { get; private set; }

    public static Vector2 CalculateElementSize(string item, GUIStyle style = null)
    {
        if (!initialized)
            InitializeStyle();

        style ??= DefaultDropdownStyle;

        return style.CalcSize(new(item));
    }

    public static Vector2 CalculateElementSize(string[] list, GUIStyle style = null)
    {
        if (!initialized)
            InitializeStyle();

        style ??= DefaultDropdownStyle;

        var content = new GUIContent(list[0]);
        var calculatedSize = style.CalcSize(content);

        for (var i = 1; i < list.Length; i++)
        {
            content.text = list[i];

            var calcSize = style.CalcSize(content);

            if (calcSize.x > calculatedSize.x)
                calculatedSize = calcSize;
        }

        return calculatedSize;
    }

    public static void Set(Dropdown dropdown, GUIStyle style = null)
    {
        dropdownStyle = style ?? DefaultDropdownStyle;
        currentDropdownID = dropdown.DropdownID;
        dropdownList = dropdown.DropdownList;
        scrollPos = dropdown.ScrollPos;
        selectedItemIndex = dropdown.SelectedItemIndex;
        scrollPos = dropdown.ScrollPos;
        buttonRect = dropdown.ButtonRect;

        var calculatedSize = dropdown.ElementSize;
        var calculatedListHeight = calculatedSize.y * dropdownList.Length;
        var heightAbove = buttonRect.y;
        var heightBelow = Screen.height - heightAbove - buttonRect.height;
        var rectWidth = Mathf.Max(calculatedSize.x + 5, buttonRect.width);
        var rectHeight = Mathf.Min(calculatedListHeight, Mathf.Max(heightAbove, heightBelow));

        if (calculatedListHeight > heightBelow && heightAbove > heightBelow)
        {
            DropdownWindow = new(buttonRect.x, buttonRect.y - rectHeight, rectWidth + 18, rectHeight);
        }
        else
        {
            if (calculatedListHeight > heightBelow)
                rectHeight -= calculatedSize.y;

            DropdownWindow = new(buttonRect.x, buttonRect.y + buttonRect.height, rectWidth + 18, rectHeight);
        }

        DropdownWindow.x = Mathf.Clamp(DropdownWindow.x, 0, Screen.width - rectWidth - 18);

        dropdownScrollRect = new(0, 0, DropdownWindow.width, DropdownWindow.height);
        dropdownRect = new(0, 0, DropdownWindow.width - 18, calculatedListHeight);

        DropdownOpen = true;
        Visible = true;
    }

    public static void HandleDropdown()
    {
        DropdownWindow = GUI.Window(Constants.DropdownWindowID, DropdownWindow, GUIFunc, string.Empty, windowStyle);

        if (UnityEngine.Input.mouseScrollDelta.y is not 0f && Visible && DropdownWindow.Contains(Event.current.mousePosition))
            UnityEngine.Input.ResetInputAxes();
    }

    private static void GUIFunc(int id)
    {
        var clicked = false;

        if (Event.current.type is EventType.MouseUp)
            clicked = true;

        scrollPos = GUI.BeginScrollView(dropdownScrollRect, scrollPos, dropdownRect);

        var selection = GUI.SelectionGrid(dropdownRect, selectedItemIndex, dropdownList, 1, dropdownStyle);

        GUI.EndScrollView();

        var clickedYou = false;

        if (Utility.AnyMouseDown())
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
    }

    private static void InitializeStyle()
    {
        defaultDropdownStyle = new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
            margin = new(0, 0, 0, 0),
        };

        defaultDropdownStyle.padding.top = defaultDropdownStyle.padding.bottom = 2;
        defaultDropdownStyle.normal.background = Utility.MakeTex(2, 2, new(0f, 0f, 0f, 0.5f));

        var whiteBackground = new Texture2D(2, 2);

        defaultDropdownStyle.onHover.background
            = defaultDropdownStyle.hover.background
            = defaultDropdownStyle.onNormal.background
            = whiteBackground;

        defaultDropdownStyle.onHover.textColor
            = defaultDropdownStyle.onNormal.textColor
            = defaultDropdownStyle.hover.textColor
            = Color.black;

        windowStyle = new(GUI.skin.box)
        {
            padding = new(0, 0, 0, 0),
            alignment = TextAnchor.UpperRight,
        };

        initialized = true;
    }

    public class DropdownEventArgs : EventArgs
    {
        public DropdownEventArgs(int dropdownID) =>
            DropdownID = dropdownID;

        public int DropdownID { get; }
    }

    public class DropdownSelectArgs : DropdownEventArgs
    {
        public DropdownSelectArgs(int dropdownID, int selection)
            : base(dropdownID) =>
            SelectedItemIndex = selection;

        public int SelectedItemIndex { get; }
    }

    public class DropdownCloseArgs : DropdownEventArgs
    {
        public DropdownCloseArgs(int dropdownID, Vector2 scrollPos, bool clickedYou = false)
            : base(dropdownID)
        {
            ScrollPos = scrollPos;
            ClickedYou = clickedYou;
        }

        public Vector2 ScrollPos { get; }

        public bool ClickedYou { get; }
    }
}
