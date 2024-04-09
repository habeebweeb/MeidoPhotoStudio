namespace MeidoPhotoStudio.Plugin;

public class BGWindowPane : BaseMainWindowPane, IEnumerable<BasePane>
{
    public override void Draw()
    {
        tabsPane.Draw();
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var pane in Panes)
            pane.Draw();

        GUILayout.EndScrollView();
    }

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public override void UpdatePanes()
    {
        if (ActiveWindow)
            base.UpdatePanes();
    }

    public void Add(BasePane pane) =>
        AddPane(pane);
}
