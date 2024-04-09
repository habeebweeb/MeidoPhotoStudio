namespace MeidoPhotoStudio.Plugin;

public class CallWindowPane : BaseMainWindowPane, IEnumerable<BasePane>
{
    public override void Draw()
    {
        tabsPane.Draw();

        foreach (var pane in Panes)
            pane.Draw();
    }

    public void Add(BasePane pane) =>
        AddPane(pane);

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
