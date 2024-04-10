namespace MeidoPhotoStudio.Plugin;

public class CharacterWindowPane : BaseMainWindowPane
{
    private readonly CharacterSwitcherPane characterSwitcherPane;
    private readonly TabSelectionController tabSelectionController;
    private readonly Dictionary<CharacterWindowTab, CharacterWindowTabPane> windowPanes =
        new(EnumEqualityComparer<CharacterWindowTab>.Instance);

    private readonly SelectionGrid tabs;

    private CharacterWindowTabPane currentTab;

    public CharacterWindowPane(
        CharacterSwitcherPane characterSwitcherPane,
        TabSelectionController tabSelectionController)
    {
        this.characterSwitcherPane = characterSwitcherPane
            ?? throw new ArgumentNullException(nameof(characterSwitcherPane));

        this.tabSelectionController = tabSelectionController
            ?? throw new ArgumentNullException(nameof(tabSelectionController));

        this.tabSelectionController.TabSelected += OnTabSelected;

        tabs = new SelectionGrid(["Body", "Face"]);
        tabs.ControlEvent += OnTabChanged;
    }

    public enum CharacterWindowTab
    {
        Pose,
        Face,
    }

    public CharacterWindowTabPane this[CharacterWindowTab tab]
    {
        get => windowPanes[tab];
        set
        {
            windowPanes[tab] = value;

            AddPane(value);
        }
    }

    public override void Draw()
    {
        tabsPane.Draw();
        characterSwitcherPane.Draw();

        tabs.Draw();
        MpsGui.WhiteLine();

        currentTab.Draw();
    }

    public override void Activate()
    {
        base.Activate();

        tabs.SelectedItemIndex = 0;
    }

    private void OnTabSelected(object sender, TabSelectionEventArgs e)
    {
        if (e.Tab is not (Constants.Window.Pose or Constants.Window.Face))
            return;

        tabs.SelectedItemIndex = e.Tab switch
        {
            Constants.Window.Pose => 0,
            Constants.Window.Face => 1,
            _ => 0,
        };
    }

    private void OnTabChanged(object sender, EventArgs e)
    {
        var tab = tabs.SelectedItemIndex switch
        {
            0 => CharacterWindowTab.Pose,
            1 => CharacterWindowTab.Face,
            _ => CharacterWindowTab.Pose,
        };

        currentTab = windowPanes[tab];
    }
}