using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

// TODO: Migrate other dropdowns to this dropdown
public class Dropdown2<T> : BaseControl, IEnumerable<T>
{
    private static readonly Func<T, int, string> DefaultItemFormatter = (T item, int index) =>
        item?.ToString() ?? string.Empty;

    private T[] items = [];
    private string[] formattedItems;
    private int selectedItemIndex;
    private Func<T, int, string> itemFormatter = DefaultItemFormatter;
    private Vector2? itemSize = null;
    private string label = string.Empty;
    private Vector2 scrollPosition;
    private bool clickedWhileOpen;
    private bool buttonClicked;

    public Dropdown2(Func<T, int, string> formatter = null) =>
        Formatter = formatter;

    public Dropdown2(IEnumerable<T> items, int selectedItemIndex = 0, Func<T, int, string> formatter = null)
        : this(formatter)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        SetItems(items, selectedItemIndex, notify: false);
    }

    public event EventHandler DropdownOpening;

    public event EventHandler<DropdownEventArgs<T>> SelectionChanged;

    public event EventHandler DropdownClosed;

    public T SelectedItem =>
        items.Any()
            ? items[SelectedItemIndex]
            : default;

    public int SelectedItemIndex
    {
        get => selectedItemIndex;
        set => SetSelectedItemIndex(value);
    }

    public Func<T, int, string> Formatter
    {
        get => itemFormatter;
        set
        {
            itemFormatter = value ?? DefaultItemFormatter;

            Reformat();
        }
    }

    internal int ID { get; } = DropdownHelper.DropdownID;

    public T this[int index] =>
        (uint)index >= items.Length ? throw new ArgumentOutOfRangeException(nameof(index)) :
        items[index];

    public IEnumerator<T> GetEnumerator() =>
        ((IEnumerable<T>)items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public override void Draw(params GUILayoutOption[] layoutOptions)
    {
        var buttonStyle = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        };

        Draw(buttonStyle, layoutOptions);
    }

    public void Draw(GUIStyle buttonStyle, params GUILayoutOption[] layoutOptions) =>
        Draw(buttonStyle, null, layoutOptions);

    public void Draw(GUIStyle buttonStyle, GUIStyle dropdownStyle = null, params GUILayoutOption[] layoutOptions)
    {
        var clicked = GUILayout.Button(label, buttonStyle, layoutOptions) && items.Length > 0;

        if (clicked)
        {
            buttonClicked = !clickedWhileOpen;
            clickedWhileOpen = false;
        }

        if (buttonClicked && Event.current.type is EventType.Repaint)
            OpenDropdown(GUILayoutUtility.GetLastRect());

        void OpenDropdown(Rect buttonRect)
        {
            DropdownOpening?.Invoke(this, EventArgs.Empty);

            buttonClicked = false;

            var rectPos = GUIUtility.GUIToScreenPoint(new(buttonRect.x, buttonRect.y));

            buttonRect.x = rectPos.x;
            buttonRect.y = rectPos.y;

            formattedItems ??= items
                .Select((item, index) => Formatter(item, index))
                .ToArray();

            itemSize ??= DropdownHelper.CalculateElementSize(formattedItems, dropdownStyle);

            DropdownHelper.SelectionChange += OnSelectionChanged;
            DropdownHelper.DropdownClose += OnDropdownClosed;

            DropdownHelper.OpenDropdown(
                this,
                scrollPosition,
                formattedItems,
                SelectedItemIndex,
                buttonRect,
                itemSize,
                dropdownStyle);

            void OnSelectionChanged(object sender, DropdownHelper.DropdownSelectArgs e)
            {
                if (e.DropdownID != ID)
                    return;

                DropdownHelper.SelectionChange -= OnSelectionChanged;

                SelectedItemIndex = e.SelectedItemIndex;
            }

            void OnDropdownClosed(object sender, DropdownHelper.DropdownCloseArgs e)
            {
                if (e.DropdownID != ID)
                    return;

                DropdownHelper.DropdownClose -= OnDropdownClosed;

                scrollPosition = e.ScrollPos;
                clickedWhileOpen = e.ClickedYou;

                if (clickedWhileOpen)
                    SelectionChanged?.Invoke(this, new(SelectedItem, SelectedItemIndex, SelectedItemIndex));

                DropdownClosed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void SetItems(IEnumerable<T> items, int? newIndex = null)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        SetItems(items, newIndex, true);
    }

    public void SetItemsWithoutNotify(IEnumerable<T> items, int? newIndex = null)
    {
        _ = items ?? throw new ArgumentNullException(nameof(items));

        SetItems(items, newIndex, false);
    }

    public void SetSelectedIndexWithoutNotify(int index) =>
        SetSelectedItemIndex(index, false);

    public void CycleNext() =>
        SelectedItemIndex = Wrap(SelectedItemIndex + 1, 0, items.Length);

    public void CyclePrevious() =>
        SelectedItemIndex = Wrap(SelectedItemIndex - 1, 0, items.Length);

    public void Reformat()
    {
        itemSize = null;
        formattedItems = null;

        label = SelectedItem is null ? string.Empty : Formatter(SelectedItem, selectedItemIndex);
    }

    private static int Wrap(int value, int min, int max) =>
        value < min ? max :
        value >= max ? min :
        value;

    private void SetItems(
        IEnumerable<T> items, int? newIndex = null, bool notify = true)
    {
        this.items = [.. items];

        SetSelectedItemIndex(newIndex ?? SelectedItemIndex, notify);
        Reformat();
    }

    private void SetSelectedItemIndex(int index, bool notify = true)
    {
        var previousSelectedItemIndex = selectedItemIndex;

        selectedItemIndex = Mathf.Clamp(index, 0, items.Length - 1);

        label = SelectedItem is null ? string.Empty : Formatter(SelectedItem, selectedItemIndex);

        if (!notify)
            return;

        SelectionChanged?.Invoke(this, new(SelectedItem, SelectedItemIndex, previousSelectedItemIndex));
    }
}
