using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Framework.UI.Legacy;

namespace MeidoPhotoStudio.Plugin.Core.UI.Legacy;

public class DragPointPane : BasePane
{
    private readonly Toggle smallCubeToggle;
    private readonly Toggle maidCubeToggle;
    private readonly PropDragHandleService propDragHandleService;
    private readonly IKDragHandleService ikDragHandleService;
    private readonly PaneHeader paneHeader;

    public DragPointPane(PropDragHandleService propDragHandleService, IKDragHandleService ikDragHandleService)
    {
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));

        paneHeader = new(Translation.Get("movementCube", "header"), true);

        smallCubeToggle = new(Translation.Get("movementCube", "small"), propDragHandleService.SmallHandle || ikDragHandleService.SmallHandle);
        smallCubeToggle.ControlEvent += OnSmallCubeToggleChanged;

        maidCubeToggle = new(Translation.Get("movementCube", "maid"), ikDragHandleService.CubeEnabled);
        maidCubeToggle.ControlEvent += OnMaidCubeToggleChanged;
    }

    public override void Draw()
    {
        paneHeader.Draw();

        if (!paneHeader.Enabled)
            return;

        GUILayout.BeginHorizontal();
        smallCubeToggle.Draw();
        maidCubeToggle.Draw();
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        paneHeader.Label = Translation.Get("movementCube", "header");
        smallCubeToggle.Label = Translation.Get("movementCube", "small");
        maidCubeToggle.Label = Translation.Get("movementCube", "maid");
    }

    private void OnSmallCubeToggleChanged(object sender, EventArgs e)
    {
        var toggle = (Toggle)sender;

        ikDragHandleService.SmallHandle = toggle.Value;
        propDragHandleService.SmallHandle = toggle.Value;
    }

    private void OnMaidCubeToggleChanged(object sender, EventArgs e) =>
        ikDragHandleService.CubeEnabled = ((Toggle)sender).Value;
}
