using MeidoPhotoStudio.Plugin.Core.Character.Pose;
using MeidoPhotoStudio.Plugin.Core.Props;

namespace MeidoPhotoStudio.Plugin;

public class DragPointPane : BasePane
{
    private readonly Toggle smallCubeToggle;
    private readonly Toggle maidCubeToggle;
    private readonly PropDragHandleService propDragHandleService;
    private readonly IKDragHandleService ikDragHandleService;
    private string header;

    public DragPointPane(PropDragHandleService propDragHandleService, IKDragHandleService ikDragHandleService)
    {
        this.propDragHandleService = propDragHandleService ?? throw new ArgumentNullException(nameof(propDragHandleService));
        this.ikDragHandleService = ikDragHandleService ?? throw new ArgumentNullException(nameof(ikDragHandleService));

        header = Translation.Get("movementCube", "header");

        smallCubeToggle = new(Translation.Get("movementCube", "small"), propDragHandleService.SmallHandle || ikDragHandleService.SmallHandle);
        smallCubeToggle.ControlEvent += OnSmallCubeToggleChanged;

        maidCubeToggle = new(Translation.Get("movementCube", "maid"), ikDragHandleService.CubeEnabled);
        maidCubeToggle.ControlEvent += OnMaidCubeToggleChanged;
    }

    public override void Draw()
    {
        MpsGui.Header(header);
        MpsGui.WhiteLine();

        GUILayout.BeginHorizontal();
        smallCubeToggle.Draw();
        maidCubeToggle.Draw();
        GUILayout.EndHorizontal();
    }

    protected override void ReloadTranslation()
    {
        header = Translation.Get("movementCube", "header");
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
