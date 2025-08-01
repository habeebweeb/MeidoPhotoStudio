using MeidoPhotoStudio.Plugin.Core.Localization;
using MeidoPhotoStudio.Plugin.Framework.Extensions;
using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window.</summary>
public partial class MainWindow : BaseWindow
{
    public const int MinimumWindowWidth = 300;

    private const float ResizeHandleSize = 15f;
    private const float MinimumWindowHeight = 400f;
    private readonly Translation translation;
    private readonly TabSelectionController tabSelectionController;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly SettingsWindow settingsWindow;
    private readonly InputRemapper inputRemapper;
    private readonly Dictionary<Tab, Toggle> tabs = [];
    private readonly Dictionary<Tab, BasePane> windowPanes = [];
    private readonly Toggle.Group tabGroup;
    private readonly Button settingsButton;
    private readonly LazyStyle pluginInfoStyle = new(
        StyleSheet.SecondaryTextSize,
        static () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
        });

    private readonly LazyStyle tabsStyle = new(StyleSheet.TextSize, static () => new(GUI.skin.button));

    private int windowWidth = MinimumWindowWidth;
    private Rect resizeHandleRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private bool resizing;
    private BasePane currentPane;
    private Tab selectedTab;

    public MainWindow(
        Translation translation,
        TabSelectionController tabSelectionController,
        CustomMaidSceneService customMaidSceneService,
        InputRemapper inputRemapper,
        SettingsWindow settingsWindow)
    {
        this.translation = translation ?? throw new ArgumentNullException(nameof(translation));
        this.tabSelectionController = tabSelectionController ?? throw new ArgumentNullException(nameof(tabSelectionController));
        this.customMaidSceneService = customMaidSceneService ?? throw new ArgumentNullException(nameof(customMaidSceneService));
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));
        this.settingsWindow = settingsWindow ?? throw new ArgumentNullException(nameof(settingsWindow));

        this.tabSelectionController.TabSelected += OnTabSelectionChanged;

        tabGroup = [];

        settingsButton = new(new LocalizableGUIContent(this.translation, "mainWindow", "settingsButton"));
        settingsButton.ControlEvent += OnSettingsButtonPushed;

        WindowRect = new(
            Screen.width,
            Screen.height * 0.08f,
            Screen.width * 0.13f,
            Screen.height * 0.9f);
    }

    public enum Tab
    {
        Call,
        Character,
        CharacterPose,
        CharacterFace,
        Environment,
        Props,
    }

    public override bool Enabled =>
        base.Enabled && !inputRemapper.Listening;

    public override Rect WindowRect
    {
        set
        {
            base.WindowRect = value with
            {
                width = ClampWindowWidth(value.width),
                height = value.height,
                x = Mathf.Clamp(value.x, 0f, Screen.width - value.width),
                y = Mathf.Clamp(value.y, -value.height + 30f, Screen.height - 50f),
            };

            int ClampWindowWidth(float width)
            {
                var minimumWidth = Mathf.Max(WindowWidth, UIUtility.Scaled(WindowWidth));
                var maximumWidth = Screen.width - 20f;

                return Mathf.RoundToInt(Mathf.Clamp(width, minimumWidth, maximumWidth));
            }
        }
    }

    public int WindowWidth
    {
        get => windowWidth;
        set
        {
            var newWidth = value;

            if (newWidth < MinimumWindowWidth)
                newWidth = MinimumWindowWidth;

            windowWidth = newWidth;

            WindowRect = WindowRect with
            {
                width = Screen.width * 0.13f,
            };
        }
    }

    public BasePane this[Tab id]
    {
        get => windowPanes[id];
        set => AddTab(id, value);
    }

    public override void Draw()
    {
        GUILayout.BeginHorizontal();

        foreach (var tab in tabGroup)
            tab.Draw(tabsStyle);

        GUILayout.EndHorizontal();

        UIUtility.DrawBlackLine();

        currentPane.Draw();

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.Space(ResizeHandleSize + 3f);

        GUILayout.Label(Plugin.PluginString, pluginInfoStyle);

        GUILayout.FlexibleSpace();

        GUI.enabled = Enabled && !inputRemapper.Listening;

        settingsButton.Draw(GUILayout.ExpandWidth(false));

        GUILayout.EndHorizontal();
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            Screen.width * 0.13f,
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;

        foreach (var pane in windowPanes.Values)
            pane.OnScreenDimensionsChanged(newScreenDimensions);
    }

    public override void GUIFunc(int id)
    {
        HandleResize();

        GUI.enabled = Enabled;

        Draw();

        GUI.enabled = true;

        GUI.Box(resizeHandleRect, GUIContent.none);

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandleRect = resizeHandleRect with
            {
                x = 0f,
                y = WindowRect.height - ResizeHandleSize,
            };

            if (resizing && !Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && Input.GetMouseButtonDown(0) && resizeHandleRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var minimumWindowWidth = Mathf.Max(WindowWidth, UIUtility.Scaled(WindowWidth));
                var xMin = Mathf.Max(0f, Mathf.Min(WindowRect.xMax - minimumWindowWidth, Input.mousePosition.x - ResizeHandleSize / 2f));
                var height = Mathf.Max(MinimumWindowHeight, Event.current.mousePosition.y + ResizeHandleSize / 2f);

                WindowRect = WindowRect with
                {
                    xMin = Mathf.RoundToInt(xMin),
                    height = height,
                };
            }
        }
    }

    public override void Activate()
    {
        SetTab(Tab.Call);

        Visible = true;

        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            Screen.width * 0.13f,
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;

        foreach (var pane in windowPanes.Values)
            pane.Activate();
    }

    private void OnTabSelectionChanged(object sender, TabSelectionEventArgs e)
    {
        var newTab = e.Tab switch
        {
            Tab.CharacterFace or Tab.CharacterPose => Tab.Character,
            _ => e.Tab,
        };

        SetTab(newTab);

        if (!Visible && e.ForceVisible)
            Visible = true;
    }

    private void OnSettingsButtonPushed(object sender, EventArgs e) =>
        settingsWindow.Visible = !settingsWindow.Visible;

    private void AddTab(Tab tab, BasePane window)
    {
        if (windowPanes.ContainsKey(tab))
            return;

        windowPanes[tab] = window;
        windowPanes[tab].SetParent(this);

        var toggle = new Toggle(new LocalizableGUIContent(translation, "mainWindowTabs", tab.ToLower()));

        toggle.ControlEvent += (sender, _) =>
        {
            if (sender is not Toggle { Value: true })
                return;

            SetTab(tab);
        };

        tabs[tab] = toggle;
        tabGroup.Add(toggle);
    }

    private void SetTab(Tab tab)
    {
        selectedTab = tab switch
        {
            Tab.CharacterFace or Tab.CharacterPose => Tab.Character,
            _ => tab,
        };

        tabs[tab].SetEnabledWithoutNotify(true);

        currentPane = windowPanes[selectedTab];
    }
}
