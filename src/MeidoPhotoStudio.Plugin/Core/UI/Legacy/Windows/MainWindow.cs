using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;
using MeidoPhotoStudio.Plugin.Service;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

/// <summary>Main window.</summary>
public partial class MainWindow : BaseWindow
{
    private const float MinimumWindowWidth = 215f;

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
            value.width = Mathf.Max(MinimumWindowWidth, Screen.width * 0.13f);
            value.height = Screen.height * 0.9f;

            if (customMaidSceneService.EditScene)
                value.height *= 0.85f;

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

    public override void Activate()
    {
        base.Activate();

        tabsPane.SelectedTab = Constants.Window.Call;
        Visible = true;
    }

    public override void Draw()
    {
        currentWindowPane?.Draw();

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        GUILayout.Label(Plugin.PluginString, pluginInfoStyle);
        GUILayout.FlexibleSpace();

        GUI.enabled = !inputRemapper.Listening;

        settingsButton.Draw(buttonStyle, GUILayout.ExpandWidth(false));

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }

    protected override void ReloadTranslation()
    {
        settingsButtonLabel = Translation.Get("settingsLabels", "settingsButton");
        closeButtonLabel = Translation.Get("settingsLabels", "closeSettingsButton");
        settingsButton.Label = selectedWindow == Constants.Window.Settings ? closeButtonLabel : settingsButtonLabel;
    }

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
