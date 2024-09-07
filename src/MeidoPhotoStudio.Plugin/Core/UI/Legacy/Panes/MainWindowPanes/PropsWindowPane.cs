using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PropsWindowPane : BaseMainWindowPane, IEnumerable<BasePane>
{
    public override void Draw()
    {
        tabsPane.Draw();

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        foreach (var pane in Panes)
            pane.Draw();

        GUI.enabled = true;

        GUILayout.EndScrollView();
    }

    public void Add(BasePane pane) =>
        AddPane(pane);

    public IEnumerator<BasePane> GetEnumerator() =>
        Panes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
