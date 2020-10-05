using System;
using UnityEngine;
namespace COM3D2.MeidoPhotoStudio.Plugin
{
    using DropdownSelectArgs = DropdownHelper.DropdownSelectArgs;
    using DropdownCloseArgs = DropdownHelper.DropdownCloseArgs;
    public class Dropdown : BaseControl
    {
        public event EventHandler SelectionChange;
        public event EventHandler DropdownOpen;
        public event EventHandler DropdownClose;
        private bool clickedYou;
        private bool showDropdown;
        private readonly string label;
        private readonly bool isMenu;
        public string[] DropdownList { get; private set; }
        public int DropdownID { get; }
        private Vector2 scrollPos;
        public Vector2 ScrollPos => scrollPos;
        private Rect buttonRect;
        public Rect ButtonRect
        {
            get => buttonRect;
            private set => buttonRect = value;
        }
        private Vector2 elementSize;
        public Vector2 ElementSize => elementSize;
        private int selectedItemIndex;
        public int SelectedItemIndex
        {
            get => selectedItemIndex;
            set
            {
                selectedItemIndex = Mathf.Clamp(value, 0, DropdownList.Length - 1);
                OnDropdownEvent(SelectionChange);
            }
        }
        public string SelectedItem => DropdownList[SelectedItemIndex];

        public Dropdown(string label, string[] itemList, int selectedItemIndex = 0)
            : this(itemList, selectedItemIndex)
        {
            isMenu = true;
            this.label = label;
        }

        public Dropdown(string[] itemList, int selectedItemIndex = 0)
        {
            DropdownID = DropdownHelper.DropdownID;
            SetDropdownItems(itemList, selectedItemIndex);

            DropdownHelper.SelectionChange += OnChangeSelection;
            DropdownHelper.DropdownClose += OnCloseDropdown;
        }

        // TODO: I don't think this works the way I think it does
        ~Dropdown()
        {
            DropdownHelper.SelectionChange -= OnChangeSelection;
            DropdownHelper.DropdownClose -= OnCloseDropdown;
        }

        public void SetDropdownItems(string[] itemList, int selectedItemIndex = -1)
        {
            if (selectedItemIndex < 0) selectedItemIndex = SelectedItemIndex;
            elementSize = Vector2.zero;

            // TODO: Calculate scrollpos position maybe
            if ((selectedItemIndex != this.selectedItemIndex) || (itemList.Length != DropdownList?.Length))
            {
                scrollPos = Vector2.zero;
            }
            DropdownList = itemList;
            SelectedItemIndex = selectedItemIndex;
        }

        public void SetDropdownItem(int index, string newItem)
        {
            if (index < 0 || index >= DropdownList.Length) return;

            Vector2 itemSize = DropdownHelper.CalculateElementSize(newItem);

            if (itemSize.x > ElementSize.x) elementSize = itemSize;

            DropdownList[index] = newItem;
        }

        public void SetDropdownItem(string newItem)
        {
            SetDropdownItem(SelectedItemIndex, newItem);
        }

        public void Step(int dir)
        {
            dir = (int)Mathf.Sign(dir);
            SelectedItemIndex = Utility.Wrap(SelectedItemIndex + dir, 0, DropdownList.Length);
        }

        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            Draw(buttonStyle, null, layoutOptions);
        }

        public void Draw(GUIStyle buttonStyle, GUIStyle dropdownStyle = null, params GUILayoutOption[] layoutOptions)
        {
            bool clicked = GUILayout.Button(
                isMenu ? label : DropdownList[selectedItemIndex], buttonStyle, layoutOptions
            );

            if (clicked)
            {
                showDropdown = !clickedYou;
                clickedYou = false;
            }

            if (showDropdown && Event.current.type == EventType.Repaint) InitializeDropdown(dropdownStyle);
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            Draw(buttonStyle, layoutOptions);
        }

        private void OnChangeSelection(object sender, DropdownSelectArgs args)
        {
            if (args.DropdownID == DropdownID)
            {
                SelectedItemIndex = args.SelectedItemIndex;
            }
        }

        private void OnCloseDropdown(object sender, DropdownCloseArgs args)
        {
            if (args.DropdownID == DropdownID)
            {
                scrollPos = args.ScrollPos;
                clickedYou = args.ClickedYou;

                if (clickedYou) OnDropdownEvent(SelectionChange);

                OnDropdownEvent(DropdownClose);
            }
        }

