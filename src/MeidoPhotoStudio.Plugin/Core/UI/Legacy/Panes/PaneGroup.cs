using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class PaneGroup : BasePane
{
    private readonly PaneHeader header;

    private HeaderGroup groupController;
    private List<SubPaneGroup> subPaneGroups;

    public PaneGroup(GUIContent headerContent, bool open = true, HeaderGroup group = null)
    {
        header = new(headerContent, open);
        header.ControlEvent += OnHeaderEnabledChanged;
        Group = group;
    }

    public PaneGroup(string label, bool open = true, HeaderGroup group = null)
        : this(new GUIContent(label ?? string.Empty), open, group)
    {
    }

    public HeaderGroup Group
    {
        get => groupController;
        set
        {
            if (groupController is not null)
                groupController.ChangedHeader -= OnHeaderEnabledChanged;

            groupController?.Deregister(header);
            groupController = value;
            groupController?.Register(header);

            if (groupController is not null)
                groupController.ChangedHeader += OnHeaderEnabledChanged;
        }
    }

    public string HeaderLabel
    {
        get => header.Label;
        set => header.Label = value ?? string.Empty;
    }

    public GUIContent HeaderContent
    {
        get => header.Content;
        set => header.Content = value;
    }

    public override void Draw()
    {
        header.Draw();

        if (header.Enabled)
            foreach (var pane in Panes)
                pane.Draw();

        GUILayout.Space(UIUtility.Scaled(3f));
    }

    public override void Add<T>(T pane)
    {
        base.Add(pane);

        if (pane is not SubPaneGroup subPaneGroup)
            return;

        subPaneGroups ??= [];
        subPaneGroups.Add(subPaneGroup);
    }

    private void OnHeaderEnabledChanged(object sender, EventArgs e)
    {
        if (Group is null)
            return;

        if (sender is not PaneHeader header || header != this.header)
            return;

        var anyOtherHeaderOpen = Group
            .Where(header => header != this.header)
            .Any(static header => header.Enabled);

        if (anyOtherHeaderOpen)
            return;

        if (subPaneGroups is null)
            return;

        var open = true;

        if (subPaneGroups.All(static subPane => subPane.Header.Enabled))
            open = false;

        foreach (var subPane in subPaneGroups)
            subPane.Header.Enabled = open;
    }
}
