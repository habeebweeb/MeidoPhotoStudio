using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class BG2WindowPane : BaseMainWindowPane
{
    private static readonly string[] TabNames = { "props", "myRoom", "mod" };

    private readonly MeidoManager meidoManager;
    private readonly PropManager propManager;
    private readonly AttachPropPane attachPropPane;
    private readonly PropManagerPane propManagerPane;
    private readonly SelectionGrid propTabs;

    private BasePane currentPropsPane;

    public BG2WindowPane(MeidoManager meidoManager, PropManager propManager)
    {
        this.meidoManager = meidoManager;
        this.propManager = propManager;
        this.propManager.FromPropSelect += (_, _) =>
            propTabs.SelectedItemIndex = 0;

        // should be added in this order
        AddPane(new PropsPane(propManager));
        AddPane(new MyRoomPropsPane(propManager));
        AddPane(new ModPropsPane(propManager));

        attachPropPane = AddPane(new AttachPropPane(this.meidoManager, propManager));
        propManagerPane = AddPane(new PropManagerPane(propManager));

        propTabs = new(Translation.GetArray("propsPaneTabs", TabNames));
        propTabs.ControlEvent += (_, _) =>
            currentPropsPane = Panes[propTabs.SelectedItemIndex];

        currentPropsPane = Panes[0];
    }

    public override void Draw()
    {
        tabsPane.Draw();
        propTabs.Draw();
        MpsGui.WhiteLine();
        currentPropsPane.Draw();

        if (propTabs.SelectedItemIndex is not 0)
            return;

        propManagerPane.Draw();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        attachPropPane.Draw();
        GUILayout.EndScrollView();
    }

    public override void UpdatePanes()
    {
        if (ActiveWindow)
            base.UpdatePanes();
    }

    protected override void ReloadTranslation() =>
        propTabs.SetItems(Translation.GetArray("propsPaneTabs", TabNames));
}