        private void InitializeDropdown(GUIStyle dropdownStyle)
        {
            showDropdown = false;

            buttonRect = GUILayoutUtility.GetLastRect();
            Vector2 rectPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.y));
            buttonRect.x = rectPos.x;
            buttonRect.y = rectPos.y;
            if (elementSize == Vector2.zero)
            {
                elementSize = DropdownHelper.CalculateElementSize(DropdownList, dropdownStyle);
            }
            DropdownHelper.Set(this, dropdownStyle);

            OnDropdownEvent(DropdownOpen);
        }

        private void OnDropdownEvent(EventHandler handler)
        {
            handler?.Invoke(this, EventArgs.Empty);
        }
    }

    public static class DropdownHelper
    {
        public static event EventHandler<DropdownSelectArgs> SelectionChange;
        public static event EventHandler<DropdownCloseArgs> DropdownClose;
        private static int dropdownID = 100;
        public static int DropdownID => dropdownID++;
        private static GUIStyle defaultDropdownStyle;
        public static GUIStyle DefaultDropdownStyle
        {
            get
            {
                if (!initialized) InitializeStyle();
                return defaultDropdownStyle;
            }
        }
        private static GUIStyle dropdownStyle;
        private static GUIStyle windowStyle;
        private static Rect buttonRect;
        private static string[] dropdownList;
        private static Vector2 scrollPos;
        private static int currentDropdownID;
        private static int selectedItemIndex;
        private static bool initialized;
        public static bool Visible { get; set; }
        public static bool DropdownOpen { get; private set; }
        private static bool onScrollBar;
        public static Rect dropdownWindow;
        private static Rect dropdownScrollRect;
        private static Rect dropdownRect;

        public static Vector2 CalculateElementSize(string item, GUIStyle style = null)
        {
            if (!initialized) InitializeStyle();

            style = style ?? DefaultDropdownStyle;

            return style.CalcSize(new GUIContent(item));
        }

        public static Vector2 CalculateElementSize(string[] list, GUIStyle style = null)
        {
            if (!initialized) InitializeStyle();

            style = style ?? DefaultDropdownStyle;

            GUIContent content = new GUIContent(list[0]);
            Vector2 calculatedSize = style.CalcSize(content);
            for (int i = 1; i < list.Length; i++)
            {
                content.text = list[i];
                Vector2 calcSize = style.CalcSize(content);
                if (calcSize.x > calculatedSize.x) calculatedSize = calcSize;
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
            Vector2 calculatedSize = dropdown.ElementSize;

            float calculatedListHeight = calculatedSize.y * dropdownList.Length;

            float heightAbove = buttonRect.y;
            float heightBelow = Screen.height - heightAbove - buttonRect.height;

            float rectWidth = Mathf.Max(calculatedSize.x + 5, buttonRect.width);
            float rectHeight = Mathf.Min(calculatedListHeight, Mathf.Max(heightAbove, heightBelow));

            if (calculatedListHeight > heightBelow && heightAbove > heightBelow)
            {
                dropdownWindow = new Rect(buttonRect.x, buttonRect.y - rectHeight, rectWidth + 18, rectHeight);
            }
            else
            {
                if (calculatedListHeight > heightBelow) rectHeight -= calculatedSize.y;
                dropdownWindow = new Rect(buttonRect.x, buttonRect.y + buttonRect.height, rectWidth + 18, rectHeight);
            }

            dropdownWindow.x = Mathf.Clamp(dropdownWindow.x, 0, Screen.width - rectWidth - 18);

            dropdownScrollRect = new Rect(0, 0, dropdownWindow.width, dropdownWindow.height);
            dropdownRect = new Rect(0, 0, dropdownWindow.width - 18, calculatedListHeight);

            DropdownOpen = true;
            Visible = true;
        }

        public static void HandleDropdown()
        {
            dropdownWindow = GUI.Window(Constants.dropdownWindowID, dropdownWindow, GUIFunc, "", windowStyle);
            if (Input.mouseScrollDelta.y != 0f && Visible && dropdownWindow.Contains(Event.current.mousePosition))
            {
                Input.ResetInputAxes();
            }
        }

        private static void GUIFunc(int id)
        {
            bool clicked = false;

            if (Event.current.type == EventType.MouseUp) clicked = true;

            scrollPos = GUI.BeginScrollView(dropdownScrollRect, scrollPos, dropdownRect);
            int selection = GUI.SelectionGrid(dropdownRect, selectedItemIndex, dropdownList, 1, dropdownStyle);
            GUI.EndScrollView();

            bool clickedYou = false;
            if (Utility.AnyMouseDown())
            {
                Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                bool clickedMe = dropdownWindow.Contains(mousePos);
                onScrollBar = mousePos.x > dropdownWindow.x + dropdownWindow.width - 12f;
                if (buttonRect.Contains(mousePos)) clickedYou = true;
                if (!clickedMe) DropdownOpen = false;
            }

            if (selection != selectedItemIndex || (clicked && !onScrollBar))
            {
                SelectionChange?.Invoke(null, new DropdownSelectArgs(currentDropdownID, selection));
                DropdownOpen = false;
            }

            if (!DropdownOpen)
            {
                Visible = false;
                DropdownClose?.Invoke(null, new DropdownCloseArgs(currentDropdownID, scrollPos, clickedYou));
            }
        }

        private static void InitializeStyle()
        {
            defaultDropdownStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(0, 0, 0, 0)
            };
            defaultDropdownStyle.padding.top = defaultDropdownStyle.padding.bottom = 2;
            defaultDropdownStyle.normal.background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f));
            Texture2D whiteBackground = new Texture2D(2, 2);
            defaultDropdownStyle.onHover.background
                = defaultDropdownStyle.hover.background
                = defaultDropdownStyle.onNormal.background
                = whiteBackground;
            defaultDropdownStyle.onHover.textColor
                = defaultDropdownStyle.onNormal.textColor
                = defaultDropdownStyle.hover.textColor
                = Color.black;

            windowStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.UpperRight
            };
            initialized = true;
        }

        public class DropdownEventArgs : EventArgs
        {
            public int DropdownID { get; }
            public DropdownEventArgs(int dropdownID) => DropdownID = dropdownID;
        }

        public class DropdownSelectArgs : DropdownEventArgs
        {
            public int SelectedItemIndex { get; }
            public DropdownSelectArgs(int dropdownID, int selection) : base(dropdownID)
            {
                SelectedItemIndex = selection;
            }
        }

        public class DropdownCloseArgs : DropdownEventArgs
        {
            public Vector2 ScrollPos { get; }
            public bool ClickedYou { get; }
            public DropdownCloseArgs(int dropdownID, Vector2 scrollPos, bool clickedYou = false) : base(dropdownID)
            {
                ScrollPos = scrollPos;
                ClickedYou = clickedYou;
            }
        }
    }
}
