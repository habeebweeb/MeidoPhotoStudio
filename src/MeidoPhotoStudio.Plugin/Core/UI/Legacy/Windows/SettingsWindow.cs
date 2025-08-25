using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class SettingsWindow : BaseWindow
{
    private const int CategoryListWidth = 200;
    private const float ResizeHandleSize = 15f;

    private readonly Translation translation;
    private readonly InputRemapper inputRemapper;
    private readonly List<SettingType> settingCategories = [];
    private readonly Dictionary<SettingType, BasePane> settingPanes = [];
    private readonly Toggle.Group settingsGroup;
    private readonly Button closeButton;
    private readonly Label pluginInformationLabel;
    private readonly LazyStyle settingCategoryStyle = new(
        StyleSheet.TextSize,
        static () => new(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleLeft,
        });

    private readonly LazyStyle buildVersionStyle = new(
        StyleSheet.SecondaryTextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
        });

    private bool resizing;
    private Rect resizeHandlRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private Vector2 settingsCategoryScrollPosition;
    private Vector2 settingsScrollPosition;
    private BasePane currentPane;

    public SettingsWindow(Translation translation, InputRemapper inputRemapper)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        settingsGroup = new();

        closeButton = new("X");
        closeButton.ControlEvent += OnCloseButtonPushed;

        pluginInformationLabel = new(Plugin.BuildVersion);

        WindowRect = UIUtility.MiddlePosition(MinimumWidth, Screen.height * 0.7f);
    }

    public enum SettingType
    {
        Controls,
        DragHandle,
        ShapeKeys,
        AutoSave,
        Translation,
        UI,
        Character,
        StartupPreset,
        Props,
    }

    public override bool Enabled =>
        base.Enabled && !inputRemapper.Listening;

    private static int MinimumWidth =>
        UIUtility.Scaled(CategoryListWidth * 3.25f) + 38;

    private static int MinimumHeight =>
        UIUtility.Scaled(500);

    public BasePane this[SettingType settingType]
    {
        get => settingPanes[settingType];
        set => AddPane(settingType, value);
    }

    public override void Draw()
    {
        GUILayout.BeginArea(new(10f, 10f, WindowRect.width - 20f, WindowRect.height - 20f));

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        pluginInformationLabel.Draw(buildVersionStyle);

        GUILayout.FlexibleSpace();

        closeButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        GUILayout.BeginHorizontal();

        DrawSettingsCategories();

        DrawSettingsPane();

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndArea();

        void DrawSettingsCategories()
        {
            var categoryWidth = GUILayout.Width(UIUtility.Scaled(CategoryListWidth));

            GUILayout.BeginVertical(categoryWidth);

            settingsCategoryScrollPosition = GUILayout.BeginScrollView(settingsCategoryScrollPosition);

            foreach (var toggle in settingsGroup)
                toggle.Draw(settingCategoryStyle);

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        void DrawSettingsPane()
        {
            GUILayout.BeginVertical();

            settingsScrollPosition = GUILayout.BeginScrollView(settingsScrollPosition);

            currentPane.Draw();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }

    public override void GUIFunc(int id)
    {
        HandleResize();

        GUI.enabled = Enabled;

        Draw();

        GUI.enabled = true;

        GUI.Box(resizeHandlRect, GUIContent.none);

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandlRect = resizeHandlRect with
            {
                x = WindowRect.width - ResizeHandleSize,
                y = WindowRect.height - ResizeHandleSize,
            };

            if (resizing && !Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && Input.GetMouseButtonDown(0) && resizeHandlRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var mousePosition = Event.current.mousePosition;

                var (windowWidth, windowHeight) = mousePosition;
                var minimumWidth = MinimumWidth;
                var minimumHeight = MinimumHeight;

                WindowRect = WindowRect with
                {
                    width = Mathf.Max(minimumWidth, windowWidth + ResizeHandleSize / 2f),
                    height = Mathf.Max(minimumHeight, windowHeight + ResizeHandleSize / 2f),
                };
            }
        }
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        var minimumWidth = MinimumWidth;
        var minimumHeight = MinimumHeight;

        WindowRect = WindowRect with
        {
            width = Mathf.Clamp(WindowRect.width, minimumWidth, Screen.width),
            height = Mathf.Clamp(WindowRect.height, minimumHeight, Screen.height),
        };
    }

    private void AddPane(SettingType settingType, BasePane pane)
    {
        _ = pane ?? throw new ArgumentNullException(nameof(pane));

        if (settingPanes.ContainsKey(settingType))
            return;

        settingCategories.Add(settingType);

        settingPanes.Add(settingType, pane);
        pane.SetParent(this);

        var toggle = new Toggle(
            new LocalizableGUIContent(translation, "settingTypes", settingType.ToLower()),
            currentPane is null);

        toggle.ControlEvent += (sender, _) =>
        {
            if (sender is not Toggle { Value: true })
                return;

            currentPane = pane;
        };

        currentPane ??= pane;
        settingsGroup.Add(toggle);
    }

    private void OnCloseButtonPushed(object sender, EventArgs e) =>
        Visible = !Visible;
}
