namespace MeidoPhotoStudio.Plugin;

public class CharacterWindowTabPane
    : BaseWindow, IEnumerable<BasePane>
{
    public override void Draw()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var pane in Panes)
            pane.Draw();

        GUILayout.EndScrollView();
    }

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public void Add(BasePane pane) =>
        AddPane(pane);
}
