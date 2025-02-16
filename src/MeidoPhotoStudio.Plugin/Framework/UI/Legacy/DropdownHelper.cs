using MeidoPhotoStudio.Plugin.Framework.Extensions;

using UInput = UnityEngine.Input;

namespace MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

internal static class DropdownHelper
{
    private static readonly LazyStyle WindowStyle = new(
        0,
        static () => new(GUI.skin.box)
        {
            padding = new(0, 0, 0, 0),
            alignment = TextAnchor.UpperRight,
        });

    private static readonly Dictionary<char, Vector2> CharacterDimensions = [];
    private static readonly VirtualList VirtualList = new();
    private static readonly GUIContent CharacterContent = new();

    private static GUIStyle calculationStyle;
    private static IDropdownHandler dropdownHandler;
    private static Rect buttonRect;
    private static Rect dropdownWindow;
    private static Rect dropdownScrollRect;

    static DropdownHelper()
    {
        ScreenSizeChecker.ScreenSizeChanged += OnScreenSizeChanged;

        static void OnScreenSizeChanged(object sender, EventArgs e)
        {
            calculationStyle = null;
            CharacterDimensions.Clear();
            VirtualList.Invalidate();

            CloseDropdown();
        }
    }

    public static bool Visible { get; private set; }

    internal static LazyStyle DropdownItemStyle { get; } = new(
        StyleSheet.TextSize,
        static () =>
        {
            var whiteBackground = new Texture2D(2, 2);

            return new(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                margin = new(0, 0, 0, 0),
                padding =
                {
                    top = 2,
                    bottom = 2,
                },
                normal = { background = UIUtility.CreateTexture(2, 2, new(0f, 0f, 0f, 0.5f)) },
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

        var windowWidth = Mathf.Max(scrollViewWidth + 12f, DropdownHelper.buttonRect.width);
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

    internal static void DrawDropdown() =>
        DrawDropdown(WindowStyle);

    internal static void DrawDropdown(GUIStyle windowStyle)
    {
        if (!Visible)
            return;

        dropdownWindow = GUI.Window(765, dropdownWindow, DropdownWindow, string.Empty, windowStyle);

        GUI.BringWindowToFront(765);

        if (AnyMouseDown() && Event.current.type is EventType.Repaint)
        {
            var mousePosition = new Vector2(UInput.mousePosition.x, Screen.height - UInput.mousePosition.y);

            if (!dropdownWindow.Contains(mousePosition))
                CloseDropdown(buttonRect.Contains(mousePosition));
        }

        static bool AnyMouseDown() =>
            UInput.GetMouseButtonDown(0) || UInput.GetMouseButtonDown(1) || UInput.GetMouseButtonDown(2);
    }

    internal static Vector2 CalculateItemDimensions(string value)
    {
        var lineCount = 0;
        var (totalWidth, totalHeight) = (0f, 0f);
        var (lineWidth, lineHeight) = (0f, 0f);

        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];
            var (characterWidth, characterHeight) = GetCharacterDimensions(character);

            if (character is '\n' or '\r')
            {
                if (character is '\r')
                {
                    if (i + 1 >= value.Length || value[i + 1] is not '\n')
                        continue;

                    (_, characterHeight) = GetCharacterDimensions('\n');

                    i++;
                }

                characterHeight /= 2f;

                if (characterHeight >= lineHeight)
                    lineHeight = characterHeight;

                if (lineWidth >= totalWidth)
                    totalWidth = lineWidth;

                totalHeight += lineHeight;

                lineCount++;
                lineWidth = 0f;
                lineHeight = 0f;
            }
            else
            {
                lineWidth += characterWidth;

                if (characterHeight >= lineHeight)
                    lineHeight = characterHeight;

                if (lineWidth >= totalWidth)
                    totalWidth = lineWidth;

                if (lineHeight >= totalHeight)
                    totalHeight = lineHeight;
            }
        }

        return new(totalWidth + 12, totalHeight + (lineCount + 1) * 2);

        static Vector2 GetCharacterDimensions(char character)
        {
            if (!CharacterDimensions.TryGetValue(character, out var dimensions))
            {
                calculationStyle ??= new GUIStyle((GUIStyle)DropdownItemStyle)
                {
                    padding = new(0, 0, 0, 0),
                    margin = new(0, 0, 0, 0),
                    border = new(0, 0, 0, 0),
                };

                CharacterContent.text = character.ToString();
                dimensions = calculationStyle.CalcSize(CharacterContent);
                CharacterDimensions[character] = dimensions;
            }

            return dimensions;
        }
    }

    internal static bool MouseOverDropdown(Vector3 mousePosition) =>
        Visible && dropdownWindow.Contains(mousePosition);

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
                DropdownItemStyle);

            if (value != (dropdownHandler.SelectedItemIndex == index))
            {
                dropdownHandler.OnItemSelected(index);
                CloseDropdown();

                break;
            }
        }

        GUI.EndScrollView();
    }

    private static void CloseDropdown(bool clickedButton = false)
    {
        dropdownHandler?.OnDropdownClosed(clickedButton);
        Visible = false;
    }
}
