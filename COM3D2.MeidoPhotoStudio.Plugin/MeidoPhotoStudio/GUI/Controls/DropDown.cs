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
        private bool clickedYou = false;
        private bool showDropdown = false;
        public string[] DropdownList { get; private set; }
        public int DropdownID { get; private set; }
        private Vector2 scrollPos;
        public Vector2 ScrollPos
        {
            get => scrollPos;
            private set => scrollPos = value;
        }
        private Rect buttonRect;
        public Rect ButtonRect
        {
            get => buttonRect;
            private set => buttonRect = value;
        }
        private Vector2 elementSize;
        public Vector2 ElementSize
        {
            get => elementSize;
            set => elementSize = value;
        }
        private int selectedItemIndex = 0;
        public int SelectedItemIndex
        {
            get => selectedItemIndex;
            set
            {
                this.selectedItemIndex = Mathf.Clamp(value, 0, DropdownList.Length);
                OnDropdownEvent(SelectionChange);
            }
        }

        public Dropdown(string[] itemList, int selectedItemIndex = 0)
        {
            DropdownID = DropdownHelper.DropdownID;
            SetDropdownItems(itemList, selectedItemIndex);

            DropdownHelper.SelectionChange += OnChangeSelection;
            DropdownHelper.DropdownClose += OnCloseDropdown;
        }

        ~Dropdown()
        {
            DropdownHelper.SelectionChange -= OnChangeSelection;
            DropdownHelper.DropdownClose -= OnCloseDropdown;
        }

        public void SetDropdownItems(string[] itemList, int selectedItemIndex = 0)
        {
            this.scrollPos = (this.elementSize = Vector2.zero);
            this.DropdownList = itemList;
            this.SelectedItemIndex = selectedItemIndex;
        }
        public void Step(int dir)
        {
            dir = (int)Mathf.Sign(dir);
            this.SelectedItemIndex = Utility.Wrap(this.SelectedItemIndex + dir, 0, this.DropdownList.Length);
        }

        public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions)
        {
            bool clicked = GUILayout.Button(DropdownList[selectedItemIndex], buttonStyle, layoutOptions);

            if (clicked)
            {
                showDropdown = !clickedYou;
                clickedYou = false;
            }

            if (showDropdown)
            {
                if (Event.current.type == EventType.Repaint) InitializeDropdown();
            }
        }

        public override void Draw(params GUILayoutOption[] layoutOptions)
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.alignment = TextAnchor.MiddleLeft;
            this.Draw(buttonStyle, layoutOptions);
        }


        private void OnChangeSelection(object sender, DropdownSelectArgs args)
        {
            if (args.DropdownID == this.DropdownID)
            {
                SelectedItemIndex = args.SelectedItemIndex;
            }
        }

        private void OnCloseDropdown(object sender, DropdownCloseArgs args)
        {
            if (args.DropdownID == this.DropdownID)
            {
                scrollPos = args.ScrollPos;
                clickedYou = args.ClickedYou;

                if (clickedYou) OnDropdownEvent(SelectionChange);

                OnDropdownEvent(DropdownClose);
            }
        }
        private void InitializeDropdown()
        {
            showDropdown = false;

            this.buttonRect = GUILayoutUtility.GetLastRect();
            Vector2 rectPos = GUIUtility.GUIToScreenPoint(new Vector2(buttonRect.x, buttonRect.y));
            buttonRect.x = rectPos.x;
            buttonRect.y = rectPos.y;
            if (this.elementSize == Vector2.zero)
            {
                this.elementSize = DropdownHelper.CalculateElementSize(this.DropdownList);
            }
            DropdownHelper.Set(this);

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
        private static GUIStyle dropdownStyle;
        private static GUIStyle windowStyle;
        private static Rect buttonRect;
        private static string[] dropdownList;
        private static Vector2 scrollPos;
        private static int currentDropdownID;
        private static int selectedItemIndex;
        private static bool initialized = false;
        public static bool Visible { get; set; }
        public static bool DropdownOpen { get; private set; }
        private static bool onScrollBar = false;
        public static Rect dropdownWindow;
        private static Rect dropdownScrollRect;
        private static Rect dropdownRect;
        public static Vector2 CalculateElementSize(string[] list)
        {
            if (!initialized) InitializeStyle();

            Vector2 calculatedSize = dropdownStyle.CalcSize(new GUIContent(list[0]));
            for (int i = 1; i < list.Length; i++)
            {
                string word = list[i];
                Vector2 calcSize = dropdownStyle.CalcSize(new GUIContent(word));
                if (calcSize.x > calculatedSize.x) calculatedSize = calcSize;
            }

            return calculatedSize;
        }

        public static void Set(Dropdown dropdown)
        {
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
        }

        private static void GUIFunc(int id)
        {
            bool clicked = false;

            if (Event.current.type == EventType.MouseUp)
            {
                clicked = true;
            }

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
            dropdownStyle = new GUIStyle(GUI.skin.button);
            dropdownStyle.alignment = TextAnchor.MiddleLeft;
            dropdownStyle.margin = new RectOffset(0, 0, 0, 0);
            dropdownStyle.padding.top = dropdownStyle.padding.bottom = 2;
            dropdownStyle.normal.background = Utility.MakeTex(2, 2, new Color(0f, 0f, 0f, 0.5f));
            Texture2D whiteBackground = new Texture2D(2, 2);
            dropdownStyle.onHover.background
                = dropdownStyle.hover.background
                = dropdownStyle.onNormal.background
                = whiteBackground;
            dropdownStyle.onHover.textColor
                = dropdownStyle.onNormal.textColor
                = dropdownStyle.hover.textColor
                = Color.black;

            windowStyle = new GUIStyle(GUI.skin.box);
            windowStyle.padding = new RectOffset(0, 0, 0, 0);
            windowStyle.alignment = TextAnchor.UpperRight;
            initialized = true;
        }

        public class DropdownEventArgs : EventArgs
        {
            public int DropdownID { get; }
            public DropdownEventArgs(int dropdownID)
            {
                this.DropdownID = dropdownID;
            }
        }

        public class DropdownSelectArgs : DropdownEventArgs
        {
            public int SelectedItemIndex { get; }
            public DropdownSelectArgs(int dropdownID, int selection) : base(dropdownID)
            {
                this.SelectedItemIndex = selection;
            }
        }

        public class DropdownCloseArgs : DropdownEventArgs
        {
            public Vector2 ScrollPos { get; }
            public bool ClickedYou { get; }
            public DropdownCloseArgs(int dropdownID, Vector2 scrollPos, bool clickedYou = false) : base(dropdownID)
            {
                this.ScrollPos = scrollPos;
                this.ClickedYou = clickedYou;
            }
        }
    }
}
