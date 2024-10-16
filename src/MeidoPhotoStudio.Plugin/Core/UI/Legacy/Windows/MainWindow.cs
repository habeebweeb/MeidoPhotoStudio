using MeidoPhotoStudio.Plugin.Framework.Service;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window.</summary>
public partial class MainWindow : BaseWindow
{
    private const float ResizeHandleSize = 15f;
    private const float MinimumWindowWidth = 255f;
    private const float MinimumWindowHeight = 400f;

    private readonly LazyStyle pluginInfoStyle = new(
        10,
        () => new(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
        });

    private readonly LazyStyle buttonStyle = new(13, () => new(GUI.skin.button));
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<Constants.Window, BaseMainWindowPane> windowPanes;
    private readonly CustomMaidSceneService customMaidSceneService;
    private readonly InputRemapper inputRemapper;
    private readonly TabsPane tabsPane;
    private readonly Button settingsButton;

    private Rect resizeHandleRect = new(0f, 0f, ResizeHandleSize, ResizeHandleSize);
    private bool resizing;
    private BaseMainWindowPane currentWindowPane;
    private string settingsButtonLabel;
    private string closeButtonLabel;
    private Constants.Window selectedWindow;

    public MainWindow(
        TabSelectionController tabSelectionController,
        CustomMaidSceneService customMaidSceneService,
        InputRemapper inputRemapper)
    {
        this.tabSelectionController = tabSelectionController;

        this.tabSelectionController.TabSelected += (_, e) =>
            ChangeWindow(e.Tab);

        this.customMaidSceneService = customMaidSceneService;
        this.inputRemapper = inputRemapper ? inputRemapper : throw new ArgumentNullException(nameof(inputRemapper));

        windowPanes = [];
        WindowRect = new(Screen.width, Screen.height * 0.08f, MinimumWindowWidth, Screen.height * 0.9f);

        tabsPane = AddPane<TabsPane>(new());
        tabsPane.TabChange += (_, _) =>
            ChangeTab();

        settingsButtonLabel = Translation.Get("settingsLabels", "settingsButton");
        closeButtonLabel = Translation.Get("settingsLabels", "closeSettingsButton");

        settingsButton = new(settingsButtonLabel);
        settingsButton.ControlEvent += (_, _) =>
        {
            if (selectedWindow is Constants.Window.Settings)
            {
                ChangeTab();
            }
            else
            {
                settingsButton.Label = closeButtonLabel;
                SetCurrentWindow(Constants.Window.Settings);
            }
        };
    }

    public override Rect WindowRect
    {
        set
        {
            value.x = Mathf.Clamp(value.x, 0, Screen.width - value.width);
            value.y = Mathf.Clamp(value.y, -value.height + 30, Screen.height - 50);

            windowRect = value;
        }
    }

    public BaseMainWindowPane this[Constants.Window id]
    {
        get => windowPanes[id];
        set => AddWindow(id, value);
    }

    public void AddWindow(Constants.Window id, BaseMainWindowPane window)
    {
        if (windowPanes.ContainsKey(id))
            Panes.Remove(windowPanes[id]);

        windowPanes[id] = window;
        windowPanes[id].SetTabsPane(tabsPane);
        windowPanes[id].SetParent(this);

        Panes.Add(windowPanes[id]);
    }

    public override void OnScreenDimensionsChanged(Vector2 newScreenDimensions)
    {
        base.OnScreenDimensionsChanged(newScreenDimensions);

        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            ClampWindowWidth(Screen.width * 0.13f),
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;
    }

    public override void Activate()
    {
        base.Activate();

        tabsPane.SelectedTab = Constants.Window.Call;
        Visible = true;

        var newWindowRect = new Rect(
            Screen.width,
            Screen.height * 0.08f,
            ClampWindowWidth(Screen.width * 0.13f),
            Screen.height * 0.9f);

        if (customMaidSceneService.EditScene)
            newWindowRect.height *= 0.85f;

        WindowRect = newWindowRect;
    }

    public override void GUIFunc(int id)
    {
        HandleResize();

        Draw();

        if (!resizing)
            GUI.DragWindow();

        void HandleResize()
        {
            resizeHandleRect = resizeHandleRect with
            {
                x = 0f,
                y = windowRect.height - ResizeHandleSize,
            };

            if (resizing && !Input.GetMouseButton(0))
                resizing = false;
            else if (!resizing && Input.GetMouseButtonDown(0) && resizeHandleRect.Contains(Event.current.mousePosition))
                resizing = true;

            if (resizing)
            {
                var minimumWindowWidth = Mathf.Max(MinimumWindowWidth, Utility.GetPix(MinimumWindowWidth));
                var xMin = Mathf.Max(0f, Mathf.Min(windowRect.xMax - minimumWindowWidth, Input.mousePosition.x - ResizeHandleSize / 2f));
                var height = Mathf.Max(MinimumWindowHeight, Event.current.mousePosition.y + ResizeHandleSize / 2f);

                WindowRect = windowRect with
                {
                    xMin = Mathf.RoundToInt(xMin),
                    height = height,
                };
            }
        }
    }

    public override void Draw()
    {
        currentWindowPane?.Draw();

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        GUILayout.Space(ResizeHandleSize + 3f);

        GUILayout.Label(Plugin.PluginString, pluginInfoStyle);

        GUILayout.FlexibleSpace();

        GUI.enabled = !inputRemapper.Listening;

        settingsButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUI.Box(resizeHandleRect, GUIContent.none);
    }

    protected override void ReloadTranslation()
    {
        settingsButtonLabel = Translation.Get("settingsLabels", "settingsButton");
        closeButtonLabel = Translation.Get("settingsLabels", "closeSettingsButton");
        settingsButton.Label = selectedWindow == Constants.Window.Settings ? closeButtonLabel : settingsButtonLabel;
    }

    private float ClampWindowWidth(float width) =>
        Mathf.Max(MinimumWindowWidth, Mathf.Min(Utility.GetPix(MinimumWindowWidth), width));

    private void ChangeTab()
    {
        settingsButton.Label = Translation.Get("settingsLabels", "settingsButton");
        SetCurrentWindow(tabsPane.SelectedTab);
    }

    private void SetCurrentWindow(Constants.Window window)
    {
        if (currentWindowPane is not null)
            currentWindowPane.ActiveWindow = false;

        selectedWindow = window;
        currentWindowPane = windowPanes[selectedWindow];
        currentWindowPane.ActiveWindow = true;
        currentWindowPane.UpdatePanes();
    }

    private void ChangeWindow(Constants.Window window)
    {
        if (selectedWindow == window)
            currentWindowPane.UpdatePanes();
        else
            tabsPane.SelectedTab = window;

        Visible = true;
    }
}
