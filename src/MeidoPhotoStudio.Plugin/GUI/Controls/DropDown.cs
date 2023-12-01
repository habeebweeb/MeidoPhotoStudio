using System;

using UnityEngine;

using DropdownCloseArgs = MeidoPhotoStudio.Plugin.DropdownHelper.DropdownCloseArgs;
using DropdownSelectArgs = MeidoPhotoStudio.Plugin.DropdownHelper.DropdownSelectArgs;

namespace MeidoPhotoStudio.Plugin;

public class Dropdown : BaseControl
{
    private readonly string label;
    private readonly bool isMenu;

    private Vector2 elementSize;
    private int selectedItemIndex;
    private bool clickedYou;
    private bool showDropdown;
    private Vector2 scrollPos;
    private Rect buttonRect;

    public Dropdown(string label, string[] itemList, int selectedItemIndex = 0)
        : this(itemList, selectedItemIndex)
    {
        this.label = label;

        isMenu = true;
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

    public event EventHandler SelectionChange;

    public event EventHandler DropdownOpen;

    public event EventHandler DropdownClose;

    public int DropdownID { get; }

    public string[] DropdownList { get; private set; }

    public Vector2 ScrollPos =>
        scrollPos;

    public string SelectedItem =>
        DropdownList[SelectedItemIndex];

    public Rect ButtonRect
    {
        get => buttonRect;
        private set => buttonRect = value;
    }

    public Vector2 ElementSize =>
        elementSize;

    public int SelectedItemIndex
    {
        get => selectedItemIndex;
        set => SetIndex(value);
    }

    public void SetDropdownItems(string[] itemList, int selectedItemIndex = -1)
    {
        if (selectedItemIndex < 0)
            selectedItemIndex = SelectedItemIndex;

        elementSize = Vector2.zero;

        // TODO: Calculate scrollpos position maybe
        if (selectedItemIndex != this.selectedItemIndex || itemList.Length != DropdownList?.Length)
            scrollPos = Vector2.zero;

        DropdownList = itemList;
        SelectedItemIndex = selectedItemIndex;
    }

    public void SetDropdownItemsWithoutNotify(string[] itemList, int selectedItemIndex = -1)
    {
        if (selectedItemIndex < 0)
            selectedItemIndex = SelectedItemIndex;

        elementSize = Vector2.zero;

        if (selectedItemIndex != this.selectedItemIndex || itemList.Length != DropdownList?.Length)
            scrollPos = Vector2.zero;

        DropdownList = itemList;
        SetIndexWithoutNotify(selectedItemIndex);
    }

    public void SetDropdownItem(int index, string newItem)
    {
        if (index < 0 || index >= DropdownList.Length)
            return;

        var itemSize = DropdownHelper.CalculateElementSize(newItem);

        if (itemSize.x > ElementSize.x)
            elementSize = itemSize;

        DropdownList[index] = newItem;
    }

    public void SetDropdownItem(string newItem) =>
        SetDropdownItem(SelectedItemIndex, newItem);

    public void Step(int dir)
    {
        dir = (int)Mathf.Sign(dir);
        SelectedItemIndex = Utility.Wrap(SelectedItemIndex + dir, 0, DropdownList.Length);
    }

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions) =>
        Draw(buttonStyle, null, layoutOptions);

    public void Draw(GUIStyle buttonStyle, GUIStyle dropdownStyle = null, params GUILayoutOption[] layoutOptions)
    {
        var clicked = GUILayout.Button(isMenu ? label : DropdownList[selectedItemIndex], buttonStyle, layoutOptions);

        if (clicked)
        {
            showDropdown = !clickedYou;
            clickedYou = false;
        }

        if (showDropdown && Event.current.type is EventType.Repaint)
            InitializeDropdown(dropdownStyle);
    }

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        };

        Draw(buttonStyle, layoutOptions);
    }

    public void SetIndexWithoutNotify(int index) =>
        SetIndex(index, false);

    private void OnChangeSelection(object sender, DropdownSelectArgs args)
    {
        if (args.DropdownID == DropdownID)
            SelectedItemIndex = args.SelectedItemIndex;
    }

    private void OnCloseDropdown(object sender, DropdownCloseArgs args)
    {
        if (args.DropdownID != DropdownID)
            return;

        scrollPos = args.ScrollPos;
        clickedYou = args.ClickedYou;

        if (clickedYou)
            OnDropdownEvent(SelectionChange);

        OnDropdownEvent(DropdownClose);
    }

    private void InitializeDropdown(GUIStyle dropdownStyle)
    {
        showDropdown = false;

        buttonRect = GUILayoutUtility.GetLastRect();

        var rectPos = GUIUtility.GUIToScreenPoint(new(buttonRect.x, buttonRect.y));

        buttonRect.x = rectPos.x;
        buttonRect.y = rectPos.y;

        if (elementSize == Vector2.zero)
            elementSize = DropdownHelper.CalculateElementSize(DropdownList, dropdownStyle);

        DropdownHelper.Set(this, dropdownStyle);

        OnDropdownEvent(DropdownOpen);
    }

    private void SetIndex(int index, bool notify = true)
    {
        selectedItemIndex = Mathf.Clamp(index, 0, DropdownList.Length - 1);

        if (notify)
            OnDropdownEvent(SelectionChange);
    }

    private void OnDropdownEvent(EventHandler handler) =>
        handler?.Invoke(this, EventArgs.Empty);
}
